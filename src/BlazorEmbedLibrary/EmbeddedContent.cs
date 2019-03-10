using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorEmbedLibrary
{
	public class EmbeddedContent : ComponentBase
	{
		[Inject] IJSRuntime jSRuntime { get; set; }
		/// <summary>
		/// Displays a list of the embedded files for each assembly and extra console logging.
		/// </summary>
		[Parameter] protected bool Debug { get; set; } = false;
		/// <summary>
		/// Easy way to enable one Assembly by passing a contained type e.g. BaseType=@(typeof(Mycomponent))
		/// </summary>
		[Parameter] protected Type BaseType { get; set; }
		/// <summary>
		/// Allows multiple Assemblies to be passed as a list e.g. Assemblies=@ListOfAssemblies (where ListOfAssemblies is List<Assembly>
		/// </summary>
		[Parameter] protected List<Assembly> Assemblies { get; set; }
		/// <summary>
		/// Allows blocking/removal of any CSS file in this list e.g. BlockCssFiles=@BlockCss (where Css is List<string>)
		/// 
		/// Prefix by an assembly name and a comma to be more specific 
		/// e.g. 
		/// "Blazored.Toast,styles.css" will only block styles.css from the Blazored.Toast assembly
		/// "styles.css" will block styles.css from ANY assembly
		/// </summary>
		[Parameter] protected List<string> BlockCssFiles { get; set; }
		private bool PreRender { get; set; } = true;
		private bool HasRun;
		protected override void OnInit()
		{
			base.OnInit();
			Assemblies = Assemblies ?? new List<Assembly>();
			BlockCssFiles = BlockCssFiles ?? new List<string>();
			if (!(BaseType is null) && !Assemblies.Contains(BaseType.Assembly))
			{
				Assemblies.Add(BaseType.Assembly);
			}
		}
		protected override async Task OnAfterRenderAsync()
		{
			await base.OnAfterRenderAsync();
			if (!PreRender && !HasRun)
			{
				HasRun = true;
				await LoadEmbeddedResources();
			}

		}
		private async Task LoadEmbeddedResources()
		{
			foreach (var assembly in Assemblies)
			{

				foreach (var item in ListEmbeddedResources(assembly))
				{
					var ext = System.IO.Path.GetExtension(item).ToLower();
					if (Debug) DebugLog($"Extension: [{ext}]");
					switch (ext)
					{
						case ".css":
						case ".js":
							bool linkExists = await DoesLinkExist(assembly, item);
							bool scriptExists = await DoesScriptExist(assembly, item);
							if (ShouldBlockItem(assembly, item, ext))
							{
								if (linkExists)
								{
									var _ = await RemoveLink(assembly, item);
								}
							}
							else
							{
								if (!linkExists && !scriptExists)
								{
									string content;
									using (var stream = GetContentStream(assembly, item))
									{
										using (var sr = new System.IO.StreamReader(stream))
										{
											content = await sr.ReadToEndAsync();
										}
									}
									if (Debug) DebugLog($"Content: {content}");
									string attachName = $"{assembly.GetName().Name}.{item}";
									switch (ext)
									{
										case ".css":
											await AttachStyleSheet(attachName, content);
											break;
										case ".js":
											await AttachJavaScript(attachName, content);
											break;
										default:
											break;
									}
								}
							}
							break;
						default:
							break;
					}
				}
			}
		}

		private bool ShouldBlockItem(Assembly assembly, string item, string ext)
		{
			if (!item.ToLowerInvariant().EndsWith(".css"))
			{
				return false; //Can only block css
			}

			var assemblyName = assembly.GetName().Name;
			return BlockCssFiles
				.Where(b => !string.IsNullOrWhiteSpace(b))
				.Any(b =>
				{
					var parts = b.Split(',');
					if (parts.Length > 1)
					{
						if (parts[0].Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
						{							
							if (item.ToLowerInvariant().Contains(parts[1].ToLowerInvariant()))
							{
								return true; // matched on assemblyname and item
							}
						}
						return false;
					}
					if (item.ToLowerInvariant().Contains(b.ToLowerInvariant()))
					{
						return true; // no assembly
					}
					return false;
			});
		}

		private void DebugLog(string message)
		{
			if (Debug) Console.WriteLine(message);
		}

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{

			base.BuildRenderTree(builder);
			if (Debug)
			{
				foreach (var assembly in Assemblies )
				{

					builder.OpenElement(0, "h4");
					builder.AddContent(1, $"--- Start Embedding Files from {assembly.GetName().Name} ---");
					builder.CloseElement();
					foreach (var item in ListEmbeddedResources(assembly))
					{
						DebugLog(item);
						builder.OpenElement(3, "div");
						builder.AddContent(4, item);
						if (ShouldBlockItem(assembly, item, ".css"))
						{
							builder.AddContent(5, "** Block **");
						}
						builder.CloseElement();
					}
					builder.OpenElement(6, "h4");
					builder.AddContent(7, "--- End Embedding Files ---");
					builder.CloseElement();
				}
			}
			DetectRenderMode(builder);
		}

		private void DetectRenderMode(RenderTreeBuilder builder)
		{
			try
			{
				var btype = builder.GetType();
				var rendererFI = btype.GetField("_renderer", BindingFlags.NonPublic | BindingFlags.Instance);
				if (rendererFI is null)
				{
					PreRender = false;
					return;
				}
				var renderer = rendererFI.GetValue(builder);
				if (renderer is null)
				{
					PreRender = false;
					return;
				}
				var rendererType = renderer.GetType();
				if (rendererType is null)
				{
					PreRender = false;
					return;
				}
				var renderModeFI = rendererType.GetField("_prerenderMode", BindingFlags.NonPublic | BindingFlags.Instance);
				if (renderModeFI is null)
				{
					PreRender = false;
					return;
				}

				PreRender = (bool)renderModeFI.GetValue(renderer);
			}
			catch
			{
				// older previews didn't have pre-render
			}
		}

		private async Task AttachStyleSheet(string name, string content)
		{
			try
			{
				var blob = $"URL.createObjectURL(new Blob([\"{SafeJsString(content)}\"],{{ \"type\": \"text/css\"}}))";
				await jSRuntime.InvokeAsync<object>("eval", $"var newLink = document.createElement('link'); newLink.rel='stylesheet'; newLink.type='text/css';newLink.href={blob}; newLink.id='{SafeFileName(name)}'; document.head.appendChild(newLink);");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		private async Task AttachJavaScript(string name, string content)
		{
			try
			{
				var blob = $"URL.createObjectURL(new Blob([\"{SafeJsString(content)}\"],{{ \"type\": \"text/javascript\"}}))";
				await jSRuntime.InvokeAsync<object>("eval", $"var newScript = document.createElement('script'); newScript.src={blob}; newScript.id='{SafeFileName(name)}'; document.head.appendChild(newScript);");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		private async Task<bool> DoesLinkExist(Assembly assembly, string name)
		{
			// name will be blazor:js:somthing.js or AssemblyNameSpace.somthing.js
			string[] parts = name.Split(':');
			string fileName = parts[parts.Length - 1];
			if (parts.Length == 3)
			{
				// the name is blazor:js:somthing.js
				fileName = $"_content/{assembly.GetName().Name}/{fileName}";
			}
			string attachName = $"{assembly.GetName().Name}.{name}";
			string script = $"document.head.querySelector(\"link[id='{SafeFileName(attachName)}'],link[href='{fileName}']\")";
			DebugLog($"DoesLinkExist {name}: {script}");
			object result = null;
			try
			{
				result = await jSRuntime.InvokeAsync<object>("eval", script);
			}
			catch (Exception ex)
			{

				Console.WriteLine(ex);

			}
			bool found = !(result is null);
			DebugLog($"DoesLinkExist {name}: {found}");
			return found;
		}
		private async Task<bool> RemoveLink(Assembly assembly, string name)
		{
			// name will be blazor:js:somthing.js or AssemblyNameSpace.somthing.js
			string[] parts = name.Split(':');
			string fileName = parts[parts.Length - 1];
			if (parts.Length == 3)
			{
				// the name is blazor:js:somthing.js
				fileName = $"_content/{assembly.GetName().Name}/{fileName}";
			}
			string attachName = $"{assembly.GetName().Name}.{name}";
			string script = $"const el=document.head.querySelector(\"link[id='{SafeFileName(attachName)}'],link[href='{fileName}']\");el.disabled = true;el.remove();";
			DebugLog($"RemoveLink {name}: {script}");
			object result = null;
			try
			{
				result = await jSRuntime.InvokeAsync<object>("eval", script);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			bool found = !(result is null);
			DebugLog($"RemoveLink {name}: {found}");
			return found;
		}

		private async Task<bool> DoesScriptExist(Assembly assembly, string name)
		{
			// name will be blazor:js:somthing.js or AssemblyNameSpace.somthing.js
			string[] parts = name.Split(':');
			string fileName = parts[parts.Length - 1];
			if (parts.Length == 3)
			{
				// the name is blazor:js:somthing.js
				fileName = $"_content/{assembly.GetName().Name}/{fileName}";
			}
			string attachName = $"{assembly.GetName().Name}.{name}";
			string script = $"document.head.querySelector(\"script[id='{SafeFileName(attachName)}'],script[src='{fileName}']\")";
			DebugLog($"DoesScriptExist {name}: {script}");
			object result = null;
			try
			{
				result = await jSRuntime.InvokeAsync<object>("eval", script);
			}
			catch (Exception ex)
			{

				Console.WriteLine(ex);

			}
			bool found = !(result is null);
			DebugLog($"DoesScriptExist {name}: {found}");
			return found;
		}
		private async Task<bool> RemoveScript(Assembly assembly, string name)
		{
			// name will be blazor:js:somthing.js or AssemblyNameSpace.somthing.js
			string[] parts = name.Split(':');
			string fileName = parts[parts.Length - 1];
			if (parts.Length == 3)
			{
				// the name is blazor:js:somthing.js
				fileName = $"_content/{assembly.GetName().Name}/{fileName}";
			}
			string attachName = $"{assembly.GetName().Name}.{name}";
			string script = $"const el=document.head.querySelector(\"script[id='{SafeFileName(attachName)}'],script[src='{fileName}']\");el.disabled=true;el.remove();";
			DebugLog($"RemoveScript {name}: {script}");
			object result = null;
			try
			{
				result = await jSRuntime.InvokeAsync<object>("eval", script);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			bool found = !(result is null);
			DebugLog($"RemoveScript {name}: {found}");
			return found;
		}

		private IEnumerable<string> ListEmbeddedResources(Assembly assembly)
		{
			var resources = assembly.GetManifestResourceNames();
			DebugLog($"Got resources: {string.Join(", ", resources)}");
			DebugLog($"Using assembly: {assembly.GetName().Name}");
			foreach (var item in resources)
			{
				yield return item;
			}
		}

		private System.IO.Stream GetContentStream(Assembly assembly, string name)
		{
			DebugLog($"GetContentStream for {name} from assembly: {assembly.GetName().Name}");
			return assembly.GetManifestResourceStream(name);
		}

		string SafeFileName(string name) => name.Replace(":", "_");

		string SafeJsString(string content) => content.Replace(@"\", @"\\").Replace("\r", @"\r").Replace("\n", @"\n").Replace("'", @"\'").Replace("\"", @"\""");

	}
}
