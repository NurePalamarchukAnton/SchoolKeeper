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
    public DbSet<ParentStudent> ParentStudents => Set<ParentStudent>();
    public DbSet<StudentTeacher> StudentTeachers => Set<StudentTeacher>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Индекс для DeviceGuid для быстрого поиска устройств по GUID
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.DeviceGuid)
            .HasDatabaseName("IX_Device_DeviceGuid");

        base.OnModelCreating(modelBuilder);
    }
}
