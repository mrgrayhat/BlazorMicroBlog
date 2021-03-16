using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using MicroBlog.Server.DTOs.Identity;
using MicroBlog.Server.Models.Identity;
using MicroBlog.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace MicroBlog.Server.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<UserInfo> _userManager;
        private readonly ITokenService _tokenService;

        public TokenController(UserManager<UserInfo> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(LoginResponseDto))]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, typeof(LoginResponseDto))]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto tokenDto)
        {
            if (tokenDto is null)
            {
                return BadRequest(new LoginResponseDto { IsAuthSuccessful = false, ErrorMessage = "Invalid client request" });
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenDto.Token);
            var username = principal.Identity.Name;

            UserInfo user = await _userManager.FindByNameAsync(username);
            if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest(new LoginResponseDto { IsAuthSuccessful = false, ErrorMessage = "Invalid client request" });

            var signingCredentials = _tokenService.GetSigningCredentials();
            var claims = await _tokenService.GetClaims(user);
            var tokenOptions = _tokenService.GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            user.RefreshToken = _tokenService.GenerateRefreshToken();

            await _userManager.UpdateAsync(user);

            return Ok(new LoginResponseDto { Token = token, RefreshToken = user.RefreshToken, IsAuthSuccessful = true });
        }
    }
}
