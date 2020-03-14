using CMS.Areas.Admin.Models.ViewModels.Shop;
using CMS.Models.Data;
using CMS.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
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

        // PRODUCTS

        //GET Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Initialize model
            ProductVM model = new ProductVM();

            // Fetch list of categories
            using (DB db = new DB())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            return View(model);
        }

        //POST Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Checking model's state
            if (!ModelState.IsValid)
            {
                using (DB db = new DB())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // Checking whether the product's name is unique
            using (DB db = new DB())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "This product name is already taken!");
                    return View(model);
                }
            }

            // Declaring product ID
            int id;
            using (DB db = new DB())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO category = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = category.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            // Set TempData 
            TempData["SM"] = "You have added product successfully!";

            // Image declaring
            #region Upload Image

            // Creating required folder structure
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            if (file != null && file.ContentLength > 0)
            {
                // Checks if file extensions are valid
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" && ext != "image/x-png" && ext != "image/png")
                {
                    using (DB db = new DB())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenie obrazu.");
                        return View(model);
                    }
                }

                // Initialize Image name
                string imageName = file.FileName;

                // Save Image Name to DB
                using (DB db = new DB())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                // Saving original image
                file.SaveAs(path);

                // Saving Thumbnail
                WebImage img = new WebImage(file.InputStream);
                img.Resize(100, 100);
                img.Save(path2);
            }
            #endregion
            return RedirectToAction("AddProduct");
        }

        // GET Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            // Declaring list of products
            List<ProductVM> listOfProductsVM;

            // Declaring number of page
            var pageNumber = page ?? 1;
            using (DB db = new DB())
            {
                // Initalizing list of products
                listOfProductsVM = db.Products.ToArray().Where(x => catId == null || catId == 0 || x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                // List of categories to dropdownLstt
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Set declared category
                ViewBag.SelectedCat = catId.ToString();
            }

            // Pagination
            var onePageOfProducts = listOfProductsVM.ToPagedList(pageNumber, 5);
            ViewBag.onePageOfProducts = onePageOfProducts;

            // Return view with list of products
            return View(listOfProductsVM);
        }

        // GET Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // ProductVM Model Assignment;
            ProductVM model;
            using (DB db = new DB())
            {
                // Fetching product to then edit it
                ProductDTO dto = db.Products.Find(id);

                // Checking if product does exist
                if (dto == null)
                {
                    return Content("This product does not exist!");
                }

                // Initializing model
                model = new ProductVM(dto);

                // List of categories
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Setting up images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                .Select(fn => Path.GetFileName(fn));
            }
            return View(model);
        }

        // POST Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Get product ID;
            int id = model.Id;

            // Fetching categories to dropdown list
            using (DB db = new DB())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            // Setting up images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                            .Select(fn => Path.GetFileName(fn));

            // Checking model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // sprawdzenie unikalnosci nazwy produktu
            using (DB db = new DB())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "This product name is already taken!");
                    return View(model);
                }
            }

            // Edycja produktu i zapis na bazie
            using (DB db = new DB())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;
                db.SaveChanges();
            }

            // TempData Assignment
            TempData["SM"] = "Edytowałeś produkt";

            #region Image Upload
            // Checking if file does exist
            if (file != null && file.ContentLength > 0)
            {
                // Checking file extension whether it is an image
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" && ext != "image/x-png" && ext != "image/png")
                {
                    using (DB db = new DB())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Invalid image extension. Action aborted");
                        return View(model);
                    }
                }

                // Creating required structure of folders
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // Deleting old files from folders
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                    file2.Delete();
                foreach (var file3 in di2.GetFiles())
                    file3.Delete();

                // Saving new image in DB
                string imageName = file.FileName;

                using (DB db = new DB())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                // Zapis nowych plików
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);
                WebImage img = new WebImage(file.InputStream);
                img.Resize(100, 100);
                img.Save(path2);
            }
            #endregion
            return RedirectToAction("EditProduct");
        }

        // GET: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            // Deleting products from DB
            using (DB db = new DB())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            // Deleting folder with all images for provided product ID
            var orginalDirector = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString = Path.Combine(orginalDirector.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            return RedirectToAction("Products");
        }

        // POST Admin/Shop/SaveGalleryImage/ID
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Looping over images
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];
                // Checking if file exists and it's not empty
                if (file != null && file.ContentLength > 0)
                {
                    // Setting routes 
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                    // Images are thrown to below path
                    string ToGallery = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    // Thumbnails are thrown to below path
                    string ToGalleryThumbs = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var pathGallery = string.Format("{0}\\{1}", ToGallery, file.FileName);
                    var pathgalleryThumbs = string.Format("{0}\\{1}", ToGalleryThumbs, file.FileName);

                    // Saving Galler Image 
                    file.SaveAs(pathGallery);

                    // Saving Thumbnails
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(100, 100);
                    img.Save(pathgalleryThumbs);
                }
            }
        }

        // POST Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            // Declaring folder structure
            string ToGallery = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string ToGalleryThumbs = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            // Checking whether data does exist and deleting it
            if (System.IO.File.Exists(ToGallery)) System.IO.File.Delete(ToGallery);
            if (System.IO.File.Exists(ToGalleryThumbs)) System.IO.File.Delete(ToGalleryThumbs);
        }

        // GET Admin/Shop/Orders
        [HttpGet]
        public ActionResult Orders()
        {
            // Initialize OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdminVM = new List<OrdersForAdminVM>();

            using (DB db = new DB())
            {
                // Getting orders
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                foreach (var order in orders)
                {
                    // Initialize dictionary of products
                    Dictionary<string, int> productsAndQuantity = new Dictionary<string, int>();

                    decimal total = 0m;

                    // Initialize ordersDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Get user
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.UserName;

                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get price of the product
                        decimal price = product.Price;

                        // Get name of product
                        string productName = product.Name;

                        // Add product to the dictionary
                        productsAndQuantity.Add(productName, orderDetails.Quantity);

                        // Set total value
                        total += orderDetails.Quantity * price;
                    }

                    ordersForAdminVM.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQuantity = productsAndQuantity,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            return View(ordersForAdminVM);
        }
    }
}