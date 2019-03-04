using Blazored.LocalStorage;
using Blazored.Toast;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using RazorComponentsSample.App.Services;

namespace RazorComponentsSample.App
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			// Example of a data service
			services.AddSingleton<WeatherForecastService>();
			services.AddScoped<ILocalStorageService, LocalStorageService>();
			services.AddScoped<IToastService, ToastService>();
		}

		public void Configure(IComponentsApplicationBuilder app)
		{
			app.AddComponent<App>("app");
		}
	}
}
