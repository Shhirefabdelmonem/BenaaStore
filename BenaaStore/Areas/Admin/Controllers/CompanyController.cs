using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace BenaaStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController(IUnitOfWork unitOfWork) : Controller
    {
        
        public IActionResult Index()
        {
            var CategoryList = unitOfWork.Category.GetAll().ToList();
            return View(CategoryList);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                return View(new Company());
            }
            //update
            var DbCompany = unitOfWork.Company.Get(c => c.Id == id);
            return View(DbCompany);
        }

        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    unitOfWork.Company.Add(obj);
                }
                else
                {
                    unitOfWork.Company.update(obj);
                }
                unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region Api Calls 
        [HttpGet]
        public IActionResult GetAll()
        {
            var ComapnyList= unitOfWork.Company.GetAll().ToList();
            return Json(new { data = ComapnyList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
           var DbCompany= unitOfWork.Company.Get(c => c.Id == id);
           if (DbCompany == null)
                return Json(new { success = false, message = "Error while deleting" });
         
            unitOfWork.Company.Remove(DbCompany);
            unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });


        }
        #endregion
    }
}
