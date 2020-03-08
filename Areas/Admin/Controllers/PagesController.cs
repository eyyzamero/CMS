using CMS.Models.Data;
using CMS.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CMS.Areas.Admin.Controllers
{
	public class PagesController : Controller
	{
		// GET: Admin/Pages
		public ActionResult Index()
		{
			// Declaring list of PageVM
			List<PageVM> PagesList;

			using (DB db = new DB())
			{
				// Initializing list
				PagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
			}

			// Return pages to view
			return View(PagesList);
		}

		// GET Admin/Pages/AddPage
		[HttpGet]
		public ActionResult AddPage() {

			return View();
		}

		// POST Admin/Pages/AddPage
		[HttpPost]
		public ActionResult AddPage(PageVM model)
		{
			// Check whether form is valid
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			using (DB db = new DB())
			{
				string slug;
				// Initializing PageDTO
				PageDTO dto = new PageDTO();

				// When page URL is undefined Page title is assigned to Slug
				if (string.IsNullOrWhiteSpace(model.Slug))
				{
					slug = model.Title.Replace(" ", "-").ToLower();
				} else
				{
					slug = model.Slug.Replace(" ", "-").ToLower();
				}

				// Prevent adding same page
				if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
				{
					ModelState.AddModelError("", "Your Page Title or Page URL is already taken");
					return View(model);
				}

				dto.Title = model.Title;
				dto.Slug = slug;
				dto.Body = model.Body;
				dto.Sorting = 1000;
				dto.HasSidebar = model.HasSidebar;

				// Saving DTO
				db.Pages.Add(dto);
				db.SaveChanges();
			}
			TempData["SM"] = "You have added a new Page!";

			return RedirectToAction("AddPage");
		}
	}
}