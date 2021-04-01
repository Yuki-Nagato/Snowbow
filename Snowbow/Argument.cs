using CommandLine;
using System;
using System.IO;

namespace Snowbow {
	[Verb("build")]
	public class BuildOptions {

		[Option(shortName: 'd', longName: "directory", Required = false)]
		public DirectoryInfo Directory { get; set; }
	}
	[Verb("server")]
	public class ServerOptions {
		[Option(shortName: 'd', longName: "directory", Required = false)]
		public DirectoryInfo Directory { get; set; }
	}

	public static class Argument {
		public static string Verb { get; private set; }
		public static void MakeEffect(string[] args) {
			Parser.Default.ParseArguments<BuildOptions, ServerOptions>(args)
				.WithParsed<BuildOptions>(buildOptions => {
					if (buildOptions.Directory != null) {
						Environment.CurrentDirectory = buildOptions.Directory.FullName;
					}
					else {
						Logger.Log("Directory not specified, use current directory " + Environment.CurrentDirectory);
					}
					Verb = "build";
				})
				.WithParsed<ServerOptions>(serverOption => {
					if (serverOption.Directory != null) {
						Environment.CurrentDirectory = serverOption.Directory.FullName;
					}
					else {
						Logger.Log("Directory not specified, use current directory " + Environment.CurrentDirectory);
					}
					Verb = "server";
				})
				.WithNotParsed(errors => {
					Logger.Log(errors.ToString());
				});
		}
	}
}