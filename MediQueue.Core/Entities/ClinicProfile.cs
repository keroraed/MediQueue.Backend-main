namespace MediQueue.Core.Entities;

/// <summary>
/// Clinic Profile - One clinic account = one doctor
/// </summary>
public class ClinicProfile
{
    public int Id { get; set; }
    public required string AppUserId { get; set; }
    public required string DoctorName { get; set; }
    public required string Specialty { get; set; }
    public required string Description { get; set; }
    public int SlotDurationMinutes { get; set; } = 30;
    public string? ProfilePictureUrl { get; set; }
    /// <summary>Doctor's consultation fee per appointment.</summary>
    public decimal? ConsultationFee { get; set; }
    /// <summary>Comma-separated accepted payment methods e.g. "VodafoneCash,InstaPay,Visa"</summary>
    public string? PaymentMethods { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ClinicAddress? Address { get; set; }
    public ICollection<ClinicPhone> Phones { get; set; } = new List<ClinicPhone>();
    public ICollection<ClinicWorkingDay> WorkingDays { get; set; } = new List<ClinicWorkingDay>();
    public ICollection<ClinicException> Exceptions { get; set; } = new List<ClinicException>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ClinicRating> Ratings { get; set; } = new List<ClinicRating>();
}
