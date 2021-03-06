﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApplication.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BlogApplication.Data.Repository;

namespace BlogApplication
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddTransient<IRepository, Repository>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Create Post", policy => policy.RequireClaim("Create Post", "accepted"));
   
            });
            services.AddAuthorization(options =>
            {
               
                options.AddPolicy("Panel", policy => policy.RequireClaim("Panel", "accepted"));
               
            });
            services.AddAuthorization(options =>
            {
               
                options.AddPolicy("Edit Post", policy => policy.RequireClaim("Edit Post", "accepted"));
                
            });
            services.AddAuthorization(options =>
            {
               
                options.AddPolicy("View Post", policy => policy.RequireClaim("View Post", "accepted"));
             
            });

            services.AddAuthorization(options =>
            {
              
                options.AddPolicy("Comment", policy => policy.RequireClaim("Comment", "accepted"));
              
            });

            services.AddAuthorization(options =>
            {
               
                options.AddPolicy("Remove Post", policy => policy.RequireClaim("Remove Post", "accepted"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            DbSeeder.SeedDb(userManager, roleManager);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Post}/{id?}");
            });
        }
    }
}
