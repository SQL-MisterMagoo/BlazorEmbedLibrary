using Blazored.LocalStorage;
using Blazored.Toast.Services;
using BlazorEmbedLibrary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorComponentsSample.Server.Components;
using RazorComponentsSample.Server.Services;
using System.Collections.Generic;
using System.Reflection;

namespace RazorComponentsSample.Server
{
	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc()
					.AddNewtonsoftJson();

			services.AddRazorComponents();

			services.AddSingleton<WeatherForecastService>();
			services.AddScoped<ILocalStorageService, LocalStorageService>();
			services.AddScoped<IToastService, ToastService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

			//app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseStaticFiles(new StaticFileOptions()
			{
				FileProvider = new BlazorFileProvider(new List<Assembly>() { typeof(BlazorComponentSample.Component1).Assembly })
			});

			app.UseRouting(routes =>
			{
				routes.MapRazorPages();
				routes.MapComponentHub<App>("app");
			});
		}
	}
}
