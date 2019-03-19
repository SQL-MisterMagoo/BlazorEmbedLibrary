using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorEmbedLibrary
{
	public class BlazorFileProvider : IFileProvider
	{
		/// <summary>
		/// Displays a list of the embedded files for each assembly and extra console logging.
		/// </summary>
		public bool Debug { get; set; } = false;
		/// <summary>
		/// Allows multiple Assemblies to be passed as a list e.g. Assemblies=@ListOfAssemblies (where ListOfAssemblies is List<Assembly>
		/// </summary>
		private List<Assembly> Assemblies { get; set; }

		private Dictionary<string, Assembly> fileMap;
		private Dictionary<string, Assembly> assMap;
		private Dictionary<string, string> nameMap;
		public BlazorFileProvider(List<Assembly> assemblies)
		{
			Assemblies = assemblies ?? new List<Assembly>();
			fileMap = new Dictionary<string, Assembly>();
			assMap = new Dictionary<string, Assembly>();
			nameMap = new Dictionary<string, string>();
			LoadEmbeddedResources();
		}
		private void LoadEmbeddedResources()
		{
			foreach (var assembly in Assemblies)
			{
				string name = assembly.GetName().Name;
				assMap.Add(name, assembly);
				foreach (var item in ListEmbeddedResources(assembly))
				{
					string key = Path.GetFileName(item.Replace(":","/"));
					if (nameMap.ContainsKey(key))
					{
						DebugLog($"BFP: Duplicate resource - unable to add {key} from {name}");
					}
					else
					{
						fileMap.Add(key, assembly);
						nameMap.Add(key, item);
						DebugLog($"BFP: Mapped {name}.{item} as {key}");
					}
				}
			}
		}

		private void DebugLog(string message)
		{
			if (Debug) Console.WriteLine(message);
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

		public IFileInfo GetFileInfo(string subpath)
		{
			DebugLog($"BFP: GetFileInfo({subpath})");
			var key = Path.GetFileName(subpath);
			if (nameMap.ContainsKey(key))
			{
				return new BlazorFileInfo(nameMap[key], key, fileMap[key]);
			}
			return new NotFoundFileInfo(subpath);
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			DebugLog($"BFP: GetDirectoryContents({subpath})");
			var parts = subpath.Split('/');
			string root;
			if (string.IsNullOrEmpty(parts[0]) && parts.Length>2)
			{
				root = parts[1];
			} else
			{
				root = parts[0];
			}
			
			if (root.Equals("_content"))
			{
				var name = parts[parts.Length-1];
				return new BlazorDirectoryContents(assMap[name]);

			}
			return new NotFoundDirectoryContents();
		}

		public IChangeToken Watch(string filter)
		{
			throw new NotImplementedException();
		}

	}
}
