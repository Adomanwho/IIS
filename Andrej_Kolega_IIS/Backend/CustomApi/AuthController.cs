using Andrej_Kolega_IIS.Backend.CustomApi.Dto;
using Andrej_Kolega_IIS.Backend.CustomApi.Jwt;
using Andrej_Kolega_IIS.Shared.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Andrej_Kolega_IIS.Backend.CustomApi
{
    [ApiController]
    [Route("api/custom/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenService _tokenService;

        public AuthController(AppDbContext context, JwtTokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            var hasher = new PasswordHasher<Shared.Entities.User>();

            if (user is null ||
                hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(await _tokenService.IssueTokensAsync(user));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshRequestDto request)
        {
            var existingToken = await _tokenService.FindActiveRefreshTokenAsync(request.RefreshToken);
            if (existingToken is null || existingToken.User is null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            await _tokenService.RevokeRefreshTokenAsync(existingToken);

            return Ok(await _tokenService.IssueTokensAsync(existingToken.User));
        }
    }
}
