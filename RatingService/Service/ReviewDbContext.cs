using Microsoft.EntityFrameworkCore;
using RatingService.Model;

namespace RatingService.Service
{
    public class ReviewDbContext : DbContext
    {
        public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options)
        {
        }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<Response> Responses { get; set; }
    }
}
