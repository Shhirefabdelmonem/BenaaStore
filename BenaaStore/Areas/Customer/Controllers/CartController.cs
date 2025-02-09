using BenaaStore.DataAccess.Repository;
using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models.Models;
using BenaaStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;
using System.Security.Claims;

namespace BenaaStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController(IUnitOfWork unitOfWork) : Controller
    {
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

            shopingCartVMs.OrderHeader.ApplicationUser=unitOfWork.ApplicationUser.Get(u=>u.Id==userId);
            shopingCartVMs.OrderHeader.Name= shopingCartVMs.OrderHeader.ApplicationUser.Name;
            shopingCartVMs.OrderHeader.PhoneNumber = shopingCartVMs.OrderHeader.ApplicationUser.PhoneNumber;
            shopingCartVMs.OrderHeader.PostalCode = shopingCartVMs.OrderHeader.ApplicationUser.PostalCode;
            shopingCartVMs.OrderHeader.State = shopingCartVMs.OrderHeader.ApplicationUser.State;
            shopingCartVMs.OrderHeader.City = shopingCartVMs.OrderHeader.ApplicationUser.City;

            foreach (var cart in shopingCartVMs.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shopingCartVMs.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(shopingCartVMs);
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
