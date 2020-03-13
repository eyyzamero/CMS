using CMS.Models.Data;
using CMS.Models.ViewModels.Account;
using CMS.Views.Account;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace CMS.Controllers
{
    public class AccountController : Controller
    {
        // GET: Accountt
        public ActionResult Index()
        {
            return RedirectToAction("~/Account/Login");
        }

        // GET Account/Login
        public ActionResult Login()
        {
            // Checking wheter user is not already logged in
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName)) return RedirectToAction("User-Profile");

            return View();
        }

        // POST Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Checking model State
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Checking user data
            bool isValid = false;
            using (DB db = new DB())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password))) {
                    isValid = true;

                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Username or Password is invalid");
                return View(model);
            } else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }
        }

        // GET Account/Create-Account
        [HttpGet]
        [ActionName("Create-Account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST Account/Create-Account
        [HttpPost]
        [ActionName("Create-Account")]
        public ActionResult CreateAccount(UserVM model)
        {
            // Checking model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // Does the password match?
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match!");
                return View("CreateAccount", model);
            }

            using (DB db = new DB())
            {
                // Checking whether username is unique
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "User with provided username does already exist!");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                // Creating user's account
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    UserName = model.UserName,
                    Password = model.Password
                };

                // Adding new user to DB
                db.Users.Add(userDTO);
                db.SaveChanges();


                // Role assignment for newly created user
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = userDTO.Id,
                    RoleId = 1 // 1 = NORMAL USER, 2 = ADMIN
                };

                // Pushing data to DB
                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }
            // TempData dialog
            TempData["SM"] = "You are now registered in our service! You can now login to your account";

            return Redirect("~/Account/Login");
        }

        // GET /Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/Account/Login");
        }

        public ActionResult UserNavPartial()
        {
            // Fetching username
            string username = User.Identity.Name;

            // Declaring model
            UserNavPartialVM model;
            using(DB db = new DB())
            {
                // Get user from DB
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            return PartialView(model);
        }

        // Get Account/User-Profile
        [ActionName("User-Profile")]
        [HttpGet]
        public ActionResult UserProfile()
        {
            // Fetching username
            string username = User.Identity.Name;

            // Declaring model
            UserProfileVM model;

            using (DB db = new DB())
            {
                // Getting user
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);
                model = new UserProfileVM(dto);
            }
            return View("UserProfile", model);
        }

        [ActionName("User-Profile")]
        [HttpPost]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // Checking model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Validating mail
            if (!string.IsNullOrEmpty(model.ConfirmPassword))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Password do not match");
                    return View("UserProfile", model);
                }
            }

            using (DB db = new DB())
            {
                // Fetching user's name
                string username = User.Identity.Name;

                // Checking whether user name is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == username))
                {
                    ModelState.AddModelError("", "User Name" + model.UserName + " is already taken!");
                    model.UserName = "";
                    return View("UserProfile", model);
                }

                // Editing DTO
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    dto.Password = model.Password;
                }

                db.SaveChanges();
            }
            // Tempdata return message
            TempData["SM"] = "You have editted your profile successfully!";

            return Redirect("~/Account/User-Profile");
        }
    }
}