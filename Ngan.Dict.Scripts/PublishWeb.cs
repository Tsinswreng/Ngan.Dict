using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Publishes the Ngan.Dict browser application.
internal static partial class PublishWeb{
	/// Publishes the browser project in Release configuration.
	internal static partial Task Main(string Root, CancellationToken Ct);
}
