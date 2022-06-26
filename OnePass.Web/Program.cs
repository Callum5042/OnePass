using Microsoft.AspNetCore.Mvc.Razor;

namespace OnePass.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = BuildWebApplication(args);
            ConfigureApp(app);
            app.Run();
        }

        private static WebApplication BuildWebApplication(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure routing
            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            // Add MVC
            builder.Services.AddControllersWithViews();

            // Minify js/css
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddWebOptimizer(minifyJavaScript: false, minifyCss: false);
            }
            else
            {
                builder.Services.AddWebOptimizer();
            }

            // Customise razor view search locations
            builder.Services.Configure<RazorViewEngineOptions>(options =>
            {
                // {2} is area, {1} is controller,{0} is the action
                options.ViewLocationFormats.Clear();
                options.ViewLocationFormats.Add("/site/{1}/{0}/{0}" + RazorViewEngine.ViewExtension);
                options.ViewLocationFormats.Add("/site/{1}/{0}" + RazorViewEngine.ViewExtension);
                options.ViewLocationFormats.Add("/site/shared/{0}" + RazorViewEngine.ViewExtension);
                options.ViewLocationFormats.Add("/site/{1}/{0}/_{0}" + RazorViewEngine.ViewExtension);

                options.AreaViewLocationFormats.Clear();
                options.AreaViewLocationFormats.Add("/site/{2}/{1}/{0}/{0}" + RazorViewEngine.ViewExtension);
                options.AreaViewLocationFormats.Add("/site/{2}/{1}/{0}" + RazorViewEngine.ViewExtension);
                options.AreaViewLocationFormats.Add("/site/{2}/{0}" + RazorViewEngine.ViewExtension);
                options.AreaViewLocationFormats.Add("/site/shared/{0}" + RazorViewEngine.ViewExtension);
            });

            // Add services to the container.
            return builder.Build();
        }

        private static void ConfigureApp(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseHttpsRedirection();
            app.UseWebOptimizer();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=dashboard}/{action=index}/{id?}");
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=home}/{action=index}/{id?}");
            });
        }
    }
}
