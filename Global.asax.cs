using CMS.Models.Data;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CMS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest()
        {
            if(User == null) return;
            
            // Fetching user name
            string username = Context.User.Identity.Name;

            // Declaring roles array
            string[] Roles = null;
            using (DB db = new DB())
            {
                // Fetching user data
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);
                Roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();
            }

            //Creating IPrincipal object
            IIdentity userIdentity = new GenericIdentity(username);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, Roles);

            // Update context user
            Context.User = newUserObj;
        }
    }
}
