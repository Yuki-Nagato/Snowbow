using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snowbow {
    public class MemoryFileSystem : SortedDictionary<string, byte[]> {

        public void CreateFile(string path, byte[] content) {
            if (path == null) {
                throw new ArgumentNullException(nameof(path));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (!path.StartsWith('/')) {
                throw new ArgumentException("file path invalid", nameof(path));
            }
            if (path.EndsWith('/')) {
                path += "index.html";
            }
            foreach (string p in Keys) {
                if (p == path || p.StartsWith(path + "/") || path.StartsWith(p + "/")) {
                    throw new ArgumentException("file already exists: " + p);
                }
            }
            Add(path, content);
        }

        public void CreateFile(string path, string content) {
            CreateFile(path, content.ToUtf8Bytes());
        }

        public string ReadFileAsString(string path) {
            return this[path].ToUtf8String();
        }

        public async Task WriteToDiskAsync(DirectoryInfo root, int ignorePrefix, CancellationToken cancellationToken) {
            string rootPath = root.FullName.Replace('\\', '/').TrimEnd('/');
            foreach (var kvp in this) {
                string path = "";
                var splittedPaths = kvp.Key.Split('/').Skip(ignorePrefix + 1);
                foreach(var splittedPath in splittedPaths) {
                    path += "/" + splittedPath;
				}
                var toWrite = new FileInfo(rootPath + path);
                toWrite.Directory!.Create();
                await toWrite.WriteAllBytesAsync(kvp.Value, cancellationToken);
            }
        }
    }
}
