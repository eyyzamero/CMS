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
	}
}