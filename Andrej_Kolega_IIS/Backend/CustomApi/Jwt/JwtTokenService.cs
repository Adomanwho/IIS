using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Andrej_Kolega_IIS.Backend.CustomApi.Dto;
using Andrej_Kolega_IIS.Shared.Data;
using Andrej_Kolega_IIS.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Andrej_Kolega_IIS.Backend.CustomApi.Jwt
{
    public class JwtTokenService
    {
        private readonly JwtSettings _settings;
        private readonly AppDbContext _context;

        public JwtTokenService(IOptions<JwtSettings> options, AppDbContext context)
        {
            _settings = options.Value;
            _context = context;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<TokenResponseDto> IssueTokensAsync(User user)
        {
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

            return new TokenResponseDto
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = await GenerateAndStoreRefreshTokenAsync(user),
                AccessTokenExpiresAtUtc = accessTokenExpiresAt
            };
        }

        public async Task<string> GenerateAndStoreRefreshTokenAsync(User user)
        {
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays)
            });
            await _context.SaveChangesAsync();

            return rawToken;
        }

        public async Task<RefreshToken?> FindActiveRefreshTokenAsync(string rawToken)
        {
            var hash = HashToken(rawToken);
            var token = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            return token is not null && token.IsActive ? token : null;
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(bytes);
        }
    }
}
