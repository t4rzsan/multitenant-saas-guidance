// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.Security.Policy;
using Tailspin.Surveys.Web.Security;
using Tailspin.Surveys.Web.Services;
using SurveyAppConfiguration = Tailspin.Surveys.Web.Configuration;


namespace Tailspin.Surveys.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            services.Configure<SurveyAppConfiguration.ConfigurationOptions>(options => Configuration.Bind(options));
            var configOptions = new SurveyAppConfiguration.ConfigurationOptions();
            Configuration.Bind(configOptions);
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.RequireSurveyCreator,
                    policy =>
                    {
                        policy.AddRequirements(new SurveyCreatorRequirement());
                        policy.RequireAuthenticatedUser(); // Adds DenyAnonymousAuthorizationRequirement 
                        // By adding the CookieAuthenticationDefaults.AuthenticationScheme,
                        // if an authenticated user is not in the appropriate role, they will be redirected to the "forbidden" experience.
                        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
                    });

                options.AddPolicy(PolicyNames.RequireSurveyAdmin,
                    policy =>
                    {
                        policy.AddRequirements(new SurveyAdminRequirement());
                        policy.RequireAuthenticatedUser(); // Adds DenyAnonymousAuthorizationRequirement 
                        // By adding the CookieAuthenticationDefaults.AuthenticationScheme,
                        // if an authenticated user is not in the appropriate role, they will be redirected to the "forbidden" experience.
                        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
                    });
            });

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(
                    options =>
                    {
                        Configuration.Bind("AzureAd", options);
                        options.Events = new SurveyAuthenticationEvents(loggerFactory);
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false };
                        options.Events.OnTokenValidated += options.Events.TokenValidated;
                    })
               .EnableTokenAcquisitionToCallDownstreamApi(configOptions.SurveyApi.Scope.Split(';'))
               .AddDistributedTokenCaches();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configOptions.Redis.Configuration;
                options.InstanceName = "TokenCache";
            });

            // Add Entity Framework services to the services container.
            services.AddEntityFrameworkSqlServer()
                   .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configOptions.Data.SurveysConnectionString), ServiceLifetime.Transient);

            // Register application services.
            services.AddSingleton<HttpClientService>();

            // Comment out the following line if you are using client certificates.
            services.AddTransient<ISurveyService, SurveyService>();
            services.AddTransient<IQuestionService, QuestionService>();
            services.AddTransient<SignInManager, SignInManager>();
            services.AddTransient<TenantManager, TenantManager>();
            services.AddTransient<UserManager, UserManager>();
            services.AddHttpContextAccessor();
        
            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var configOptions = new SurveyAppConfiguration.ConfigurationOptions();
            Configuration.Bind(configOptions);

            // Configure the HTTP request pipeline.
            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
