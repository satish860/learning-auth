using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

namespace AuthCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDataProtection();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<AuthService>();
            builder.Services
                   .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                   .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            var app = builder.Build();

            //app.Use((context, next) =>
            //{
            //    var dataProtectionProvider = context.RequestServices.GetRequiredService<IDataProtectionProvider>();
            //    var decryptor = dataProtectionProvider.CreateProtector("login");
            //    var payload = context.Request.Cookies["auth"];
            //    if (payload != null)
            //    {
            //        var decryptedstring = decryptor.Unprotect(payload);
            //        var payloadParts = decryptedstring.Split(":");
            //        var key = payloadParts[0];
            //        var value = payloadParts[1];
            //        var claims = new List<Claim>
            //    {
            //        new Claim("name",value)
            //    };
            //        var claimIdentity = new ClaimsIdentity(claims);
            //        context.User = new System.Security.Claims.ClaimsPrincipal(claimIdentity);
            //    }
            //    return next();
            //});

            app.UseAuthentication();

            app.Map("/username", (HttpContext ctx, IDataProtectionProvider dataProtectionProvider) =>
            {
                return ctx.User.Claims.FirstOrDefault(p => p.Type == "name").Value;
            });

            app.Map("/login", async (HttpContext authService) =>
            {
                var claims = new List<Claim>
                {
                   new Claim("name","satish")
                };
                var claimIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimIdentity);
                await authService.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,claimsPrincipal);
                return "ok";
            });

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }

    public class AuthService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDataProtectionProvider dataProtectionBuilder;

        public AuthService(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionBuilder)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.dataProtectionBuilder = dataProtectionBuilder;
        }
        public void SignIn()
        {
            var protector = dataProtectionBuilder.CreateProtector("login");
            this.httpContextAccessor.HttpContext.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:satish")}";
        }
    }
}