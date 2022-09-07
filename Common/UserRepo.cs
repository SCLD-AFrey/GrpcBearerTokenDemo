using System;
using System.Collections.ObjectModel;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace CommonFiles
{
    public static class UserRepo
    {
        public static Collection<ClientUser> Users()
        {
            Collection<ClientUser> users = new Collection<ClientUser>
            {
                new ClientUser()
                {
                    UserName = "afrey", Password = @"password", EmailAddress = "afrey@steelcloud.com", Dob = DateTime.Parse("01/01/2001"), Roles = new enmEnRoles[] {enmEnRoles.ADMIN, enmEnRoles.POWERUSER}
                },
                new ClientUser()
                {
                    UserName = "user1", Password = @"password", EmailAddress = "user1@steelcloud.com", Dob = DateTime.Parse("02/02/2002"), Roles = new enmEnRoles[] {enmEnRoles.POWERUSER}
                },
                new ClientUser()
                {
                    UserName = "user2", Password = @"password", EmailAddress = "user2@steelcloud.com", Dob = DateTime.Parse("03/03/2003"), Roles = new enmEnRoles[] {enmEnRoles.PRIVATE_USER}
                },
                new ClientUser()
                {
                    UserName = "user3", Password = @"password", EmailAddress = "user3@steelcloud.com", Dob = DateTime.Parse("04/04/2004")
                },
                new ClientUser()
                {
                    UserName = "user4", Password = @"password", EmailAddress = "user4@steelcloud.com", Dob = DateTime.Parse("05/05/2004")
                }
            };
            return users;
        }
        [Flags]
        public enum enmEnRoles : byte
        {
            
            PUBLIC_USER = 0,
            ADMIN   = 1,
            POWERUSER   = 2, 
            PRIVATE_USER = 4
        }
        public class ClientUser
        {
            public string? UserName { get; init; }
            public string? Password { get; init; }
            public UserRepo.enmEnRoles[]? Roles { get; set; } = {enmEnRoles.PUBLIC_USER};
            public string? EmailAddress { get; set; }
            public DateTime? Dob { get; set; }
        }
        
        
        public static string HashPassword(string password, string algorithm = "sha256")
        {
            return Hash(Encoding.UTF8.GetBytes(password), algorithm);
        }

        private static string Hash(byte[] input, string algorithm = "sha256")
        {
            using (var hashAlgorithm = HashAlgorithm.Create(algorithm))
            {
                return Convert.ToBase64String(hashAlgorithm.ComputeHash(input));
            }
        }
        
    }
}