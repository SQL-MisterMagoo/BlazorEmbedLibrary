using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorEmbedLibrary
{
	public class EmbeddedContent : ComponentBase
	{
		[Inject] IJSRuntime jSRuntime { get; set; }
		[Parameter] protected bool Debug { get; set; } = false;
		[Parameter] protected Type BaseType { get; set; }

		protected override async Task OnInitAsync()
		{
			foreach (var item in ListEmbeddedResources(BaseType))
			{
				var ext = System.IO.Path.GetExtension(item).ToLower();
				if (Debug) DebugLog($"Extension: [{ext}]");
				switch (ext)
				{
					case ".css":
					case ".js":
						if (!(await DoesLinkExist(BaseType,item)) && !(await DoesScriptExist(BaseType,item)))
						{
							string content;
							using (var stream = GetContentStream(BaseType, item))
							{
								using (var sr = new System.IO.StreamReader(stream))
								{
									content = await sr.ReadToEndAsync();
								}
							}
							if (Debug) DebugLog($"Content: {content}");
							switch (ext)
							{
								case ".css":
									await AttachStyleSheet(item, content);
									break;
								case ".js":
									await AttachJavaScript(item, content);
									break;
								default:
									break;
							}
						}
						break;
					default:
						break;
				}
			}
		}

		private void DebugLog(string message)
		{
			if (Debug) Console.WriteLine(message);
		}

		protected override bool ShouldRender()
		{
			return Debug;
		}
		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			base.BuildRenderTree(builder);
			if (Debug)
			{
				builder.OpenElement(0, "h4");
				builder.AddContent(1, "--- Embedded Files ---");
				builder.CloseElement();
				builder.OpenElement(2, "ul");
				foreach (var item in ListEmbeddedResources(BaseType))
				{
					DebugLog(item);
					builder.OpenElement(3, "li");
					builder.AddContent(4, item);
					builder.CloseElement();
				}
				builder.CloseElement();
				builder.OpenElement(0, "h4");
				builder.AddContent(1, "--- Embedded Files ---");
				builder.CloseElement();
			}
		}

		public async Task AttachStyleSheet(string name, string content)
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

		public async Task AttachJavaScript(string name, string content)
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

		public async Task<bool> DoesLinkExist(Type type, string name)
		{
			// name will be blazor:js:somthing.js or AssemblyNameSpace.somthing.js
			string[] parts = name.Split(':');
			string fileName = parts[parts.Length - 1];
			if (parts.Length == 3)
			{
				// the name is blazor:js:somthing.js
				fileName = $"_content/{type.Assembly.GetName().Name}/{fileName}";
			}
			string script = $"document.head.querySelector(\"link[id='{SafeFileName(name)}'],link[href='{fileName}']\")";
			DebugLog($"DoesLinkExist {name}: {script}");
			var result = await jSRuntime.InvokeAsync<object>("eval", script);
			bool found = !(result is null);
			DebugLog($"DoesLinkExist {name}: {found}");
			return found;
		}

		public async Task<bool> DoesScriptExist(Type type, string name)
		{
			// name will be blazor:js:somthing.js or AssemblyNameSpace.somthing.js
			string[] parts = name.Split(':');
			string fileName = parts[parts.Length - 1];
			if (parts.Length==3)
			{
				// the name is blazor:js:somthing.js
				fileName = $"_content/{type.Assembly.GetName().Name}/{fileName}";
			}
			string script = $"document.head.querySelector(\"script[id='{SafeFileName(name)}'],script[src='{fileName}']\")";
			DebugLog($"DoesScriptExist {name}: {script}");
			var result = await jSRuntime.InvokeAsync<object>("eval", script);
			bool found = !(result is null);
			DebugLog($"DoesScriptExist {name}: {found}");
			return found;
		}

		public IEnumerable<string> ListEmbeddedResources(Type type)
		{
			var resources = type.Assembly.GetManifestResourceNames();
			Console.WriteLine($"Got resources: {string.Join(", ",resources)}");
			DebugLog($"Using type: {type.Name} from {type.Assembly.GetName().Name}");
			foreach (var item in resources)
			{
				yield return item;
			}
		}

		public System.IO.Stream GetContentStream(Type type, string name)
		{
			DebugLog($"GetContentStream for {name} from type: {type.Name} from {type.Assembly.GetName().Name}");
			return type.Assembly.GetManifestResourceStream(name);
		}

		string SafeFileName(string name) => name.Replace(":", "_");
		
		string SafeJsString(string content) => content.Replace(@"\", @"\\").Replace("\r", @"\r").Replace("\n", @"\n").Replace("'", @"\'").Replace("\"", @"\""");
	}
}
