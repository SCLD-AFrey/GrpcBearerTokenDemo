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

        private static IHostBuilder CreateHostBuilder(string[] p_args) =>
            Host.CreateDefaultBuilder(p_args)
                .ConfigureWebHostDefaults(p_webBuilder =>
                {
                    p_webBuilder.UseStartup<Startup>();
                });
    }
}
