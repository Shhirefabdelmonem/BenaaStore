
using BenaaStore.DataAccess.Repository;
using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using BenaaStore.Models.Models;
using BenaaStore.Models.ViewModels;
using BenaaStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol.Plugins;


namespace BenaaStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(SD.Role_Admin)]
    public class ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment) : Controller
    {

        public IActionResult Index()
        {
            var ProductList = unitOfWork.Product.GetAll(includeProp:"Category");
            return View(ProductList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productViewModel = new()
            {
                CategoryList = unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value=u.CategoryId.ToString()
                }),
                Product = new Product()
            }; 
            if (id == 0 || id == null)
            {
                //create
                return View(productViewModel);
            }

                //update
                productViewModel.Product = unitOfWork.Product.Get(p=>p.Id == id);
                return View(productViewModel);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productViewModel, IFormFile? file)
        {
            
            if (!ModelState.IsValid)
            {
                productViewModel.CategoryList = unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString()
                });
                    
                
                return View(productViewModel);
            }
            
            string wwwRootPath = webHostEnvironment.WebRootPath;// gives wwwroot path
            if (file != null)
            {
                string fileName=Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);// fileName + extention
                string productPath=Path.Combine(wwwRootPath, @"images\product");// full file Location
                
                if (!string.IsNullOrEmpty(productViewModel.Product.ImageUrl))
                {
                    //delete old image
                    var oldImagePath=Path.Combine(wwwRootPath, productViewModel.Product.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                using (var fileStream=new FileStream(Path.Combine(productPath,fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                productViewModel.Product.ImageUrl=@"\images\product\" + fileName;

            }



            if (productViewModel.Product.Id == 0)
            {
                unitOfWork.Product.Add(productViewModel.Product);
                TempData["success"] = "Product created successfullty";
            }
            else
            {
                unitOfWork.Product.Update(productViewModel.Product);
                TempData["success"] = "Product updated  successfullty";
            }
           
            unitOfWork.Save();
            
           
            return RedirectToAction("index");
        }


        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var ProductList = unitOfWork.Product.GetAll(includeProp: "Category");
            return Json(new {data=ProductList});

        }
        
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToDelete = unitOfWork.Product.Get(x => x.Id == id);
            if (productToDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var oldImagePath =
                           Path.Combine(webHostEnvironment.WebRootPath,
                           productToDelete.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            unitOfWork.Product.Remove(productToDelete);
            unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }



        #endregion
    }
}
