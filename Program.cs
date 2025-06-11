using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using ASPProjectFinal.Models;
using System.Globalization;
namespace ASPProjectFinal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                // Insert custom model binder for decimal types
                options.ModelBinderProviders.Insert(0, new InvariantCultureModelBinderProvider());
            });

            // Register Northwind DbContext once
            builder.Services.AddDbContext<Northwind>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Northwind"))
                       .EnableSensitiveDataLogging() // Keep for debugging
                       .EnableDetailedErrors());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Orders}/{action=Index}/{id?}");

            app.Run();
        }
    }

    // Custom model binder provider for invariant culture
    public class InvariantCultureModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(decimal) ||
                context.Metadata.ModelType == typeof(decimal?))
            {
                return new InvariantCultureModelBinder();
            }
            return null;
        }
    }

    public class InvariantCultureModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"La valeur '{value}' n'est pas un nombre décimal valide.");
            }

            return Task.CompletedTask;
        }
    }
}