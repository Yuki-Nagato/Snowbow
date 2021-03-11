using RazorLight;
using System;
using System.Threading.Tasks;

namespace Snowbow {
	class Program {
		static async Task Main(string[] args) {
			var engine = new RazorLightEngineBuilder()
				.UseEmbeddedResourcesProject(System.Reflection.Assembly.GetEntryAssembly())
				.UseMemoryCachingProvider()
				.Build();

			string template = "<div>@Model.Content</div>";
			var model = new { Content = "長門有希 <^_^>" };

			string result = await engine.CompileRenderStringAsync("templateKey", template, model);
			Console.WriteLine(result);
		}
	}
}
