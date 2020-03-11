using CMS.Models.Data;
using CMS.Models.ViewModels.Shop;
using System.Collections.Generic;
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
    }
}