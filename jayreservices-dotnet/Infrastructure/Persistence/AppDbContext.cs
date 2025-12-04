using jayreservices_dotnet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace jayreservices_dotnet.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext (options)
    {
        public DbSet<User> Users { get; set; }
    }
}
