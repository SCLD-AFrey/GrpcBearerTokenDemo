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

        public override Task<UserRepoReply> GetUserAllUsers(Empty p_request, ServerCallContext p_context)
        {
            var reply = new UserRepoReply();
            foreach (var user in UserRepo.Users())
            {
                reply.Users.Add(new User()
                {
                    Username = user.UserName
                });
                
            }
            
            return Task.FromResult(reply);
        }

        public override Task<BasicReply> GetSumRpc(BasicRequest p_request, ServerCallContext p_context)
        {
            int val = 0;
            foreach (var num in p_request.Content.Split(' '))
            {
                val += int.Parse(num);
            }
            
            
            return Task.FromResult(new BasicReply()
            {
                Content = val.ToString()
            });
        }

        public override Task<BasicReply> ReturnUtcDate(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new BasicReply()
            {
                Content = DateTime.UtcNow.ToString()
            });
        }
    }
}