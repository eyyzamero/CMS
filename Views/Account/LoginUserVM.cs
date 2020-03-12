using System.ComponentModel.DataAnnotations;

namespace CMS.Views.Account
{
    public class LoginUserVM
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}