using jayreservices_dotnet.Common;
using jayreservices_dotnet.Features.Auth.Login;
using jayreservices_dotnet.Features.Auth.Register;

namespace jayreservices_dotnet.Features.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
