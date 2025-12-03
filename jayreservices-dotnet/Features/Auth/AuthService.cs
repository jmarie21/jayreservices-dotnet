using jayreservices_dotnet.Common;
using jayreservices_dotnet.Domain;
using jayreservices_dotnet.Features.Auth.Register;
using jayreservices_dotnet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace jayreservices_dotnet.Features.Auth
{
    public class AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher) : IAuthService
    {
        public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            User? existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return Result<RegisterResponse>.Failure("Email is already registered.");
            }

            User user = new()
            {
                Name = request.Name,
                Email = request.Email,
                Role = "User",
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            RegisterResponse response = new()
            {
                Name = user.Name
            };

            return Result<RegisterResponse>.Success(response);
        }
    }
}
