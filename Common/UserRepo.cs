using System;
using System.Collections.ObjectModel;

namespace Common
{
    public static class UserRepo
    {
        public static Collection<ClientUser> Users()
        {
            Collection<ClientUser> users = new Collection<ClientUser>();
            users.Add(new ClientUser(){UserName = "afrey", EmailAddress = "afrey@steelcloud.com", DOB = DateTime.Parse("01/01/2001"), Roles = new enRoles[] {enRoles.ADMIN, enRoles.POWERUSER}});
            users.Add(new ClientUser(){UserName = "user1", EmailAddress = "user1@steelcloud.com", DOB = DateTime.Parse("02/02/2002"), Roles = new enRoles[] {enRoles.POWERUSER}});
            users.Add(new ClientUser(){UserName = "user2", EmailAddress = "user2@steelcloud.com", DOB = DateTime.Parse("03/03/2003"), Roles = new enRoles[] {enRoles.PRIVATE_USER}});
            users.Add(new ClientUser(){UserName = "user3", EmailAddress = "user3@steelcloud.com", DOB = DateTime.Parse("04/04/2004"), Roles = new enRoles[] {enRoles.PUBLIC_USER}});
            return users;
        }
        [Flags]
        public enum enRoles : byte
        {
            ADMIN   = 1 << 0,
            POWERUSER   = 1 << 1, 
            PRIVATE_USER = 1 << 2,
            PUBLIC_USER = 1 << 4,
        }
        public class ClientUser
        {
            public string UserName { get; set; }
            public UserRepo.enRoles[] Roles { get; set; }
            public string EmailAddress { get; set; }
            public DateTime DOB { get; set; }
        }
    }
}