using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using YukiToolkit.DataStructures;
using YukiToolkit.UsefulConsts;

namespace Snowbow {
	public record SiteConfig {
        public Uri BaseUrl { get; init; }
        public string Theme { get; init; }
        public string DefaultLanguage { get; init; }
        public Dictionary<string, SiteLanguageConfig> Language { get; init; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Extra { get; init; }

        public static SiteConfig Read() {
			return JsonConvert.DeserializeObject<SiteConfig>(File.ReadAllText("site-config.json", ConstStuff.UniversalUtf8Encoding), Helper.MyJsonSerializerSettings)!;
        }
    }
    public record SiteLanguageConfig {
        public string Title { get; init; }
        public string Description { get; init; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Extra { get; init; }
    }
}
