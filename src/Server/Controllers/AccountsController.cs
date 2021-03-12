using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroBlog.Server.DTOs.Identity;
using MicroBlog.Server.Models.Identity;
using MicroBlog.Server.Services.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSwag.Annotations;

namespace MicroBlog.Server.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<UserInfo> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfigurationSection _jwtSettings;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(ILogger<AccountsController> logger, UserManager<UserInfo> userManager, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("JwtSettings");
            _userManager = userManager;
        }

        [HttpPost("Registration")]
        [SwaggerResponse(System.Net.HttpStatusCode.Created, typeof(RegistrationResponseDto))]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, typeof(RegistrationResponseDto))]
        public async Task<ActionResult<RegistrationResponseDto>> RegisterUser([FromBody] UserRegistrationDto userForRegistration)
        {
            if (userForRegistration == null || !ModelState.IsValid)
            {
                return BadRequest(new RegistrationResponseDto
                {
                    IsSuccessfulRegistration = false,
                    Errors = ModelState.Values.SelectMany(item => item.Errors.Select(error => error.ErrorMessage))
                });
            }
            var user = new UserInfo
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
                    IsSuccessfulRegistration = false,
                    Errors = errors
                });
            }
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogWarning("User {user} Registered.", userForRegistration.Username);

            return StatusCode(201, new RegistrationResponseDto
            {
                IsSuccessfulRegistration = true
            });
        }

        [HttpPost("Login")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(LoginResponseDto))]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, typeof(LoginResponseDto))]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, typeof(LoginResponseDto))]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] UserLoginDto userLoginDto)
        {
            UserInfo user;

            if (userLoginDto.UsernameOrEmail.Contains("@"))
                user = await _userManager.FindByEmailAsync(userLoginDto.UsernameOrEmail);
            else
                user = await _userManager.FindByNameAsync(userLoginDto.UsernameOrEmail);

            // check account lock state
            if (user != null && await _userManager.IsLockedOutAsync(user))
            {
                DateTimeOffset? lockedDate = await _userManager.GetLockoutEndDateAsync(user);

                _logger.LogWarning("Account {account} LockedOut!", user.UserName, lockedDate.Value.ToLocalTime().ToString());
                return Unauthorized(new LoginResponseDto
                {
                    IsAuthSuccessful = false,
                    ErrorMessage = $"due to too many tries, this account is locked out until{lockedDate.Value.ToLocalTime()}. you can try to recover your account."
                });
            }
            if (user is null) // not valid username or user existence
            {
                _logger.LogWarning("Username/Email {user} NotFound to login", userLoginDto.UsernameOrEmail);

                return Unauthorized(new LoginResponseDto
                {
                    ErrorMessage = "Invalid Username/Password"
                });
            }
            else if (!await _userManager.CheckPasswordAsync(user, userLoginDto.Password))
            {
                if (await _userManager.GetLockoutEnabledAsync(user))
                {
                    await _userManager.AccessFailedAsync(user).ConfigureAwait(false);
                    _logger.LogWarning("AccessFailed occured. due to lockout setting.");
                }

                _logger.LogWarning("Incorrect Password Entered for user {user} to login", userLoginDto.UsernameOrEmail);
                return Unauthorized(new LoginResponseDto
                {
                    ErrorMessage = "Invalid Username/Password"
                });
            }
            // generate access token
            SigningCredentials signingCredentials = JwtTokenManagerExtensions
                .GetSigningCredentials(_jwtSettings);
            List<Claim> claims = await JwtTokenManagerExtensions
                .GetClaims(user, _userManager, _webHostEnvironment.WebRootPath);
            JwtSecurityToken tokenOptions = JwtTokenManagerExtensions
                .GenerateTokenOptions(signingCredentials, claims, _jwtSettings);
            string token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            _logger.LogWarning("Token Generated for user {user}", userLoginDto.UsernameOrEmail);
            return Ok(new LoginResponseDto
            {
                IsAuthSuccessful = true,
                Token = token
            });
        }
    }
}
