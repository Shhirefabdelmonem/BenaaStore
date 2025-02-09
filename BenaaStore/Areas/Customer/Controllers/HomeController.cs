using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using BenaaStore.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Permissions;
using Microsoft.AspNetCore.Identity;

namespace BenaaStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork _unitOfWork)
        {
            _logger = logger;
            unitOfWork = _unitOfWork;
        }

        public IActionResult Index()
        {
            var ProductList = unitOfWork.Product.GetAll(includeProp: "Category");
            return View(ProductList);
        }

        public IActionResult Details(int ProductId)
        {
            ShoppingCart Cart = new()
            {
                Product = unitOfWork.Product.Get(p => p.Id == ProductId, includeProp: "Category"),
                ProductId = ProductId,
                Count = 1
            };
            return View(Cart);
        }
        public ApplicationUser ApplicationUser { get; set; }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            // Adds a product to the user's shopping cart and associates it with the authenticated user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId= claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.ApplicationUserId = UserId;

            var cartDb = unitOfWork.ShoppingCart.Get(c => c.ProductId == cart.ProductId && c.ApplicationUserId == cart.ApplicationUserId);
            if (cartDb != null)
            {
                cartDb.Count += cart.Count;
                unitOfWork.ShoppingCart.update(cartDb);
            }
            else
            {
                unitOfWork.ShoppingCart.Add(cart);
            }

            TempData["success"] = "Cart updated successfully";
            unitOfWork.Save();

            return RedirectToAction(nameof(Index));


        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
