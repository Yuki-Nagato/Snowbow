using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukiToolkit.UsefulConsts;

namespace Snowbow {
	class Executor {
		public static async Task<int> ExecAsync(string fileName, string arguments, TextReader? stdin, TextWriter? stdout, TextWriter? stderr, CancellationToken cancellationToken) {
			using var p = new Process();
			p.StartInfo.FileName = fileName;
			p.StartInfo.Arguments = arguments;

			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.StandardInputEncoding = ConstStuff.UniversalUtf8Encoding;
			p.StartInfo.StandardOutputEncoding = ConstStuff.UniversalUtf8Encoding;
			p.StartInfo.StandardErrorEncoding = ConstStuff.UniversalUtf8Encoding;
			p.Start();

			// stdin, stdout, stderr at the same time
			var stdinTask = Task.Run(async () => {
				if(stdin != null) {
					var buf = new char[1024];
					while (true) {
						var len = await stdin.ReadAsync(buf, 0, buf.Length);
						if(len <= 0) {
							break;
						}
						await p.StandardInput.WriteAsync(buf, 0, len);
						await p.StandardInput.FlushAsync();
					}
				}
				p.StandardInput.Close();
			}, cancellationToken);
			var stdoutTask = Task.Run(async () => {
				var buf = new char[1024];
				while (true) {
					var len = await p.StandardOutput.ReadAsync(buf, 0, buf.Length);
					if (len <= 0) {
						break;
					}
					if (stdout != null) {
						await stdout.WriteAsync(buf, 0, len);
						await stdout.FlushAsync();
					}
				}
				if (stdout != null) {
					stdout.Close();
				}
			}, cancellationToken);
			var stderrTask = Task.Run(async () => {
				var buf = new char[1024];
				while (true) {
					var len = await p.StandardError.ReadAsync(buf, 0, buf.Length);
					if (len <= 0) {
						break;
					}
					if (stderr != null) {
						await stderr.WriteAsync(buf, 0, len);
						await stderr.FlushAsync();
					}
				}
				if (stderr != null) {
					stderr.Close();
				}
			}, cancellationToken);
			await Task.WhenAll(stdinTask, stdoutTask, stderrTask);
			await p.WaitForExitAsync(cancellationToken);
			return p.ExitCode;
		}
	}
}
