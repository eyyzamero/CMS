using System.Data.Entity;

namespace CMS.Models.Data
{
    public class DB: DbContext
    {
        public DbSet<PageDTO> Pages { get; set; }
    }
}