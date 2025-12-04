using jayreservices_dotnet.Common;
using jayreservices_dotnet.Domain.Entities;
using jayreservices_dotnet.Domain.Enums;
using jayreservices_dotnet.Features.Auth.Interfaces;
using jayreservices_dotnet.Features.Auth.Login;
using jayreservices_dotnet.Features.Auth.Register;
using jayreservices_dotnet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace jayreservices_dotnet.Features.Auth.Services
{
    public class AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher, IJwtService jwtService) : IAuthService
    {
        public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                User? existingUser = await context.Users
                                    .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

                if (existingUser != null)
                {
                    return Result<RegisterResponse>.Failure("Email is already registered. Try a different one");
                }

                User user = new()
                {
                    Name = request.Name,
                    Email = request.Email,
                    Role = UserRole.Admin,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                    UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

                context.Users.Add(user);
                await context.SaveChangesAsync(cancellationToken);

                RegisterResponse response = new(
                    user.Name
                );

                return Result<RegisterResponse>.Success(response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            User? user = await context.Users
                        .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user is null)
                return Result<LoginResponse>.Failure("Invalid email or password");

            PasswordVerificationResult checkPassword = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (checkPassword == PasswordVerificationResult.Failed)
                return Result<LoginResponse>.Failure("Invalid email or password");

            string token = jwtService.GenerateToken(user);

            LoginResponse response = new(token);

            return Result<LoginResponse>.Success(response);
        }
    }
}
