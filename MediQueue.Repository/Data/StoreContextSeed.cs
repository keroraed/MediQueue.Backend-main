using MediQueue.Core.Entities;
using Microsoft.Extensions.Logging;

namespace MediQueue.Repository.Data;

public class StoreContextSeed
{
    public static async Task SeedAsync(StoreContext context)
    {
        try
        {
        // No default seed data for clinic system
        // Clinics will be created by users through registration
  await Task.CompletedTask;
        }
        catch (Exception)
{
// Log error if needed
throw;
        }
    }
}
