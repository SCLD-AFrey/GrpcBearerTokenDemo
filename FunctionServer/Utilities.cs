using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using CommonFiles;
using Microsoft.IdentityModel.Tokens;

namespace FunctionServer
{
    public class Utilities
    {
        public static string GenerateJwtToken(string p_username, SecurityKey p_securityKey, JwtSecurityTokenHandler p_jwtTokenHandler)
        {            
            var claims = new List<Claim>();
            var roles = new List<string>();
            UserRepo.ClientUser user;
            SigningCredentials credentials;
            Collection<UserRepo.ClientUser> users = UserRepo.Users();
            
            user = users.Where(o => o.UserName == p_username.ToLower()).FirstOrDefault();

            if (user != null)
            {
                
                
                claims.Add(new Claim(ClaimTypes.Name, p_username));
                claims.Add(new Claim(ClaimTypes.DateOfBirth, user.DOB.ToString()));
                claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));
                credentials = new SigningCredentials(p_securityKey, SecurityAlgorithms.HmacSha256);


                if (user.Roles.Length == 0)
                {
                    claims.Add(new Claim(ClaimTypes.Role, UserRepo.enRoles.PUBLIC_USER.ToString()));
                }
                else
                {
                    foreach (var role in user.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                        roles.Add(role.ToString());
                    }
                }
                
                
                Console.WriteLine($"{p_username} authenticated as {string.Join(", ", user.Roles)}");
                var token = new JwtSecurityToken(
                    "ExampleServer", 
                    "ExampleClients", 
                    claims.ToArray(), 
                    expires: DateTime.Now.AddSeconds(60), 
                    signingCredentials: credentials);
                return p_jwtTokenHandler.WriteToken(token);
            }
            else
            {
                Console.WriteLine($"{p_username} not found");
                return null;
            }

            
            
            
            
            
            
        }
    }
}