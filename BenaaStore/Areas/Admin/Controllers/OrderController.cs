using BenaaStore.DataAccess.Repository;
using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models.Models;
using BenaaStore.Models.ViewModels;
using BenaaStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BenaaStore.Areas.Admin.Controllers
{
    [Area("admin")]
    public class OrderController(IUnitOfWork unitOfWork) : Controller
    {
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId )
        {
            OrderVM orderVM = new()
            {
                orderHeader = unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProp: "ApplicationUser"),
                orderDetails = unitOfWork.OrderDetails.GetAll(u => u.OrderHeaderId == orderId, includeProp: "Product")
            };
            return View(orderVM);
            
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.orderHeader.PhoneNumber; 
            orderHeaderFromDb.StreetAddress = OrderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.orderHeader.City;
            orderHeaderFromDb.State = OrderVM.orderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderHeader.TrackingNumber;
            }
            unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
           unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id,SD.StatusInProcess);
           unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }

        public IActionResult ShipOrder()
        {
            var orderHeader= unitOfWork.OrderHeader.Get(u=>u.Id==OrderVM.orderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.orderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.orderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment) 
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            unitOfWork.OrderHeader.Update(orderHeader);
            unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader= unitOfWork.OrderHeader.Get(u=>u.Id == OrderVM.orderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions()
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent= orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                var refund =service.Create(options);
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            unitOfWork.Save();

            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.orderHeader = unitOfWork.OrderHeader.Get(u=>u.Id==OrderVM.orderHeader.Id,includeProp: "ApplicationUser");
            OrderVM.orderDetails=unitOfWork.OrderDetails.GetAll(u=>u.Id== OrderVM.orderHeader.Id,includeProp: "Product");

            // stripe Logic 

            var domain = "https://localhost:7216/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"

            };

            
            foreach(var item in OrderVM.orderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData= new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency ="usd",
                        ProductData=new SessionLineItemPriceDataProductDataOptions
                        {
                            Name=item.Product.Title
                        }

                    },
                    Quantity=item.Count
                    

                }; 
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            var session= service.Create(options);
            
            unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.orderHeader.Id,session.Id,session.PaymentLinkId);
            unitOfWork.Save();
            Response.Headers.Add("Location",session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            var orderHeader = unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company
               
                var service =new SessionService();
                var session=service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id,session.PaymentLinkId);
                    unitOfWork.OrderHeader.UpdateStatus(orderHeaderId,session.PaymentStatus,SD.PaymentStatusApproved);
                }

            }
            return View(orderHeaderId);
        }

                #region Api Calls
                [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = unitOfWork.OrderHeader.GetAll(includeProp: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity=  (ClaimsIdentity)User.Identity;
                var userId= claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProp: "ApplicationUser");
            }
         

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment).ToList();
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess).ToList();
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped).ToList();
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved).ToList();
                    break;
                default:
                    break;
            }
            return Json(new { data = objOrderHeaders });

            

        }

        #endregion

    }
}
