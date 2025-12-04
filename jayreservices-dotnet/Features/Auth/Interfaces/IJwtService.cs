using jayreservices_dotnet.Domain.Entities;

namespace jayreservices_dotnet.Features.Auth.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
