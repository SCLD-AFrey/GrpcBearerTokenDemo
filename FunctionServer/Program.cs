using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using CommonFiles;
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
                    p_webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Loopback, Constants.Ports.FunctionInsecure);
                        options.Listen(IPAddress.Loopback, Constants.Ports.FunctionSecure, configure => configure.UseHttps());
                    });
                    p_webBuilder.UseStartup<Startup>();
                });
    }
}
