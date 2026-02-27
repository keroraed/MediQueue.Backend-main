using Microsoft.EntityFrameworkCore;
using MediQueue.Core.Entities;

namespace MediQueue.Repository.Data;

public class StoreContext : DbContext
{
    public StoreContext(DbContextOptions<StoreContext> options) : base(options)
    {
    }

    // DbSets for MediQueue Entities
    public DbSet<User> Users { get; set; }
    public DbSet<ClinicProfile> ClinicProfiles { get; set; }
    public DbSet<ClinicAddress> ClinicAddresses { get; set; }
    public DbSet<ClinicPhone> ClinicPhones { get; set; }
    public DbSet<ClinicWorkingDay> ClinicWorkingDays { get; set; }
    public DbSet<ClinicException> ClinicExceptions { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<ClinicRating> ClinicRatings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
     base.OnModelCreating(modelBuilder);
        
        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
    entity.HasKey(e => e.Id);
         entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Email).IsUnique();
  });

    // ClinicProfile Configuration
        modelBuilder.Entity<ClinicProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
        entity.Property(e => e.AppUserId).IsRequired().HasMaxLength(450);
        entity.Property(e => e.DoctorName).IsRequired().HasMaxLength(200);
         entity.Property(e => e.Specialty).IsRequired().HasMaxLength(200);
  entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(e => e.ConsultationFee).HasColumnType("decimal(10,2)");
            entity.Property(e => e.PaymentMethods).HasMaxLength(200);
      
            // Note: No navigation to AppUser since it's in a different DbContext (Identity)
    
            entity.HasIndex(e => e.AppUserId).IsUnique();
            entity.HasIndex(e => e.Specialty);
        });

     // ClinicAddress Configuration
        modelBuilder.Entity<ClinicAddress>(entity =>
    {
        entity.HasKey(e => e.Id);
         entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
  entity.Property(e => e.City).IsRequired().HasMaxLength(100);
     entity.Property(e => e.Area).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Street).IsRequired().HasMaxLength(200);
      entity.Property(e => e.Building).IsRequired().HasMaxLength(50);
    entity.Property(e => e.Notes).HasMaxLength(500);
            
entity.HasOne(a => a.Clinic)
   .WithOne(c => c.Address)
          .HasForeignKey<ClinicAddress>(a => a.ClinicId)
   .OnDelete(DeleteBehavior.Cascade);
           
          entity.HasIndex(e => e.City);
     });

   // ClinicPhone Configuration
        modelBuilder.Entity<ClinicPhone>(entity =>
        {
 entity.HasKey(e => e.Id);
        entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
        
entity.HasOne(p => p.Clinic)
             .WithMany(c => c.Phones)
      .HasForeignKey(p => p.ClinicId)
 .OnDelete(DeleteBehavior.Cascade);
    });

        // ClinicWorkingDay Configuration
        modelBuilder.Entity<ClinicWorkingDay>(entity =>
        {
       entity.HasKey(e => e.Id);
  entity.Property(e => e.DayOfWeek).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
    
      entity.HasOne(w => w.Clinic)
     .WithMany(c => c.WorkingDays)
    .HasForeignKey(w => w.ClinicId)
        .OnDelete(DeleteBehavior.Cascade);
     
          entity.HasIndex(e => new { e.ClinicId, e.DayOfWeek }).IsUnique();
      });

        // ClinicException Configuration
 modelBuilder.Entity<ClinicException>(entity =>
        {
        entity.HasKey(e => e.Id);
            entity.Property(e => e.ExceptionDate).IsRequired();
         entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
       
            entity.HasOne(ex => ex.Clinic)
           .WithMany(c => c.Exceptions)
 .HasForeignKey(ex => ex.ClinicId)
     .OnDelete(DeleteBehavior.Cascade);
     
            entity.HasIndex(e => new { e.ClinicId, e.ExceptionDate });
  });

        // Appointment Configuration
        modelBuilder.Entity<Appointment>(entity =>
  {
            entity.HasKey(e => e.Id);
   entity.Property(e => e.AppointmentDate).IsRequired();
      entity.Property(e => e.QueueNumber).IsRequired();
        entity.Property(e => e.Status).IsRequired();
   
            entity.HasOne(a => a.Clinic)
    .WithMany(c => c.Appointments)
         .HasForeignKey(a => a.ClinicId)
     .OnDelete(DeleteBehavior.Restrict);
    
            // Note: Patient relationship removed - PatientId now references AppUser (Identity) string ID
            // The navigation is not available since it crosses contexts
    
            entity.HasIndex(e => new { e.ClinicId, e.AppointmentDate, e.QueueNumber }).IsUnique();
       entity.HasIndex(e => new { e.PatientId, e.AppointmentDate });
entity.HasIndex(e => e.Status);
  });

        // ClinicRating Configuration
        modelBuilder.Entity<ClinicRating>(entity =>
        {
       entity.HasKey(e => e.Id);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Review).HasMaxLength(1000);
  
         entity.HasOne(r => r.Clinic)
   .WithMany(c => c.Ratings)
         .HasForeignKey(r => r.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);
      
         // Note: Patient relationship removed - PatientId now references AppUser (Identity) string ID
         // The navigation is not available since it crosses contexts
  
  entity.HasIndex(e => new { e.ClinicId, e.PatientId }).IsUnique();
        entity.HasIndex(e => e.Rating);
        });
    }
}
