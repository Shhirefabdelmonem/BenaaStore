
using BenaaStore.DataAccess.Repository;
using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using BenaaStore.Models.Models;
using BenaaStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol.Plugins;


namespace BenaaStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment) : Controller
    {

        public IActionResult Index()
        {
            var ProductList = unitOfWork.Product.GetAll();
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

            string wwwRootPath = webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName=Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath=Path.Combine(wwwRootPath, @"images\product");
                
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
            }
            else
            {
                unitOfWork.Product.Update(productViewModel.Product);
            }
           
            unitOfWork.Save();
            TempData["success"] = "Product created successfullty";
            return RedirectToAction("index");
        }


        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var ProductFromDb = unitOfWork.Product.Get(c => c.Id == id);
            return View(ProductFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var model = unitOfWork.Product.Get(c => c.Id == id);
            if (model == null)
            {
                return NotFound();
            }
            unitOfWork.Product.Remove(model);
            unitOfWork.Save();
            TempData["success"] = "Product Deleted successfullty";
            return RedirectToAction("index");
        }



    }
}
