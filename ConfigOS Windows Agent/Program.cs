using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommonFiles;

namespace ConfigOS_Windows_Agent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(p_webBuilder =>
                {
                    p_webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Loopback, Constants.Ports.WinAgentInsecure);
                        options.Listen(IPAddress.Loopback, Constants.Ports.WinAgentSecure, configure => configure.UseHttps());
                    });
                    p_webBuilder.UseStartup<Startup>();
                });
    }
}
