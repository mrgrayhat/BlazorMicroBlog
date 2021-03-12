using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace MicroBlog.Blazor.Client.Services.Auth
{
    /// <summary>
    /// jwt token and roles parser
    /// </summary>
    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            List<Claim> claims = new List<Claim>();
            string payload = jwt.Split('.')[1];

            Dictionary<string, object> keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(ParseBase64WithoutPadding(payload));

            ExtractRolesFromJWT(claims, keyValuePairs);

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
                default: break;
            }
            return Convert.FromBase64String(base64);
        }
        private static void ExtractRolesFromJWT(List<Claim> claims, Dictionary<string, object> keyValuePairs)
        {
            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (roles != null)
            {
                string[] parsedRoles = roles.ToString().Trim().TrimStart('[').TrimEnd(']').Split(',');

                if (parsedRoles.Length > 1)
                {
                    for (int i = 0; i < parsedRoles.Length; i++)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRoles[i].Trim('"')));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, parsedRoles[0]));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }
        }
    }
}
