using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using RazorLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Vlingo.UUID;
using YamlDotNet.Serialization;
using System.ServiceModel.Syndication;
using System.Net;

namespace Snowbow {
	public class Subprocess {
		public static async Task<string> PandocRenderAsync(string markdown, CancellationToken cancellationToken) {
			var stdout = new StringWriter();
			var stderr = new StringWriter();
			var code = await Executor.ExecAsync("pandoc", "--no-highlight --mathml --eol=lf", new StringReader(markdown), stdout, stderr, cancellationToken);
			if (!string.IsNullOrEmpty(stderr.ToString())) {
				Logger.Log("Pandoc error, {0}", stderr.ToString());
			}
			return stdout.ToString();
		}


		public static JToken ParseFrontMatter(string markdownWithFrontMatter) {
			var posEnd = markdownWithFrontMatter.IndexOf("---", 1);
			var frontMatter = markdownWithFrontMatter.Substring(0, posEnd);
			var yamlObject = new Deserializer().Deserialize<object>(frontMatter);
			var jToken = JToken.FromObject(yamlObject);
			return jToken;
		}

		public static string ExtractPartialHtml(string contentHtml) {
			var contentHtmlDocument = new HtmlDocument();
			contentHtmlDocument.LoadHtml(contentHtml);
			HtmlNode endNode = contentHtmlDocument.DocumentNode.SelectSingleNode("//comment()[contains(., 'more')]");
			if (endNode == null) return contentHtml;
			int endPos = endNode.StreamPosition;
			string result = contentHtml.Substring(0, endPos);
			return result;
		}
		public static TableOfContentsItem[] GenerateTableOfContents(string contentHtml) {
			var ps = new SortedDictionary<int, TableOfContentsItem>();
			var document = new HtmlDocument();
			document.LoadHtml(contentHtml);

			foreach (var node in document.DocumentNode.SelectNodes("/h1").OrEmptyIfNull()) {
				ps.Add(node.StreamPosition, new TableOfContentsItem(node.Name, node.InnerHtml, node.Attributes["id"].Value));
			}
			foreach (var node in document.DocumentNode.SelectNodes("/h2").OrEmptyIfNull()) {
				ps.Add(node.StreamPosition, new TableOfContentsItem(node.Name, node.InnerHtml, node.Attributes["id"].Value));
			}
			foreach (var node in document.DocumentNode.SelectNodes("/h3").OrEmptyIfNull()) {
				ps.Add(node.StreamPosition, new TableOfContentsItem(node.Name, node.InnerHtml, node.Attributes["id"].Value));
			}
			foreach (var node in document.DocumentNode.SelectNodes("/h4").OrEmptyIfNull()) {
				ps.Add(node.StreamPosition, new TableOfContentsItem(node.Name, node.InnerHtml, node.Attributes["id"].Value));
			}
			foreach (var node in document.DocumentNode.SelectNodes("/h5").OrEmptyIfNull()) {
				ps.Add(node.StreamPosition, new TableOfContentsItem(node.Name, node.InnerHtml, node.Attributes["id"].Value));
			}
			foreach (var node in document.DocumentNode.SelectNodes("/h6").OrEmptyIfNull()) {
				ps.Add(node.StreamPosition, new TableOfContentsItem(node.Name, node.InnerHtml, node.Attributes["id"].Value));
			}
			return ps.Values.ToArray();
		}

		public static string RedirectArticleAssetPath(SiteConfig siteConfig, string articleCommonName, string html) {
			var document = new HtmlDocument();
			document.LoadHtml(html);
			var nodes = document.DocumentNode.SelectNodes("//*[starts-with(@src, 'assets/')]");
			if (nodes != null) {
				foreach (var node in nodes) {
					var src = node.Attributes["src"].Value;
					node.Attributes["src"].Value = siteConfig.BaseUrl.LocalPath + "article-assets/" + articleCommonName + src[6..];
				}
			}

			nodes = document.DocumentNode.SelectNodes("//*[starts-with(@href, 'assets/')]");
			if (nodes != null) {
				foreach (var node in nodes) {
					var href = node.Attributes["href"].Value;
					node.Attributes["href"].Value = siteConfig.BaseUrl.LocalPath + "article-assets/" + articleCommonName + href[6..];
				}
			}

			return document.DocumentNode.OuterHtml;
		}

