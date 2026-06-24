using Dams.Web.Models;

namespace Dams.Web.Services;

public interface IPasswordService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string password);
}
