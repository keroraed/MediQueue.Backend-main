namespace MediQueue.Core.Enums;

/// <summary>
/// User role enumeration
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Admin - Full system access
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Clinic - Doctor account
    /// </summary>
    Clinic = 2,

    /// <summary>
    /// Patient - Regular user
    /// </summary>
    Patient = 3
}