		static async Task<Dictionary<string, List<RuntimeModel>>> GenerateArticlesAsync(SiteConfig siteConfig, ThemeConfig themeConfig, CancellationToken cancellationToken) {
			var articles = new Dictionary<string, List<RuntimeModel>>();
			foreach (string language in siteConfig.Language.Keys) {
				articles.Add(language, new List<RuntimeModel>());
			}

			DirectoryInfo articlesDir = new DirectoryInfo("articles/");
			DirectoryInfo[] articleDirs = articlesDir.GetDirectories();
			foreach (DirectoryInfo articleDir in articleDirs) {
				Match m = Regex.Match(articleDir.Name, @"^(\d{8})-(.+)$");
				if (!m.Success) continue;
				string articleCommonName = m.Groups[2].Value;
				FileInfo[] articleMarkdownFiles = articleDir.GetFiles("*.*.md");
				// add default language if specified language not exist
				RuntimeModel defaultLanguage = null!;
				var existedLanguage = new SortedSet<string>();
				foreach (FileInfo articleMarkdownFile in articleMarkdownFiles) {
					string[] temp = articleMarkdownFile.Name.Split('.');
					string language = temp[temp.Length - 2];
					if (!siteConfig.Language.ContainsKey(language)) {
						continue;
					}
					string markdown = await articleMarkdownFile.ReadAllTextAsync(cancellationToken);
					var frontMatter = ParseFrontMatter(markdown);
					string contentHtml = await PandocRenderAsync(markdown, cancellationToken);
					contentHtml = RedirectArticleAssetPath(siteConfig, articleCommonName, contentHtml);
					string partialHtml = ExtractPartialHtml(contentHtml);
					TableOfContentsItem[] tableOfContents = GenerateTableOfContents(contentHtml);
					RuntimeModel article = new RuntimeModel("/articles/" + articleCommonName + "/", siteConfig, themeConfig, articles, -1, articleCommonName, "article", language, frontMatter.Value<string>("title"), DateTimeOffset.Parse(frontMatter.Value<string>("time")), tableOfContents, contentHtml, partialHtml, frontMatter);
					articles[language].Add(article);
					if (language == siteConfig.DefaultLanguage) {
						defaultLanguage = article;
					}
					existedLanguage.Add(language);
				}
				if (defaultLanguage == null) {
					throw new Exception($"article {articleCommonName} does not contain default language");
				}

				foreach (string language in siteConfig.Language.Keys) {
					if (!existedLanguage.Contains(language)) {
						JToken newFrontMatter = defaultLanguage.FrontMatter.DeepClone();
						newFrontMatter["contentLanguage"] = siteConfig.DefaultLanguage;

						RuntimeModel article = new RuntimeModel("/articles/" + articleCommonName + "/", siteConfig, themeConfig, articles, -1, articleCommonName, "article", language, defaultLanguage.Title, defaultLanguage.Time, defaultLanguage.TableOfContents, defaultLanguage.Content, defaultLanguage.PartialContent, newFrontMatter);
						articles[language].Add(article);
					}
				}

			}
			foreach (var kvp in articles) {
				kvp.Value.Sort((RuntimeModel a, RuntimeModel b) => {
					if (a.Time > b.Time) return -1;
					else if (a.Time < b.Time) return 1;
					else return 0;
				});
				for (int i = 0; i < kvp.Value.Count; i++) {
					kvp.Value[i].ArticleIndex = i;
				}
			}
			return articles;
		}

