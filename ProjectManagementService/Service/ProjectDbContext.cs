using Microsoft.EntityFrameworkCore;
using ProjectManagementService.Model;

namespace ProjectManagementService.Service
{
    public interface IProjectDbContext
    {
        DbSet<Project> Projects { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class ProjectDbContext : DbContext, IProjectDbContext
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
    }
}
