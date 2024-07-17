using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Core.Data.EF;
using Core.Entities;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Presentation.Site.Auth;
using Presentation.Site.Auth.Jwt;
using Presentation.Site.Bootstraping;

namespace Presentation.Site
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

            services.AddDbContext<DataContext>(options =>options
                .UseSqlServer(Configuration.GetConnectionString("Sql")));

            ConfigureAuth(services);

            //services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<DataContext>();

            // services.AddMvc()
            //    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            //    .AddFluentValidation(options =>
            //    {
            //        options.RegisterValidatorsFromAssembly(Assembly.Load("Core"));
            //    });

            services.AddMvc();
            services.AddFluentValidationAutoValidation();
            services.AddControllersWithViews();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen();

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        #region Auth Configuration

        private void ConfigureAuth(IServiceCollection services)
        {
            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));
            var jwtSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions[nameof(JwtIssuerOptions.SecretKey)]));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(jwtSigningKey, SecurityAlgorithms.HmacSha256);
                options.ValidFor = TimeSpan.Parse(jwtAppSettingOptions[nameof(JwtIssuerOptions.ValidFor)]);
                options.RefreshTokenValidFor = TimeSpan.Parse(jwtAppSettingOptions[nameof(JwtIssuerOptions.RefreshTokenValidFor)]);
                options.SecretKey = jwtAppSettingOptions[nameof(JwtIssuerOptions.SecretKey)];
            });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(configureOptions =>
                {
                    configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                    
                    configureOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                        ValidateAudience = true,
                        ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = jwtSigningKey,

                        RequireExpirationTime = false,
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.Zero
                    };
                    
                    configureOptions.SaveToken = true;
                    
                    
                    //configureOptions.IncludeErrorDetails = Env.IsDevelopment();
                }).AddCookie(IdentityConstants.ApplicationScheme);

            /*
            // api user claim policy
            services.AddAuthorization(options =>
            {
                AddPolicyClaims(options);
            });
            */
            var builder = services.AddIdentityCore<User>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            });
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services).AddRoles<IdentityRole>();
            builder.AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
            builder.AddSignInManager<SignInManager<User>>();
            services.AddSingleton<IJwtFactory, JwtFactory>();
        }

        private void AddPolicyClaims(AuthorizationOptions options)
        {
            // TODO: Make Policies dynamic to avoid having to
            // modify this list for each distinct permission we have
            // ref: https://www.jerriepelser.com/blog/creating-dynamic-authorization-policies-aspnet-core/
            foreach (var permission in Auth.Helpers.Permissions)
            {
                options.AddPolicy(permission.Name, policy =>
                {
                    policy.AddRequirements(new ConfirmAccountRequirement());

                    policy.RequireClaim("permission", permission.Name);
                });
            }
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new BootstrapperModule(Configuration));
        }

        #endregion

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/TiendaNube/VerError");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            
            
            app.UseSession();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