		static async Task<List<RuntimeModel>> GeneratePagesAsync(SiteConfig siteConfig, ThemeConfig themeConfig, Dictionary<string, List<RuntimeModel>> articles, CancellationToken cancellationToken) {
			List<RuntimeModel> pages = new List<RuntimeModel>();
			// Generate explicit pages.
			foreach (FileInfo file in new DirectoryInfo("pages/").GetFiles("*.md", SearchOption.AllDirectories)) {
				string? specialLanguage = null;
				foreach (string language in siteConfig.Language.Keys) {
					DirectoryInfo languageDirectory = new DirectoryInfo("pages/" + language + "/");
					if (file.IsUnderDirectory(languageDirectory)) {
						specialLanguage = language;
						break;
					}
				}
				if (specialLanguage != null) {
					string noLanguagePath = "/" + Helper.RelativePath(new DirectoryInfo("pages/" + specialLanguage + "/"), file);
					if (noLanguagePath.EndsWith("/index.md", StringComparison.Ordinal)) {
						noLanguagePath = noLanguagePath[0..^8];
					}
					else {
						// ens with xxx.md
						noLanguagePath = noLanguagePath[0..^3] + ".html";
					}
					string markdown = await file.ReadAllTextAsync(cancellationToken);
					var frontMatter = ParseFrontMatter(markdown);
					string contentHtml = await PandocRenderAsync(markdown, cancellationToken);
					string partialHtml = ExtractPartialHtml(contentHtml);
					TableOfContentsItem[] tableOfContents = GenerateTableOfContents(contentHtml);
					RuntimeModel page = new RuntimeModel(noLanguagePath, siteConfig, themeConfig, articles, -1, null, frontMatter.Value<string>("layout"), specialLanguage, frontMatter.Value<string>("title"), ((JObject)frontMatter).ValueOrDefault("time", DateTimeOffset.Now), tableOfContents, contentHtml, partialHtml, frontMatter);
					pages.Add(page);
				}
				else {
					string noLanguagePath = "/" + Helper.RelativePath(new DirectoryInfo("pages/"), file);
					if (noLanguagePath.EndsWith("/index.md", StringComparison.Ordinal)) {
						noLanguagePath = noLanguagePath[0..^8];
					}
					else {
						// ens with xxx.md
						noLanguagePath = noLanguagePath[0..^3] + ".html";
					}
					string markdown = await file.ReadAllTextAsync(cancellationToken);
					var frontMatter = ParseFrontMatter(markdown);
					string contentHtml = await PandocRenderAsync(markdown, cancellationToken);
					string partialHtml = ExtractPartialHtml(contentHtml);
					TableOfContentsItem[] tableOfContents = GenerateTableOfContents(contentHtml);
					RuntimeModel page = new RuntimeModel(noLanguagePath, siteConfig, themeConfig, articles, -1, null, frontMatter.Value<string>("layout"), specialLanguage, frontMatter.Value<string>("title"), ((JObject)frontMatter).ValueOrDefault("time", DateTimeOffset.Now), tableOfContents, contentHtml, partialHtml, frontMatter);
					pages.Add(page);
				}
			}

			// Generate implicit pages.
			foreach (string language in siteConfig.Language.Keys) {
				// index
				RuntimeModel index = new RuntimeModel("/", siteConfig, themeConfig, articles, -1, null, "index", language, themeConfig.Translation[language]["index"], DateTimeOffset.Now, null, null, null, new JObject());
				pages.Add(index);

				// categories
				var categoriesSet = new SortedSet<string>();
				foreach (RuntimeModel article in articles[language]) {
					if (((JObject)article.FrontMatter).TryGetValue("categories", out var categories)) {
						foreach (var category in (JArray)categories) {
							categoriesSet.Add(category.ToObject<string>()!);
						}
					}
				}
				RuntimeModel categoryIndex = new RuntimeModel("/categories/", siteConfig, themeConfig, articles, -1, null, "categories", language, themeConfig.Translation[language]["categories"], DateTimeOffset.Now, null, null, null, new JObject(new JProperty("categories", categoriesSet)));
				pages.Add(categoryIndex);
				foreach (string category in categoriesSet) {
					RuntimeModel categoryPage = new RuntimeModel("/categories/" + category + "/", siteConfig, themeConfig, articles, -1, null, "category", language, themeConfig.Translation[language]["category"] + ": " + category, DateTimeOffset.Now, null, null, null, new JObject(new JProperty("category", category)));
					pages.Add(categoryPage);
				}

				// tags
				var tagsSet = new SortedSet<string>();
				foreach (RuntimeModel article in articles[language]) {
					if (((JObject)article.FrontMatter).TryGetValue("tags", out var tags)) {
						foreach (var tag in (JArray)tags) {
							tagsSet.Add(tag.ToObject<string>()!);
						}
					}
				}
				RuntimeModel tagIndex = new RuntimeModel("/tags/", siteConfig, themeConfig, articles, -1, null, "tags", language, themeConfig.Translation[language]["tags"], DateTimeOffset.Now, null, null, null, new JObject(new JProperty("tags", tagsSet)));
				pages.Add(tagIndex);
				foreach (string tag in tagsSet) {
					RuntimeModel tagPage = new RuntimeModel("/tags/" + tag + "/", siteConfig, themeConfig, articles, -1, null, "tag", language, themeConfig.Translation[language]["tag"] + ": " + tag, DateTimeOffset.MinValue, null, null, null, new JObject(new JProperty("tag", tag)));
					pages.Add(tagPage);
				}
			}
			return pages;
		}

