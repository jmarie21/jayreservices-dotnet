using FluentValidation;
using FluentValidation.Results;
using jayreservices_dotnet.Features.Auth.Interfaces;
using jayreservices_dotnet.Features.Auth.Login;
using jayreservices_dotnet.Features.Auth.Register;
using Microsoft.AspNetCore.Mvc;

namespace jayreservices_dotnet.Features.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IAuthService authService, 
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await registerValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var result = await authService.RegisterAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await loginValidator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });

            var result = await authService.LoginAsync(request, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { errors = result.Error });

            return Ok(result.Value);
        }
    }
}
