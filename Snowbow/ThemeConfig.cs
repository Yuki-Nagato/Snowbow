using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using YukiToolkit.UsefulConsts;

namespace Snowbow {
	public record ThemeConfig {
		public Dictionary<string, IDictionary<string, string>> Translation { get; init; }

		[JsonExtensionData]
		public Dictionary<string, JToken> Extra { set; get; }

		public static ThemeConfig Read(SiteConfig siteConfig) {
			return JsonConvert.DeserializeObject<ThemeConfig>(File.ReadAllText("themes/" + siteConfig.Theme + "/theme-config.json", ConstStuff.UniversalUtf8Encoding), Helper.MyJsonSerializerSettings)!;
		}
	}
}
