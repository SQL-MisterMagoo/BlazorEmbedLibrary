using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace BlazorEmbedLibrary
{
	internal class BlazorDirectoryContents : IDirectoryContents
	{
		private readonly Assembly assembly;

		public BlazorDirectoryContents(Assembly assembly)
		{
			this.assembly = assembly;
		}
		public bool Exists => true;

		public IEnumerator<IFileInfo> GetEnumerator()
		{
			var resources = assembly.GetManifestResourceNames();
			foreach (var item in resources)
			{
				var name = Path.GetFileName(item.Replace(":", "/"));
				yield return new BlazorFileInfo(item, name, assembly);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new System.NotImplementedException();
		}
	}
}