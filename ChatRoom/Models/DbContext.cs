using Microsoft.EntityFrameworkCore;

namespace ChatRoom.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {

        }

        public DbSet<User> Usuario { get; set; }
        public DbSet<Mensaje> Mensaje { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
