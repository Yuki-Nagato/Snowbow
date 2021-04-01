using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Snowbow {
	public class Logger {
        public static void Log(string format, params object?[]? arg) {
            var stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);
            var fileName = stackFrame?.GetFileName();
            fileName = Path.GetFileName(fileName);
            var lineNumber = stackFrame?.GetFileLineNumber();
            Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zz}] [{fileName}:{lineNumber}] " + format, arg);
        }
    }
}
