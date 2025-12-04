using FluentAssertions;
using jayreservices_dotnet.Domain.Entities;
using jayreservices_dotnet.Domain.Enums;
using jayreservices_dotnet.Features.Auth.Interfaces;
using jayreservices_dotnet.Features.Auth.Login;
using jayreservices_dotnet.Features.Auth.Register;
using jayreservices_dotnet.Features.Auth.Services;
using jayreservices_dotnet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace jayreservices_dotnet_Tests.Features.Auth;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        _jwtServiceMock = new Mock<IJwtService>();
        _authService = new AuthService(_context, _passwordHasherMock.Object, _jwtServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest("John Doe", "john@example.com", "Password123!");
        _passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<User>(), request.Password))
                            .Returns("hashed_password");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldSaveUserToDatabase()
    {
        // Arrange
        var request = new RegisterRequest("Jane Doe", "jane@example.com", "Password123!");
        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), request.Password))
            .Returns("hashed_password");

        // Act
        await _authService.RegisterAsync(request);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        savedUser.Should().NotBeNull();
        savedUser.Name.Should().Be("Jane Doe");
        savedUser.Email.Should().Be("jane@example.com");
        savedUser.PasswordHash.Should().Be("hashed_password");
        savedUser.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = new User
        {
            Name = "Existing User",
            Email = "existing@example.com",
            PasswordHash = "hashed_password",
            Role = UserRole.Admin,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest("New User", "existing@example.com", "Password123!");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email is already registered. Try a different one");
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        var request = new RegisterRequest("Test User", "test@example.com", "MySecurePassword!");
        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), "MySecurePassword!"))
            .Returns("securely_hashed_password");

        // Act
        await _authService.RegisterAsync(request);

        // Assert
        _passwordHasherMock.Verify(
            x => x.HashPassword(It.IsAny<User>(), "MySecurePassword!"),
            Times.Once);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            Role = UserRole.Admin,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("test@example.com", "Password123!");

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, "hashed_password", "Password123!"))
            .Returns(PasswordVerificationResult.Success);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("jwt_token_here");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().Be("jwt_token_here");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "Password123!");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            Role = UserRole.Admin,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("test@example.com", "WrongPassword!");

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, "hashed_password", "WrongPassword!"))
            .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldGenerateToken()
    {
        // Arrange
        var user = new User
        {
            Name = "Token User",
            Email = "token@example.com",
            PasswordHash = "hashed_password",
            Role = UserRole.Admin,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("token@example.com", "Password123!");

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), "hashed_password", "Password123!"))
            .Returns(PasswordVerificationResult.Success);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("generated_jwt_token");

        // Act
        await _authService.LoginAsync(request);

        // Assert
        _jwtServiceMock.Verify(x => x.GenerateToken(It.Is<User>(u => u.Email == "token@example.com")), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithRehashNeeded_ShouldReturnSuccess()
    {
        // Arrange
        var user = new User
        {
            Name = "Rehash User",
            Email = "rehash@example.com",
            PasswordHash = "old_hashed_password",
            Role = UserRole.Admin,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("rehash@example.com", "Password123!");

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, "old_hashed_password", "Password123!"))
            .Returns(PasswordVerificationResult.SuccessRehashNeeded);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("jwt_token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
