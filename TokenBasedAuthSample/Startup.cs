using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NETCore.Encrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TokenBasedAuthSample.identity;
using TokenBasedAuthSample.Services;

namespace TokenBasedAuthSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddControllersWithViews();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TokenBasedAuthSample", Version = "v1" });
            });

            //services.AddIdentity<MyUser, MyRole>();

            // login olurken kullan�lan �ema 
            // Bir uygulamada farkl� �ekillerde birden fazla login yap�labilmesi i�in farkl� isimlerde �emaya ihtiya� var.
            services.AddAuthentication().AddJwtBearer("myScheme",opt =>
            {

                opt.SaveToken = true;
               
                

               string issuer =  EncryptProvider.AESDecrypt(EncrptedHelper.Replace(Configuration["JWT:issuer"]), ConfigurationEncryptionTypes.SecretKey, ConfigurationEncryptionTypes.VectorKey);

                string audience = EncryptProvider.AESDecrypt(EncrptedHelper.Replace(Configuration["JWT:audience"]), ConfigurationEncryptionTypes.SecretKey, ConfigurationEncryptionTypes.VectorKey);


                string signingKey = EncryptProvider.AESDecrypt(EncrptedHelper.Replace(Configuration["JWT:signingKey"]), ConfigurationEncryptionTypes.SecretKey, ConfigurationEncryptionTypes.VectorKey);

                // access token control mekanizmas�

                opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true, // issuer bilgisi yanl�� g�nderilierse accesstoken onaylanamaz
                    ValidateAudience = true, // audience yanl�� g�nderilirse accesstoken onaylanmaz
                    ValidateLifetime = true, // exipire olursa onaylanmaz
                    ValidateIssuerSigningKey = true, // singkey yanl�� ise onaylanmaz
                    ValidIssuer = issuer, // issuer de�eri
                    ValidAudience = audience, // audince de�eri
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)) // private key
                };

            });


            services.AddAuthentication(o => {

                o.DefaultScheme = "google";
                o.DefaultSignInScheme = "External";
               
                // External d�� bir kayna�a y�nledirmek i�in kullan�lan scheme ismi External bu isim default bir isim bu sayede uygulama harici bir kaynaktan login olaca��m�z� anl�yor.
                // Schema ismini de�i�tireebiliriz login olurkende bu ismi kullanmaya dikkat etmemiz laz�m

            })
            .AddCookie("google")
            .AddCookie("External")
            .AddGoogle(googleOptions =>
            {
                googleOptions.SaveTokens = true;
                googleOptions.ClientId = "239647126082-jlaa7t5r9l5d0ba9ejr0inofv4bjq23q.apps.googleusercontent.com";
                googleOptions.ClientSecret = "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";
            });

            // farkl� bir jwt �emas� da tan�mlad�k.
            //services.AddAuthentication().AddJwtBearer("google",opt => { });

            services.AddSingleton<ITokenService, JwtTokenService>();


            // bu kontrol i�in kullan�c�n�n type email olan bir clai�m olmas� laz�m
            // role d���ndaki t�m claimler i�in policy tan�mlamas� yapar�z.
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("emailRequired", policy =>
                {
                    policy.RequireAuthenticatedUser().RequireClaim("emailaddress");
                });
            });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("AgeRequired", policy =>
                {
                    policy.RequireClaim("age");
                });
            });

            // value'su �zerinden claim kontol�
            // sadece bu email hesab�na sahip olanlar girebilir.
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("SpesificEmailAddress", policy =>
                {
                    policy.RequireClaim("emailaddress","mert@test.com","test@test.com","mert.alptekin@neominal.com");
                });
            });

            //services.AddTransient<SignInManager<MyUser>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TokenBasedAuthSample v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
            });
        }
    }
}
