using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Snowbow {
	class Program {
		static async Task Main(string[] args) {
			CancellationToken cancellationToken = new CancellationToken();
			Argument.MakeEffect(args);
			if (Argument.Verb == "build") {
				await Subprocess.BuildAsync(cancellationToken);
			}
			else if (Argument.Verb == "server") {
				await Subprocess.ServerAsync(cancellationToken);
			}
		}
	}
}
