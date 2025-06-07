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
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

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
            _logger.Debug("Begin GenerateTokensAsync for UserId={UserId}, UserName={UserName}", user.Id, user.UserName);

            // Generate access token
            var accessToken = await GenerateAccessTokenAsync(user);
            _logger.Debug("Access token generated for UserId={UserId}", user.Id);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            _logger.Debug("Refresh token generated for UserId={UserId}", user.Id);

            // Save refresh token to database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
                IssuedDate = DateTime.UtcNow,
                IsRevoked = false
            };

            try
            {
                await _refreshTokenRepository.AddAsync(refreshTokenEntity);
                _logger.Debug("Refresh token saved to database for UserId={UserId}, Expires={Expiry}", user.Id, refreshTokenEntity.ExpiryDate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving refresh token for UserId={UserId}", user.Id);
                throw;
            }

            var expiresInSeconds = int.Parse(_configuration["JWT:ExpiryInMinutes"]) * 60;
            _logger.Debug("GenerateTokensAsync completed for UserId={UserId} (AccessExpiresIn={ExpiresIn}s)", user.Id, expiresInSeconds);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresInSeconds
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            _logger.Debug("Begin RefreshTokenAsync");

            ClaimsPrincipal principal;
            try
            {
                principal = GetPrincipalFromExpiredToken(accessToken);
            }
            catch (SecurityTokenException stex)
            {
                _logger.Warn(stex, "Invalid access token provided to RefreshTokenAsync");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error validating expired token in RefreshTokenAsync");
                throw;
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.Warn("ClaimTypes.NameIdentifier not found in expired token");
                throw new SecurityTokenException("Invalid access token");
            }
            _logger.Debug("Expired token belongs to UserId={UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.Warn("User with ID={UserId} not found during RefreshTokenAsync", userId);
                throw new SecurityTokenException("User not found");
            }

            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedRefreshToken == null)
            {
                _logger.Warn("No stored refresh token found in DB for provided token. UserId={UserId}", userId);
                throw new SecurityTokenException("Invalid refresh token");
            }

            if (storedRefreshToken.UserId != userId)
            {
                _logger.Warn("Refresh token UserId mismatch. Expected={Expected}, Actual={Actual}", userId, storedRefreshToken.UserId);
                throw new SecurityTokenException("Invalid refresh token");
            }

            if (storedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                _logger.Warn("Refresh token expired. UserId={UserId}, ExpiredAt={ExpiryDate}", userId, storedRefreshToken.ExpiryDate);
                throw new SecurityTokenException("Invalid refresh token");
            }

            if (storedRefreshToken.IsRevoked)
            {
                _logger.Warn("Refresh token already revoked. UserId={UserId}", userId);
                throw new SecurityTokenException("Invalid refresh token");
            }

            // Revoke the current refresh token
            storedRefreshToken.IsRevoked = true;
            try
            {
                await _refreshTokenRepository.UpdateAsync(storedRefreshToken);
                _logger.Debug("Revoked existing refresh token for UserId={UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error revoking refresh token for UserId={UserId}", userId);
                throw;
            }

            // Generate new tokens
            _logger.Debug("Generating new tokens for UserId={UserId}", userId);
            var newTokens = await GenerateTokensAsync(user);
            _logger.Debug("RefreshTokenAsync completed successfully for UserId={UserId}", userId);

            return newTokens;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            _logger.Debug("Begin RevokeRefreshTokenAsync for token: {RefreshToken}", refreshToken);

            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedRefreshToken == null)
            {
                _logger.Warn("Refresh token not found during RevokeRefreshTokenAsync");
                return;
            }

            if (storedRefreshToken.IsRevoked)
            {
                _logger.Warn("Refresh token already revoked. TokenId={TokenId}", storedRefreshToken.Id);
                return;
            }

            storedRefreshToken.IsRevoked = true;
            try
            {
                await _refreshTokenRepository.UpdateAsync(storedRefreshToken);
                _logger.Debug("Refresh token revoked successfully. TokenId={TokenId}, UserId={UserId}",
                    storedRefreshToken.Id, storedRefreshToken.UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error revoking refresh token. TokenId={TokenId}", storedRefreshToken.Id);
                throw;
            }
        }

        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            _logger.Debug("Begin GenerateAccessTokenAsync for UserId={UserId}", user.Id);

            var userRoles = await _userManager.GetRolesAsync(user);
            _logger.Debug("UserId={UserId} has Roles: {Roles}", user.Id, string.Join(", ", userRoles));

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

            var secretKey = _configuration["JWT:Secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.Error("JWT:Secret configuration is missing");
                throw new InvalidOperationException("JWT:Secret not configured");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryInMinutesValue = _configuration["JWT:ExpiryInMinutes"];
            if (!int.TryParse(expiryInMinutesValue, out var expiryInMinutes))
            {
                _logger.Error("Invalid JWT:ExpiryInMinutes configuration: {ExpiryInMinutesValue}", expiryInMinutesValue);
                throw new InvalidOperationException("JWT:ExpiryInMinutes invalid");
            }

            var expiry = DateTime.UtcNow.AddMinutes(expiryInMinutes);
            _logger.Debug("Access token expiry set to {ExpiryUtc}", expiry);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: expiry,
                signingCredentials: credentials
            );

            var jwtHandler = new JwtSecurityTokenHandler();
            var writtenToken = jwtHandler.WriteToken(tokenDescriptor);
            _logger.Info("Access token created for UserId={UserId}", user.Id);

            return writtenToken;
        }

        private string GenerateRefreshToken()
        {
            _logger.Debug("Begin GenerateRefreshToken");
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);
            _logger.Debug("Generated raw refresh token string (not yet stored)");
            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            _logger.Debug("Begin GetPrincipalFromExpiredToken");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // We only want to read claims from an expired token
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudiences = new[] { _configuration["JWT:ValidAudience"] },
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
            };

            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = jwtHandler.ReadJwtToken(token);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to read JWT token string");
                throw new SecurityTokenException("Invalid access token");
            }

            _logger.Debug("Token audiences: {Audiences}", string.Join(", ", jwtSecurityToken.Audiences));

            ClaimsPrincipal principal;
            SecurityToken validatedToken;
            try
            {
                principal = jwtHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Token validation failed in GetPrincipalFromExpiredToken");
                throw new SecurityTokenException("Invalid access token");
            }

            if (validatedToken is not JwtSecurityToken validJwtToken ||
                !validJwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {                
                throw new SecurityTokenException("Invalid token");
            }

            _logger.Debug("GetPrincipalFromExpiredToken succeeded");
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
