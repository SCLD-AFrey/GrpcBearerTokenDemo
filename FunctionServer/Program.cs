using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using FunctionServer;
using FunctionServerProto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;  

namespace FunctionServer
{
    public static class Program
    {
        public static void Main(string[] p_args)
        {
            CreateHostBuilder(p_args).Build().Run();
        }                                                                                                           

        private static IHostBuilder CreateHostBuilder(string[] p_args) =>
            Host.CreateDefaultBuilder(p_args)
                .ConfigureWebHostDefaults(p_webBuilder =>
                {
                    p_webBuilder.UseStartup<Startup>();
                });
    }
}
