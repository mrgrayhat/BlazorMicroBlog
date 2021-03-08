using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MicroBlog.Server.Services.Security
{
    public static class JwtTokenManagerExtensions
    {
        public static SigningCredentials GetSigningCredentials(this IConfiguration jwtSettings)
        {
            byte[] key = Encoding.UTF8.GetBytes(jwtSettings.GetSection("securityKey").Value);
            SymmetricSecurityKey secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        public static async Task<List<Claim>> GetClaims(this IdentityUser user, UserManager<IdentityUser> userManager)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
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
