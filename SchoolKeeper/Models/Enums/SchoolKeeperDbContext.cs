using Microsoft.EntityFrameworkCore;

namespace SchoolKeeper.Models.Enums;

public class SchoolKeeperDbContext : DbContext
{
    // --- Конструкторы ---
    public SchoolKeeperDbContext() { }
    public SchoolKeeperDbContext(DbContextOptions<SchoolKeeperDbContext> options) : base(options) { }

    // --- DbSet'ы ---
    public DbSet<School> Schools => Set<School>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<Rept> Reports => Set<Rept>();
    public DbSet<ReptIncident> ReptIncidents => Set<ReptIncident>();
    public DbSet<UserIncident> UserIncidents => Set<UserIncident>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
    }
}
