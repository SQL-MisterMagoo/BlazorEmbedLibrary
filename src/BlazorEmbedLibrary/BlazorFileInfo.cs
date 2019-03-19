using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace BlazorEmbedLibrary
{
	internal class BlazorFileInfo : IFileInfo
	{
		private readonly string subpath;
		private readonly string filename;
		private readonly Assembly assembly;
		public BlazorFileInfo(string path, string name, Assembly assembly)
		{
			this.subpath = path;
			this.filename = name;
			this.assembly = assembly;
		}

		public bool Exists => true;

		public long Length => assembly.GetManifestResourceStream(subpath).Length;

		public string PhysicalPath => $"/{filename}";

		public string Name => filename;

		public DateTimeOffset LastModified => DateTimeOffset.FromFileTime( int.Parse(assembly.GetName().Version.ToString().Replace(".","")));

		public bool IsDirectory => false;

		public Stream CreateReadStream()
		{
			return assembly.GetManifestResourceStream(subpath);
		}
	}
}