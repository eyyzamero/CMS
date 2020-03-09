using CMS.Models.Data;
using CMS.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CMS.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET Admin/Shop/Categories
        [HttpGet]
        public ActionResult Categories()
        {
            // Declaring list of categories
            List<CategoryVM> categoryVMList;

            using (DB db = new DB())
            {
                // Fetch categories from DB
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            return View(categoryVMList);
        }

        // POST Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // Declaring ID
            string id;

            using (DB db = new DB())
            {
                // Checking whether Category Title does not exist in DB
                if (db.Categories.Any(x => x.Name == catName)) return "TitleAlreadyTaken";

                // Initalizing DTO
                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;

                // Saving data in DB
                db.Categories.Add(dto);
                db.SaveChanges();

                // Fetching given ID
                id = dto.Id.ToString();
            }
            return id;
        }

        // POST Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (DB db = new DB())
            {
                // Counter
                int count = 1;

                // DTO Declaration
                CategoryDTO dto;

                // Sorting
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
            return View();
        }

        // DELETE Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (DB db = new DB())
            {
                // Get Category by ID
                CategoryDTO dto = db.Categories.Find(id);
                
                // Delete category
                db.Categories.Remove(dto);
                db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }

        // POST Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (DB db = new DB())
            {
                // Check whether the category is unique
                if (db.Categories.Any(x => x.Name == newCatName)) return "TitleAlreadyTaken";

                // Editing category
                CategoryDTO dto = db.Categories.Find(id);
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // Save changes
                db.SaveChanges();
            }
            return "OK";
        }
    }
}