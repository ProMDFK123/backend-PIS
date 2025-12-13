using Microsoft.EntityFrameworkCore;
using bolsafeucn_back.src.models;

namespace bolsafeucn_back.src.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}
