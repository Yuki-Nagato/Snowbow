using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YukiToolkit.UsefulConsts;

namespace Snowbow {
	public static class Helper {
		public static readonly JsonSerializerSettings MyJsonSerializerSettings = new JsonSerializerSettings() {
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		public enum MergeKeyExistedPolicy {
			Ignore, Update, Exception
		}
		public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> to, IDictionary<TKey, TValue> from, MergeKeyExistedPolicy keyExistedPolicy) {
			if (to == null) {
				throw new ArgumentNullException(nameof(to));
			}
			if (from == null) {
				throw new ArgumentNullException(nameof(from));
			}
			foreach (var it in from) {
				if (to.ContainsKey(it.Key)) {
					switch (keyExistedPolicy) {
						case MergeKeyExistedPolicy.Ignore: break;
						case MergeKeyExistedPolicy.Update: to[it.Key] = it.Value; break;
						case MergeKeyExistedPolicy.Exception: throw new Exception($"Key existed: {it.Key}");
					}
				}
				else {
					to.Add(it);
				}
			}
		}
		public static bool IsUnderDirectory(this FileInfo file, DirectoryInfo directory) {
			string filePath = file.FullName.Replace('\\', '/');
			string dirPath = directory.FullName.Replace('\\', '/');
			if (!dirPath.EndsWith('/')) {
				dirPath += "/";
			}
			return filePath.StartsWith(dirPath, StringComparison.Ordinal);
		}

		public static string RelativePath(string source, string target) {
			string[] sourcePath = source.Split('/', '\\');
			string[] targetPath = target.Split('/', '\\');
			int sameLen;
			for (sameLen = 0; sameLen < sourcePath.Length - 1 && sameLen < targetPath.Length - 1; sameLen++) {
				if (sourcePath[sameLen] != targetPath[sameLen]) {
					break;
				}
			}
			string result = "";
			for (int i = 0; i < sourcePath.Length - sameLen - 1; i++) {
				result += "../";
			}
			for (int i = sameLen; i < targetPath.Length - 1; i++) {
				result += targetPath[i] + "/";
			}
			return result + targetPath[^1];
		}

		public static string RelativePath(DirectoryInfo source, FileInfo target) {
			return RelativePath(source.FullName.TrimEnd('/', '\\') + "/", target.FullName);
		}

		public static async Task<string> ReadAllTextAsync(this FileInfo file, CancellationToken cancellationToken) {
			return await File.ReadAllTextAsync(file.FullName, ConstStuff.UniversalUtf8Encoding, cancellationToken);
		}
		public static async Task<byte[]> ReadAllBytesAsync(this FileInfo file, CancellationToken cancellationToken) {
			return await File.ReadAllBytesAsync(file.FullName, cancellationToken);
		}
		public static async Task WriteAllBytesAsync(this FileInfo file, byte[] bytes, CancellationToken cancellationToken) {
			await File.WriteAllBytesAsync(file.FullName, bytes, cancellationToken);
		}

		public static byte[] ToUtf8Bytes(this string str) {
			return ConstStuff.UniversalUtf8Encoding.GetBytes(str);
		}

		public static string ToUtf8String(this byte[] bytes) {
			return ConstStuff.UniversalUtf8Encoding.GetString(bytes);
		}

		public static T ValueOrDefault<T>(this JObject jObject, string propertyName, T @default) {
			if (jObject.TryGetValue(propertyName, out var result)) {
				return result.ToObject<T>()!;
			}
			else {
				return @default;
			}
		}

		public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source) {
			return source ?? Enumerable.Empty<T>();
		}

		public static T GetOrDefault<T>(this JToken json, string propertyName, T @default) {
			JObject jObject = (JObject)json;
			if (jObject.ContainsKey(propertyName)) {
				return jObject[propertyName]!.ToObject<T>()!;
			}
			else {
				return @default;
			}
		}
	}
}
