using Microsoft.AspNetCore.DataProtection;

namespace AuthCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDataProtection();
            var app = builder.Build();


            app.Map("/username", (HttpContext ctx,IDataProtectionProvider dataProtectionProvider) =>
            {
                var decryptor = dataProtectionProvider.CreateProtector("login");
                var payload = ctx.Request.Cookies["auth"];
                var decryptedstring = decryptor.Unprotect(payload);
                var payloadParts = decryptedstring.Split(":");
                var key = payloadParts[0];
                var value = payloadParts[1];
                return value;
            });

            app.Map("/login", (HttpContext ctx,IDataProtectionProvider provider) =>
            {
                var encryptor = provider.CreateProtector("login");
                ctx.Response.Headers["set-cookie"] = $"auth={ encryptor.Protect("usr:satish")}";
                return "ok";
            });

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}