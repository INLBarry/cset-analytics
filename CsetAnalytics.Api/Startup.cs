using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CsetAnalytics.Business;
using CsetAnalytics.Business.Analytics;
using CsetAnalytics.DomainModels;
using CsetAnalytics.DomainModels.Models;
using CsetAnalytics.Factories;
using CsetAnalytics.Factories.Analytics;
using CsetAnalytics.Interfaces;
using CsetAnalytics.Interfaces.Analytics;
using CsetAnalytics.Interfaces.Factories;
using CsetAnalytics.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;


namespace CsetAnalytics.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; set; }
        private IConfigurationRoot _config;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true,
                    reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = configuration;
            this.HostingEnvironment = hostingEnvironment;
            _config = builder.Build();
        }

        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton(_config);

            //var mappingConfig = new MapperConfiguration(mc =>
            //{
            //    mc.AddProfile(new FactoryProfile());
            //});
            //IMapper mapper = mappingConfig.CreateMapper();

            services.AddAutoMapper(typeof(FactoryProfile));

            //services.AddSingleton(mapper);

            

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .SetIsOriginAllowed((host) => true)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
            });
            services.AddDbContext<CsetContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("CsetConnection"), b=>b.MigrationsAssembly("CsetAnalytics.DomainModels")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<CsetContext>()
                .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(Int32.Parse(Configuration["Login:DefaultLockoutTimespan"]));
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            });

            var key = Encoding.ASCII.GetBytes(Configuration["Tokens:Key"]);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true, 
                    ValidateAudience = true, 
                    ValidIssuer = Configuration["Tokens:Issuer"], 
                    ValidAudience = Configuration["Token:Audience"], 
                    ValidateIssuerSigningKey = true, 
                    IssuerSigningKey = new SymmetricSecurityKey(key), 
                    ValidateLifetime = true
                };
            });

            //Business
            services.AddTransient<IUserBusiness, UsersBusiness>();
            services.AddTransient<IAnalyticBusiness, AnalyticsBusiness>();

            //Factories
            services.AddTransient<IBaseFactoryAsync<AnalyticDemographicViewModel, AnalyticDemographic>, AnalyticDemographicModelFactory>();
            services.AddTransient<IBaseFactoryAsync<AnalyticDemographic, AnalyticDemographicViewModel>, AnalyticDemographicViewModelFactory>();
            services.AddTransient<IBaseFactoryAsync<AnalyticQuestionViewModel, AnalyticQuestion>, AnalyticQuestionModelFactory>();
            services.AddTransient<IBaseFactoryAsync<AnalyticQuestion, AnalyticQuestionViewModel>, AnalyticQuestionViewModelFactory>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<CsetContext>();
                context.Database.Migrate();
            }
            app.UseCors("AllowAll");
            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
