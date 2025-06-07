using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AuthenticationService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WinLicenseBackend.Models;
using WinLicenseBackend.Services;

namespace CustomerApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        //private readonly AdminUserInitializer _adminInitializer;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            AdminUserInitializer adminInitializer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
           // _adminInitializer = adminInitializer;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            var authResponse = await _tokenService.GenerateTokensAsync(user);
            
            // Set refresh token in an HTTP-only cookie
            SetRefreshTokenCookie(authResponse.RefreshToken);

            return Ok(new
            {
                Token = authResponse.AccessToken,
                ExpiresIn = authResponse.ExpiresIn,
                User = new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email
                }
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { Message = "Refresh token is required" });
            }

            try
            {
                var authResponse = await _tokenService.RefreshTokenAsync(accessToken, refreshToken);
                
                // Set new refresh token in an HTTP-only cookie
                SetRefreshTokenCookie(authResponse.RefreshToken);

                return Ok(new
                {
                    Token = authResponse.AccessToken,
                    ExpiresIn = authResponse.ExpiresIn
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken);
                
                // Remove the refresh token cookie
                Response.Cookies.Delete("refreshToken");
            }

            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpGet("health")]
        public IActionResult CheckHealth()
        {
            return Ok(new { message = "Server is up and running" });
        }


        //[HttpPost("createAdmin")]
        //public async Task<IActionResult> CreateAdmin([FromBody] LoginRequest model)
        //{
        //    bool success = await _adminInitializer.InitializeAdminUserAsync(
        //        model.Username, model.Password);
        //    return Ok(success);
        //}

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
