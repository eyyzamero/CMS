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
    }
}