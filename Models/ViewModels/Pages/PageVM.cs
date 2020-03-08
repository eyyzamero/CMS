using CMS.Models.Data;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models.ViewModels.Pages
{
    public class PageVM
    {
        public PageVM()
        {

        }

        public PageVM(PageDTO row)
        {
            Id = row.Id;
            Title = row.Title;
            Slug = row.Slug;
            Body = row.Body;
            Sorting = row.Sorting;
            HasSidebar = row.HasSidebar;
        }

        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Page Title")]
        public string Title { get; set; }
        [Display(Name = "Page URL")]
        public string Slug { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 3)]
        [Display(Name = "Page Content")]
        public string Body { get; set; }
        public int Sorting { get; set; }
        [Display(Name = "Does it have sidebar?")]
        public bool HasSidebar { get; set; }
    }
}