using System;
using System.Collections.ObjectModel;

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
                    UserName = "afrey", EmailAddress = "afrey@steelcloud.com", Dob = DateTime.Parse("01/01/2001"), Roles = new enmEnRoles[] {enmEnRoles.ADMIN, enmEnRoles.POWERUSER}
                },
                new ClientUser()
                {
                    UserName = "user1", EmailAddress = "user1@steelcloud.com", Dob = DateTime.Parse("02/02/2002"), Roles = new enmEnRoles[] {enmEnRoles.POWERUSER}
                },
                new ClientUser()
                {
                    UserName = "user2", EmailAddress = "user2@steelcloud.com", Dob = DateTime.Parse("03/03/2003"), Roles = new enmEnRoles[] {enmEnRoles.PRIVATE_USER}
                },
                new ClientUser()
                {
                    UserName = "user3", EmailAddress = "user3@steelcloud.com", Dob = DateTime.Parse("04/04/2004")
                },
                new ClientUser()
                {
                    UserName = "user4", EmailAddress = "user4@steelcloud.com", Dob = DateTime.Parse("05/05/2004")
                }
            };
            return users;
        }
        [Flags]
        public enum enmEnRoles : byte
        {
            ADMIN   = 1 << 0,
            POWERUSER   = 1 << 1, 
            PRIVATE_USER = 1 << 2,
            PUBLIC_USER = 1 << 4,
        }
        public class ClientUser
        {
            public string? UserName { get; init; }
            public UserRepo.enmEnRoles[]? Roles { get; set; } = {enmEnRoles.PUBLIC_USER};
            public string? EmailAddress { get; set; }
            public DateTime? Dob { get; set; }
        }
    }
}