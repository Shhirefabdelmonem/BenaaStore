using BenaaStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BenaaStore.Controllers
{
    public class CategoryController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext context = context;

        public IActionResult Index()
        {
            var CategoryList=context.Categories.ToList();
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
                return View(category);
            
            context.Categories.Add(category);
            context.SaveChanges();
            TempData["success"] = "Category created successfullty";
            return RedirectToAction("index");
        }

        public IActionResult Edit(int ?id)
        {
            if (id==null || id ==0)
                return NotFound();
            var CategoryFromDb=context.Categories.FirstOrDefault(c=>c.CategoryId==id);
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

            context.Categories.Update(category);
            context.SaveChanges();
            TempData["success"] = "Category Updated successfullty";
            return RedirectToAction("index");
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var CategoryFromDb = context.Categories.FirstOrDefault(c => c.CategoryId == id);
            return View(CategoryFromDb);
        }
        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePost(int ?id)
        {
            var model = context.Categories.Find(id);
            if (model==null)
            {
                return NotFound();
            }
            context.Categories.Remove(model);
            context.SaveChanges();
            TempData["success"] = "Category Deleted successfullty";
            return RedirectToAction("index");
        }
    }
}
