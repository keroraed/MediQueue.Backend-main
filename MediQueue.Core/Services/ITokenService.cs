using MediQueue.Core.Entities.Identity;

namespace MediQueue.Core.Services;

public interface ITokenService
{
    string CreateToken(AppUser user, IList<string> roles);
}
