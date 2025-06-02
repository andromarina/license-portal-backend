using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthenticationService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace WinLicenseBackend.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public TokenService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<AuthResponse> GenerateTokensAsync(ApplicationUser user)
        {
            // Generate access token
            var accessToken = await GenerateAccessTokenAsync(user);
            
            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            
            // Save refresh token to database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
                IssuedDate = DateTime.UtcNow,
                IsRevoked = false
            };
            
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = int.Parse(_configuration["JWT:ExpiryInMinutes"]) * 60
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                throw new SecurityTokenException("Invalid access token");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityTokenException("Invalid access token");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new SecurityTokenException("User not found");
            }

            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedRefreshToken == null || 
                storedRefreshToken.UserId != userId || 
                storedRefreshToken.ExpiryDate <= DateTime.UtcNow ||
                storedRefreshToken.IsRevoked)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            // Revoke the current refresh token
            storedRefreshToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(storedRefreshToken);

            // Generate new tokens
            return await GenerateTokensAsync(user);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedRefreshToken != null && !storedRefreshToken.IsRevoked)
            {
                storedRefreshToken.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(storedRefreshToken);
            }
        }

        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryInMinutes = int.Parse(_configuration["JWT:ExpiryInMinutes"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // We don't care about the token's expiration date
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudiences = [_configuration["JWT:ValidAudience"]],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Console.WriteLine("aud: " + string.Join(", ", jwt.Audiences));
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }

    public interface ITokenService
    {
        Task<AuthResponse> GenerateTokensAsync(ApplicationUser user);
        Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
