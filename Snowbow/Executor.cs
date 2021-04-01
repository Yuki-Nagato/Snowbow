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
		public static async Task<(int exitCode, string stdout, string stderr)> ExecAsync(string fileName, string arguments, string? stdin, CancellationToken cancellationToken) {
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
			//if (stdout != null) {
			//	p.OutputDataReceived += (sender, args) => {
			//		if (string.IsNullOrEmpty(args.Data)) return;
			//		stdout.Write(args.Data);
			//		stdout.Flush();
			//	};
			//}
			//if(stderr != null) {
			//	p.ErrorDataReceived += (sender, args) => {
			//		if (string.IsNullOrEmpty(args.Data)) return;
			//		stderr.Write(args.Data);
			//		stderr.Flush();
			//	};
			//}
			p.Start();
			//p.BeginOutputReadLine();
			//p.BeginErrorReadLine();
			p.StandardInput.Write(stdin);
			p.StandardInput.Close();
			var stdoutTask = p.StandardOutput.ReadToEndAsync();
			var stderrTask = p.StandardError.ReadToEndAsync();
			var output = await Task.WhenAll(stdoutTask, stderrTask);
			await p.WaitForExitAsync(cancellationToken);
			return (p.ExitCode, output[0], output[1]);
		}
	}
}