		static async Task<MemoryFileSystem> RazorRenderAsync(SiteConfig siteConfig, Dictionary<string, List<RuntimeModel>> articles, List<RuntimeModel> pages, CancellationToken cancellationToken) {
			RazorLightEngine engine = new RazorLightEngineBuilder().AddDefaultNamespaces("Snowbow").UseFileSystemProject(new DirectoryInfo("themes/" + siteConfig.Theme).FullName).UseMemoryCachingProvider().Build();
			MemoryFileSystem mfs = new MemoryFileSystem();
			foreach (List<RuntimeModel> languagedArticles in articles.Values) {
				foreach (RuntimeModel article in languagedArticles) {
					var rendered = await engine.CompileRenderAsync("index.cshtml", article);
					var stdout = new StringWriter();
					var stderr = new StringWriter();
					var code = await Executor.ExecAsync("tidy", "-utf8 --wrap 0 --tidy-mark no --drop-empty-elements no --quiet yes -indent --newline LF --output-bom no", new StringReader(rendered), stdout, stderr, cancellationToken);
					//if (!string.IsNullOrEmpty(stderr.ToString())) {
					//	Logger.Log("Tidy error, {0}", stderr.ToString());
					//}
					mfs.CreateFile(article.AbsolutePath, stdout.ToString());
				}
			}
			foreach (RuntimeModel page in pages) {
				var rendered = await engine.CompileRenderAsync("index.cshtml", page);
				var stdout = new StringWriter();
				var stderr = new StringWriter();
				var code = await Executor.ExecAsync("tidy", "-utf8 --wrap 0 --tidy-mark no --drop-empty-elements no --quiet yes -indent --newline LF --output-bom no", new StringReader(rendered), stdout, stderr, cancellationToken);
				//if (!string.IsNullOrEmpty(stderr.ToString())) {
				//	Logger.Log("Tidy error, {0}", stderr.ToString());
				//}
				mfs.CreateFile(page.AbsolutePath, stdout.ToString());
			}
			return mfs;
		}

