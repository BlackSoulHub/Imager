using Imager.Database.Configuration;
using Imager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Imager.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        // TODO: Change to migrations?
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new FriendListEntityConfiguration());
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ImageEntity> Images { get; set; }
}