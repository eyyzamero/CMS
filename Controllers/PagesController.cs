using CMS.Models.Data;
using CMS.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.Controllers
{
    public class PagesController : Controller
    {
        // GET Index/{page}
        public ActionResult Index(string page = "")
        {
            // Setting URL
            if (page == "") page = "home";

            // Declaring PageVM and PageDTO
            PageVM model;
            PageDTO dto;

            // Checking whether the site does exist
            using (DB db = new DB())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page))) return RedirectToAction("Index", new { page = "" });
            }

            // Fetching pageDTO
            using (DB db = new DB())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            // Updating page Title
            ViewBag.PageTitle = dto.Title;

            // Check whether the site has sidebar
            if (dto.HasSidebar == true) ViewBag.Sidebar = true;
             else ViewBag.Sidebar = false;

            // PageVM init
            model = new PageVM(dto);

            return View(model);
        }

        public ActionResult PageMenuPartial() {
            // Declaring PageVM
            List<PageVM> pageVMList;

            // Fetching pages
            using (DB db = new DB())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }
            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            // Declaring model
            SidebarVM model;

            // Model initialization
            using (DB db = new DB())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                model = new SidebarVM(dto);
            }

            return PartialView(model);
        }
    }
}