		static async Task<MemoryFileSystem> CopyAssetsAsync(SiteConfig siteConfig, CancellationToken cancellationToken) {
			MemoryFileSystem mfs = new MemoryFileSystem();
			// articles/xxx/assets/
			DirectoryInfo articlesDir = new DirectoryInfo("articles/");
			DirectoryInfo[] articleDirs = articlesDir.GetDirectories();
			foreach (DirectoryInfo articleDir in articleDirs) {
				Match m = Regex.Match(articleDir.Name, @"^(\d{8})-(.+)$");
				if (!m.Success) continue;
				string articleCommonName = m.Groups[2].Value;
				DirectoryInfo[] assetsDirectories = articleDir.GetDirectories("assets");
				if (assetsDirectories.Length != 1) {
					continue;
				}
				DirectoryInfo assetsDirectory = assetsDirectories[0];
				FileInfo[] assets = assetsDirectory.GetFiles("*", SearchOption.AllDirectories);
				foreach (FileInfo asset in assets) {
					string path = siteConfig.BaseUrl.LocalPath + "article-assets/" + articleCommonName + "/" + Helper.RelativePath(assetsDirectory, asset);
					mfs.CreateFile(path, await asset.ReadAllBytesAsync(cancellationToken));
				}
			}
			// themes/xxx/assets/
			DirectoryInfo themeAssetsDirectory = new DirectoryInfo("themes/" + siteConfig.Theme + "/assets");
			FileInfo[] themeAssets = themeAssetsDirectory.GetFiles("*", SearchOption.AllDirectories);
			foreach (FileInfo asset in themeAssets) {
				string path = "/" + Helper.RelativePath(themeAssetsDirectory, asset);
				mfs.CreateFile(path, await asset.ReadAllBytesAsync(cancellationToken));
			}
			return mfs;
		}

