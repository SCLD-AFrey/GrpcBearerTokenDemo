using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace FunctionServer
{
    public static class Program
    {
        public static void Main(string[] p_args)
        {
            CreateHostBuilder(p_args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        private static IHostBuilder CreateHostBuilder(string[] p_args) =>
            Host.CreateDefaultBuilder(p_args)
                .ConfigureWebHostDefaults(p_webBuilder =>
                {
                    p_webBuilder.UseStartup<Startup>();
                });
    }
}
