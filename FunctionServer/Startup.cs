using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
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
    public class Startup
    {
        private readonly JwtSecurityTokenHandler m_jwtTokenHandler = new JwtSecurityTokenHandler();
        //private readonly SymmetricSecurityKey m_securityKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());
        private readonly SymmetricSecurityKey m_securityKey = new SymmetricSecurityKey(Guid.Parse("76150A70-DA3D-4FAE-B584-5C51307F9A04").ToByteArray());
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<Services.SampleInterceptor>();
                options.Interceptors.Add<Services.IpAddressAuthenticator>();
            });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.Name);
                    policy.RequireClaim(ClaimTypes.Role);
                });
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = true,
                            IssuerSigningKey = m_securityKey
                        };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder p_app, IWebHostEnvironment p_env)
        {
            if (p_env.IsDevelopment())
            {
                p_app.UseDeveloperExceptionPage();
            }

            p_app.UseRouting();
            p_app.UseAuthentication();
            p_app.UseAuthorization();

            p_app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Services.FunctionsServiceImpl>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });                
                endpoints.MapGet(
                    "/generateJwtToken", 
                    context => context.Response.WriteAsync( Utilities.GenerateJwtToken(context.Request.Query["name"], m_securityKey, m_jwtTokenHandler)));
            });
        }
    }
}
