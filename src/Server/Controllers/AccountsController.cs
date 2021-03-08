using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroBlog.Server.DTOs.Identity;
using MicroBlog.Server.Services.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MicroBlog.Server.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _jwtSettings;

        public AccountsController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("JwtSettings");
            _userManager = userManager;
        }

        [HttpPost("Registration")]
        public async Task<ActionResult<RegistrationResponseDto>> RegisterUser([FromBody] UserRegistrationDto userForRegistration)
        {
            if (userForRegistration == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            var user = new IdentityUser
            {
                UserName = userForRegistration.Username,
                Email = userForRegistration.Email,
                PasswordHash = _userManager.PasswordHasher.HashPassword(null, userForRegistration.Password),
                ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var result = await _userManager.CreateAsync(user, userForRegistration.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new RegistrationResponseDto
                {
                    Errors = errors
                });
            }
            await _userManager.AddToRoleAsync(user, "User");

            return StatusCode(201);
        }

        [HttpPost("Login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] UserLoginDto userLoginDto)
        {
            IdentityUser user;

            if (userLoginDto.UsernameOrEmail.Contains("@"))
                user = await _userManager.FindByEmailAsync(userLoginDto.UsernameOrEmail);
            else
                user = await _userManager.FindByNameAsync(userLoginDto.UsernameOrEmail);

            if (user is null || !await _userManager.CheckPasswordAsync(user, userLoginDto.Password))
            {
                return Unauthorized(new LoginResponseDto
                {
                    ErrorMessage = "Invalid User/Password"
                });
            }

            SigningCredentials signingCredentials = JwtTokenManagerExtensions
                .GetSigningCredentials(_jwtSettings);
            List<Claim> claims = await JwtTokenManagerExtensions
                .GetClaims(user, _userManager);
            JwtSecurityToken tokenOptions = JwtTokenManagerExtensions
                .GenerateTokenOptions(signingCredentials, claims, _jwtSettings);
            string token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return Ok(new LoginResponseDto
            {
                IsAuthSuccessful = true,
                Token = token
            });
        }

    }
}
