using CMS.Models.Data;
using CMS.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CMS.Controllers
{
    public class ShopController : Controller
    {
        // GET Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            // Delcaring categoryVMList
            List<CategoryVM> categoryVMList;

            // List init
            using (DB db = new DB())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            return PartialView(categoryVMList);
        }

        public ActionResult Category(string name)
        {
            // Declaring productVMList
            List<ProductVM> productVMList;
            using (DB db = new DB())
            {
                // Fetch ID of category
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                // Initalizing list of products
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                // Fetching name of category
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }
            return View(productVMList);
        }

        [ActionName("Product-Details")]
        public ActionResult ProductDetails(string name)
        {
            // ProductVM and ProductDTO declaration
            ProductVM model;
            ProductDTO dto;

            // Initalizing productID
            int id = 0;

            using (DB db = new DB()) {
                // Checking whether the product does exist
                if (!db.Products.Any(x => x.Slug.Equals(name))) return RedirectToAction("Index", "Shop");

                // Initalizing productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // Fetching ID
                id = dto.Id;

                // Initalizing model
                model = new ProductVM(dto);
            }
            // Getting galery of images for selected product
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));
            return View("ProductDetails", model);
        }
    }
}