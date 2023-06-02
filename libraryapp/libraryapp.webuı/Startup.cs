
using FluentValidation.AspNetCore;
using libraryapp.core.Logger;
using libraryapp.dataaccess.Concrete.EfCore;
using libraryapp.dataaccess.Concrete.EfCore.Context.IdentityContext;
using libraryapp.dataaccess.Concrete.EfCore.Context.LibraryContext;
using libraryapp.entity.IdentityEntities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Hosting;
using System;
using libraryapp.core.Services;
using libraryapp.core.RouteBuilder;
using Microsoft.AspNetCore.SignalR;
using libraryapp.core.Hubs;

namespace libraryapp.webuı
{
    public class Startup
    {


        private IConfiguration configuration;
        public Startup(IConfiguration _configuration)
        {
            configuration = _configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) // ICOnfiguration config kaldırınca ki zaten kendim eklemiştirm düzeldi 
        // f5  ile programı çalıştırınca bazen hata beriyor!
        {

            services.AddSignalR();

            // services.AddMvc(x => x.EnableEndpointRouting = false);
            services.AddControllersWithViews().AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<Startup>());
            /*-------------------------------------------------*/
            services.AddDbContext<LibraryContext>(options =>
            options.UseMySql("server=localhost;port=3306;database=librarydb;user=root;password=nyks6957!")
            );
            #region Identity
            /*-----------------Identity servisleri-------------*/
            services.AddDbContext<IdentityContext>(options =>
            options
            .UseMySql("server=localhost;port=3306;database=identitydb;user=root;password=nyks6957!")
            );

            services.AddIdentity<User, Role>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.ClaimsIdentity = new ClaimsIdentityOptions() { SecurityStampClaimType = "AspNet.Identity.SecurityStamp" };
            }
            ).AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();
            // services.AddScoped<IUserClaimsPrincipalFactory<User>, myAppUserClaimsPrincipalFactory>();
            /*-----------------------------------------------------*/
            services.AddAuthentication()
                .AddCookie("AuthCookie", configureOptions =>
                {
                    configureOptions.Cookie.Name = "MyAuthCookie";
                    configureOptions.LoginPath = "/admin/account/login";
                    configureOptions.AccessDeniedPath = "/admin/account/accessdenied";
                })
                .AddFacebook(opt =>
                {
                    opt.AppId = configuration.GetSection("Authentication:Facebook:AppId").Value;
                    opt.AppSecret = configuration.GetSection("Authentication:Facebook:AppSecret").Value;
                    //opt.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/admin/account/FacebookResponse");
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("developeronly", policy => policy.RequireClaim("claimtype:developer", "claimvalue:nyks6957!"));

            });
            /*---------------------------------------------------*/
            services.ConfigureApplicationCookie(option => option.LoginPath = "/admin/account/login");
            services.ConfigureApplicationCookie(options => options.AccessDeniedPath = "/admin/account/accessdenied");
            #endregion

            /*-----------------Data acces && Buisness Injections--------------------*/
            services.RegisterMyServices();
            /*----------------------------------------------------------------------*/
            services.AddSession();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IHostEnvironment hostEnvironment,
            IServiceProvider serviceProvider,
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment webHostEnvironment,
            ILoggerFactory loggerFactory
            )
        {
            loggerFactory.AddProvider(new LoggerProvider(hostEnvironment));

            if (webHostEnvironment.IsDevelopment())
            {
                SeedDatabase.SeedLibraryData();
                applicationBuilder.UseDeveloperExceptionPage();
            }

            /*builder design pattern!*/
            applicationBuilder.UseRouting();
            applicationBuilder.UseAuthentication().UseAuthorization();
            applicationBuilder.UseSession().UseStaticFiles();
            applicationBuilder.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "node_modules")),
                    RequestPath = "/node_modules"
                }
                );


           
            applicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.RegisterMyRoutes();
                endpoints.MapHub<CommentHub>("/commentHub");

            });


        }
    }

}
