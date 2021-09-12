using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using CommonFiles;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using FunctionServerProto;
using Microsoft.AspNetCore.Http;

namespace FunctionServer.Services
{
    public class FunctionsServiceImpl : FunctionsService.FunctionsServiceBase
    {
        public FunctionsServiceImpl() { }

        [Authorize(Roles = "POWERUSER")]
        public override Task<UserInfoReply> GetUserInfoRpc(UserRequest p_request, ServerCallContext p_context)
        {
            var identity = p_context.GetHttpContext().User.Identity as ClaimsIdentity;

            var roles = new List<string>();
            var reply = new UserInfoReply();
            foreach (var claim in identity.Claims)
            {
                if (claim.Type.Equals(ClaimTypes.Name))
                {
                    reply.Username = claim.Value;
                }
                if (claim.Type.Equals(ClaimTypes.Email))
                {
                    reply.Emailaddress = claim.Value;
                }
                if (claim.Type.Equals(ClaimTypes.DateOfBirth))
                {
                    reply.Dob = claim.Value;
                }
                if (claim.Type.Equals(ClaimTypes.Role))
                {
                    reply.Roles.Add(new UserRole()
                    {
                        Name = claim.Value
                    });
                }
            }
            
            return Task.FromResult(reply);

        }

        [Authorize(Roles = "ADMIN")]
        public override Task<UserRepoReply> GetUserAllUsers(Empty p_request, ServerCallContext p_context)
        {
            var reply = new UserRepoReply();
            foreach (var user in UserRepo.Users())
            {
                var u = new UserInfoReply();
                u.Username = user.UserName;
                u.Emailaddress = user.EmailAddress;
                foreach (var urole in user.Roles)
                {
                    u.Roles.Add(new UserRole()
                    {
                         Name = urole.ToString()
                    });
                }
                reply.Users.Add(u);
            }
            
            return Task.FromResult(reply);
        }



        [Authorize(Roles = "PRIVATE_USER")]
        public override Task<BasicReply> ReturnUtcDate(Empty p_request, ServerCallContext p_context)
        {
            return Task.FromResult(new BasicReply()
            {
                Content = DateTime.UtcNow.ToString()
            });
        }

        [Authorize]
        public override Task<Timestamp> ReturnCurrentTimestamp(Empty p_request, ServerCallContext p_context)
        {
            return Task.FromResult(new Timestamp());
        }
    }
}