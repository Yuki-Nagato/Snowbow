using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using static Snowbow.Subprocess;

namespace Snowbow {
	public class RuntimeModel {
        private readonly string _noLanguagePath;
        public SiteConfig SiteConfig { get; }

        public ThemeConfig ThemeConfig { get; }

        public Dictionary<string, List<RuntimeModel>> Articles { get; }

        public int ArticleIndex { get; internal set; }

        public string? ArticleCommonName { get; }

        public string Layout { get; }

        public string? Language { get; }

        public string Title { get; }

        public DateTimeOffset Time { get; }

        public TableOfContentsItem[]? TableOfContents { get; }

        public string? Content { get; }

        public string? PartialContent { get; }

        public JToken FrontMatter { get; }

        public string T(string value) {
            if (Language != null)
                return ThemeConfig.Translation[Language][value];
            else
                return ThemeConfig.Translation[SiteConfig.DefaultLanguage][value];
        }

        public string ALP(string path) {
            if (Language != null)
                return SiteConfig.BaseUrl.LocalPath + Language + path;
            else
                return SiteConfig.BaseUrl.LocalPath + SiteConfig.DefaultLanguage + path;
        }

        public string AP(string path) {
            return SiteConfig.BaseUrl.LocalPath + path.TrimStart('/');
        }

        public string AbsolutePath {
            get {
                if (Language != null)
                    return SiteConfig.BaseUrl.LocalPath + Language + _noLanguagePath;
                else
                    return SiteConfig.BaseUrl.LocalPath + _noLanguagePath.TrimStart('/');
            }
        }

        public string TranslationPath(string language) {
            return SiteConfig.BaseUrl.LocalPath + language + _noLanguagePath;
        }

        public string Permalink {
            get {
                if (Language != null)
                    return SiteConfig.BaseUrl.OriginalString + Language + _noLanguagePath;
                else
                    return SiteConfig.BaseUrl.OriginalString + _noLanguagePath.TrimStart('/');
            }
        }

        public SiteLanguageConfig SiteLanguageConfig {
            get {
                if (Language != null)
                    return SiteConfig.Language[Language];
                else
                    return SiteConfig.Language[SiteConfig.DefaultLanguage];
            }
        }

        public RuntimeModel(string noLanguagePath, SiteConfig siteConfig, ThemeConfig themeConfig, Dictionary<string, List<RuntimeModel>> articles, int articleIndex, string? articleCommonName, string layout, string? language, string title, DateTimeOffset time, TableOfContentsItem[]? tableOfContents, string? content, string? partialContent, JToken frontMatter) {
            _noLanguagePath = noLanguagePath;
            SiteConfig = siteConfig;
            ThemeConfig = themeConfig;
            Articles = articles;
            ArticleIndex = articleIndex;
            ArticleCommonName = articleCommonName;
            Layout = layout;
            Language = language;
            Title = title;
            Time = time;
            TableOfContents = tableOfContents;
            Content = content;
            PartialContent = partialContent;
            FrontMatter = frontMatter;
        }
    }
}
