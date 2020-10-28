using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using WebApi.Data;
using WebApi.Services;
using WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using System.Text;
using System.Net;

namespace WebApi
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

            //DBContext and SQL connection
            services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<User>()
             .AddRoles<Role>()
             .AddEntityFrameworkStores<DataContext>();

            services.Configure<IdentityOptions>(options =>
            {
                //Email validation
                //  options.SignIn.RequireConfirmedEmail = true;
                // Password settings.
                options.Password.RequireDigit = true;
              //  options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
              //  options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
             //   options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            //MVC
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddTransient<AccountService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                  .AddJwtBearer(options =>
                  {

                      var signingKey = Convert.FromBase64String(Configuration["Jwt:SigningSecret"]);
                      options.TokenValidationParameters = new TokenValidationParameters
                      {
                          ValidateIssuer = false,
                          ValidateAudience = false,
                          ValidateIssuerSigningKey = true,
                          IssuerSigningKey = new SymmetricSecurityKey(signingKey)
                      };
                      options.Events = new JwtBearerEvents
                      {
                          OnAuthenticationFailed = context =>
                          {
                              Console.WriteLine("OnAuthenticationFailed: " +
                                  context.Exception.Message);
                              return Task.CompletedTask;
                          },
                          OnTokenValidated = context =>
                          {
                              Console.WriteLine("OnTokenValidated: " +
                                  context.SecurityToken);
                              return Task.CompletedTask;
                          }
                      };


                  });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Formatting = Formatting.Indented;
            });

            //services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfig>());

            //services.AddTransient<IEmailService, EmailService>();
            services.AddOptions();

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICalendarService, CalendarService>();
            services.AddScoped<IHelperService, HelperService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2", new Info { Title = "FMR API", Version = "v2.0" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var ex = error.Error;
                        await context.Response.WriteAsync(new ErrorDto()
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = ex.Message
                        }.ToString(), Encoding.UTF8);
                    }
                });
            });

            
            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "FMR API V2.0");
            });
        }
    }
}
