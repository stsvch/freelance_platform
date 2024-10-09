using Microsoft.EntityFrameworkCore;
using SearchService.Model;

namespace SearchService.Service
{
    public class ProjectDbContext : DbContext
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
    }
}
