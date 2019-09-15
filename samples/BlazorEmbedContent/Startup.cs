//using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEmbedContent
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			//services.AddScoped<IToastService, ToastService>();
		}

		public void Configure(IComponentsApplicationBuilder app)
		{
			app.AddComponent<App>("app");
		}
	}
}
