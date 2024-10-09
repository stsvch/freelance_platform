using Microsoft.EntityFrameworkCore;
using ProjectManagementService.Model;

namespace ProjectManagementService.Service
{
    public class ProjectDbContext : DbContext
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
    }
}
