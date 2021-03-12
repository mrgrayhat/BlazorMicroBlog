using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MicroBlog.Server.Models.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MicroBlog.Server.Services.Security
{
    public static class JwtTokenManagerExtensions
    {
        public static SigningCredentials GetSigningCredentials(this IConfiguration jwtSettings)
        {
            SymmetricSecurityKey secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("securityKey")));

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        public static async Task<List<Claim>> GetClaims(this UserInfo user, UserManager<UserInfo> userManager, string webRootPath)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Country, user.Country),
                new Claim(ClaimTypes.Locality,user.LocaleCulture),
                new Claim("Avatar", user.Avatar)
            };
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
        public static JwtSecurityToken GenerateTokenOptions(this SigningCredentials signingCredentials, List<Claim> claims, IConfiguration jwtSettings)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings.GetSection("validIssuer").Value,
                audience: jwtSettings.GetSection("validAudience").Value,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("expiryInMinutes").Value)),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }
    }
}
