using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using BenaaStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BenaaStore.Areas.Admin.Controllers
{
    [Area("Admin")]
   // [Authorize(SD.Role_Admin)]
    public class CategoryController(IUnitOfWork unitOfWork) : Controller
    {


        public IActionResult Index()
        {
            var CategoryList = unitOfWork.Category.GetAll();
            return View(CategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order Cannot exactly match the Name");
            }
            if (!ModelState.IsValid)
                return View();

            unitOfWork.Category.Add(category);
            unitOfWork.Save();
            TempData["success"] = "Category created successfullty";
            return RedirectToAction("index");
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var CategoryFromDb = unitOfWork.Category.Get(c => c.CategoryId == id);
            return View(CategoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {

            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order Cannot exactly match the Name");
            }
            if (!ModelState.IsValid)
                return View(category);

            unitOfWork.Category.Update(category);
            unitOfWork.Save();
            TempData["success"] = "Category Updated successfullty";
            return RedirectToAction("index");
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var CategoryFromDb = unitOfWork.Category.Get(c => c.CategoryId == id);
            return View(CategoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var model = unitOfWork.Category.Get(c => c.CategoryId == id);
            if (model == null)
            {
                return NotFound();
            }
            unitOfWork.Category.Remove(model);
            unitOfWork.Save();
            TempData["success"] = "Category Deleted successfullty";
            return RedirectToAction("index");
        }
    }
}
