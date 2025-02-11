using BenaaStore.DataAccess.Repository;
using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models.Models;
using BenaaStore.Models.ViewModels;
using BenaaStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe.Terminal;
using System.Drawing.Text;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace BenaaStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController(IUnitOfWork unitOfWork) : Controller
    {
        [BindProperty]
        public ShopingCartVM shopingCartVMs { get; set; }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId= claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


             shopingCartVMs = new()
            {
                ShoppingCartList=unitOfWork.ShoppingCart.GetAll(s=>s.ApplicationUserId == userId,includeProp:"Product"),
                 OrderHeader = new()
             };

            foreach (var cart in shopingCartVMs.ShoppingCartList)
            {
                cart.Price= GetPriceBasedOnQuantity(cart);
                shopingCartVMs.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
                  
            return View(shopingCartVMs);
        }

        private double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            if (cart.Count <= 50)
                return cart.Product.Price;
            else if (cart.Count <= 100)
                return cart.Product.Price50;
            else 
                return cart.Product.Price100;
        }

        public IActionResult Summary()
        {
            var claimsIdentity= (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shopingCartVMs = new()
            {
                ShoppingCartList = unitOfWork.ShoppingCart.GetAll(s=>s.ApplicationUserId == userId,includeProp:"Product"),
                OrderHeader = new ()
            };

            // var applicationUser instead of shopingCartVMs.OrderHeader.ApplicationUser
            shopingCartVMs.OrderHeader.ApplicationUser=unitOfWork.ApplicationUser.Get(u=>u.Id==userId);

            shopingCartVMs.OrderHeader.Name= shopingCartVMs.OrderHeader.ApplicationUser.Name;
            shopingCartVMs.OrderHeader.PhoneNumber = shopingCartVMs.OrderHeader.ApplicationUser.PhoneNumber;
            shopingCartVMs.OrderHeader.PostalCode = shopingCartVMs.OrderHeader.ApplicationUser.PostalCode;
            shopingCartVMs.OrderHeader.State = shopingCartVMs.OrderHeader.ApplicationUser.State;
            shopingCartVMs.OrderHeader.City = shopingCartVMs.OrderHeader.ApplicationUser.City;
            shopingCartVMs.OrderHeader.StreetAddress = shopingCartVMs.OrderHeader.ApplicationUser.StreetAddress;

            foreach (var cart in shopingCartVMs.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shopingCartVMs.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(shopingCartVMs);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shopingCartVMs.ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
               includeProp: "Product");

            shopingCartVMs.OrderHeader.ApplicationUserId = userId;
            var applicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            shopingCartVMs.OrderHeader.OrderDate = System.DateTime.Now;

   
            foreach (var cart in shopingCartVMs.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shopingCartVMs.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                shopingCartVMs.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shopingCartVMs.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //it is a company user
                shopingCartVMs.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                shopingCartVMs.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            unitOfWork.OrderHeader.Add(shopingCartVMs.OrderHeader);
            unitOfWork.Save();
            
            foreach(var cart in shopingCartVMs.ShoppingCartList)
            {
                var OrderDetail = new OrderDetails()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shopingCartVMs.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                unitOfWork.OrderDetails.Add(OrderDetail);
                
                unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                //stripe logic

                var domain = "https://localhost:7216/";

                var options = new SessionCreateOptions
                {
                    SuccessUrl= domain+ $"Customer/Cart/OrderConfirmation/{shopingCartVMs.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems=new List<SessionLineItemOptions>(),
                    Mode="payment",


                };

                foreach(var cart in shopingCartVMs.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount= (long)(cart.Price*100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = cart.Product.Title
                            }
                        },
                        Quantity = cart.Count

                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session =service.Create(options);

                // store sessionId in database OderHeaderTable to retrive session content later 
                // only sessionId will be updated
                unitOfWork.OrderHeader.UpdateStripePaymentID(shopingCartVMs.OrderHeader.Id, session.Id, session.PaymentIntentId); ;
                unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = shopingCartVMs.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeaderDb = unitOfWork.OrderHeader.Get(u => u.Id == id, includeProp: "ApplicationUser");
            if (orderHeaderDb.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by customer
                var service = new SessionService();
                var session = service.Get(orderHeaderDb.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    // here paymentIntendId only Updated if payment completd successfully
                    unitOfWork.OrderHeader.UpdateStripePaymentID(id ,session.Id,session.PaymentIntentId);

                    unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    unitOfWork.Save();
                }
            }

            var shoppingCartList = unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == orderHeaderDb.ApplicationUserId).ToList();
            unitOfWork.ShoppingCart.RemoveRange(shoppingCartList);
            unitOfWork.Save();

            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            unitOfWork.ShoppingCart.update(cartFromDb);
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                //remove that from cart
                unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                unitOfWork.ShoppingCart.update(cartFromDb);
            }
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            unitOfWork.ShoppingCart.Remove(cartFromDb);
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

    }
}
