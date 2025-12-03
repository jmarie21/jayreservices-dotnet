using jayreservices_dotnet.Common;
using jayreservices_dotnet.Features.Auth.Register;

namespace jayreservices_dotnet.Features.Auth
{
    public interface IAuthService
    {
        Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    }
}