		public static MemoryFileSystem GenerateAtomSyndication(SiteConfig siteConfig, Dictionary<string, List<RuntimeModel>> articles) {
			MemoryFileSystem result = new MemoryFileSystem();
			using NameBasedGenerator uuidGenerator = new NameBasedGenerator(HashType.Sha1);
			foreach (var kvp in articles) {
				string language = kvp.Key;
				SiteLanguageConfig config = siteConfig.Language[language];
				SyndicationFeed feed = new SyndicationFeed(config.Title, config.Description, new Uri(siteConfig.BaseUrl.OriginalString + language + "/"));
				feed.Authors.Add(new SyndicationPerson("yuki@yuki-nagato.com", "長門有希", siteConfig.BaseUrl.OriginalString + language + "/"));
				feed.LastUpdatedTime = DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0));
				feed.Id = uuidGenerator.GenerateGuid(UUIDNameSpace.Url, siteConfig.BaseUrl.OriginalString + language + "/").ToString();
				List<SyndicationItem> items = new List<SyndicationItem>();
				foreach (var article in kvp.Value) {
					SyndicationItem entry = new SyndicationItem();
					entry.Title = SyndicationContent.CreatePlaintextContent(article.Title);
					entry.PublishDate = article.Time;
					entry.LastUpdatedTime = article.Time;
					entry.Content = SyndicationContent.CreateHtmlContent(article.Content);
					entry.Links.Add(SyndicationLink.CreateAlternateLink(new Uri(article.Permalink)));
					entry.Id = uuidGenerator.GenerateGuid(UUIDNameSpace.Url, article.Permalink).ToString();
					items.Add(entry);
				}
				feed.Items = items;
				StringWriter sw = new StringWriter();
				XmlTextWriter xmlWriter = new XmlTextWriter(sw);
				feed.SaveAsAtom10(xmlWriter);
				xmlWriter.Close();
				sw.Close();
				result.CreateFile(siteConfig.BaseUrl.LocalPath + language + "/atom.xml", sw.ToString());
			}
			return result;
		}

		public static MemoryFileSystem GenerateMeta(SiteConfig siteConfig, Dictionary<string, List<RuntimeModel>> articles) {
			MemoryFileSystem result = GenerateAtomSyndication(siteConfig, articles);
			result.CreateFile(siteConfig.BaseUrl.LocalPath, @$"<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <meta http-equiv=""refresh"" content=""0; url={siteConfig.BaseUrl.LocalPath}{siteConfig.DefaultLanguage}/"" />
  </head>
  <body></body>
</html>");
			return result;
		}

		public static async Task<MemoryFileSystem> GenerateAllAsync(SiteConfig siteConfig, CancellationToken cancellationToken) {
			Logger.Log("Building...");
			ThemeConfig themeConfig = ThemeConfig.Read(siteConfig);
			var articles = await GenerateArticlesAsync(siteConfig, themeConfig, cancellationToken);
			var pages = await GeneratePagesAsync(siteConfig, themeConfig, articles, cancellationToken);
			MemoryFileSystem mfs = await RazorRenderAsync(siteConfig, articles, pages, cancellationToken);
			mfs.Merge(await CopyAssetsAsync(siteConfig, cancellationToken), Helper.MergeKeyExistedPolicy.Exception);
			mfs.Merge(GenerateMeta(siteConfig, articles), Helper.MergeKeyExistedPolicy.Exception);
			Logger.Log("Built successfully.");
			return mfs;
		}
		public static async Task BuildAsync(CancellationToken cancellationToken) {
			var siteConfig = SiteConfig.Read();
			var mfs = await GenerateAllAsync(siteConfig, cancellationToken);
			var publishDirectory = new DirectoryInfo("public");
			if (publishDirectory.Exists) {
				publishDirectory.Delete(true);
			}
			await mfs.WriteToDiskAsync(publishDirectory, 0, cancellationToken);
		}

		public static async Task ServerAsync(CancellationToken cancellationToken) {
			MemoryFileSystem mfs = new MemoryFileSystem();
			HttpListener listener = new HttpListener();
			SiteConfig siteConfig = SiteConfig.Read();
			listener.Prefixes.Add("http://127.0.0.1:4000" + siteConfig.BaseUrl.LocalPath);
			listener.Start();
			Task.Run(() => {
				while (true) {
					var context = listener.GetContext();
					var req = context.Request;
					var resp = context.Response;
					var path = req.Url.LocalPath;
					byte[] respContent;
					bool existed = mfs.TryGetValue(path.EndsWith('/') ? path + "index.html" : path, out respContent);
					if (!existed) {
						resp.StatusCode = (int)HttpStatusCode.NotFound;
						resp.ContentType = "text/plain; charset=utf-8";
						resp.Close("404 not found".ToUtf8Bytes(), true);
						continue;
					}
					if (path.EndsWith(".css")) {
						resp.ContentType = "text/css";
					}
					resp.Close(respContent, true);
				}
			});
			mfs = await GenerateAllAsync(siteConfig, cancellationToken);
			var watcher = new FileSystemWatcher(Environment.CurrentDirectory);
			//watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.CreationTime | NotifyFilters.Security;
			watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
			watcher.Filter = "*.md";
			watcher.IncludeSubdirectories = true;

			CancellationTokenSource? watcherBuildingCancellationTokenSource = null;
			watcher.Changed += async (sender, args) => {
				watcherBuildingCancellationTokenSource?.Cancel();
				watcherBuildingCancellationTokenSource = new CancellationTokenSource();
				Logger.Log("Debounce...");
				try {
					await Task.Delay(TimeSpan.FromSeconds(2), watcherBuildingCancellationTokenSource.Token);
				}
				catch (TaskCanceledException e) {
					Logger.Log(e.ToString());
					return;
				}
				try {
					mfs = await GenerateAllAsync(siteConfig, watcherBuildingCancellationTokenSource.Token);
				}
				catch (Exception e) {
					Logger.Log(e.ToString());
					return;
				}
			};
			watcher.EnableRaisingEvents = true;
			Logger.Log("Watching " + watcher.Path);
			Console.ReadLine();
		}
	}
}
