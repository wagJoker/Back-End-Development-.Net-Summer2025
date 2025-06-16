using ChatApp.Backend.Models;
using ChatApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                var token = _jwtService.GenerateJwtToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiryTime = DateTime.UtcNow.AddDays(7);

                await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, expiryTime);

                _logger.LogInformation("User registered successfully: {Username}", user.Username);

                return Ok(new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiryTime,
                    Username = user.Username
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for username: {Username}", request.Username);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
                return StatusCode(500, "An error occurred during registration");
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!await _userService.ValidateUserAsync(request.Username, request.Password))
                {
                    return Unauthorized("Invalid username or password");
                }

                var user = await _userService.GetUserByUsernameAsync(request.Username);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var token = _jwtService.GenerateJwtToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiryTime = DateTime.UtcNow.AddDays(7);

                await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, expiryTime);
                await _userService.UpdateUserLastLoginAsync(user.Id);

                _logger.LogInformation("User logged in successfully: {Username}", user.Username);

                return Ok(new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiryTime,
                    Username = user.Username
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
                return StatusCode(500, "An error occurred during login");
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var user = await _userService.GetUserByRefreshTokenAsync(request.RefreshToken);
                if (user == null)
                {
                    return Unauthorized("Invalid refresh token");
                }

                var token = _jwtService.GenerateJwtToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiryTime = DateTime.UtcNow.AddDays(7);

                await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, expiryTime);

                _logger.LogInformation("Token refreshed successfully for user: {Username}", user.Username);

                return Ok(new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiryTime,
                    Username = user.Username
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, "An error occurred while refreshing token");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("nameid")?.Value ?? "0");
                await _userService.UpdateRefreshTokenAsync(userId, string.Empty, DateTime.UtcNow);

                _logger.LogInformation("User logged out successfully: {Username}", User.Identity?.Name);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "An error occurred during logout");
            }
        }
    }
} 