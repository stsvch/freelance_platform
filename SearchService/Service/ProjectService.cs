using Microsoft.EntityFrameworkCore;

namespace SearchService.Service
{
    public class ProjectService
    {
        private readonly ProjectDbContext _context;

        public ProjectService(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<List<Model.Project>> SearchProjects(string searchTerm)
        {
            return await _context.Projects
                .Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();
        }

        // Добавьте дополнительные методы, если необходимо
    }
}
