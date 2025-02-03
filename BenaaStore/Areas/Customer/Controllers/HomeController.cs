using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        public IActionResult Details(int id)
        {
            var Product = unitOfWork.Product.Get(p => p.Id == id, includeProp: "Category");
            return View(Product); 
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
