namespace Ngan.Dict.Scripts;

using System.Runtime.CompilerServices;

/// Ngan.Dict 維護腳本的命令列入口。
/// 第一個引數選擇具體腳本；其餘引數保留給該腳本未來的專用選項。
internal static partial class Program{
	/// 將命令列入口分派至各個具名的 Ngan.Dict 發布或資源同步工作。
	internal static async Task Main(string[] Args) {
		using var CtSource = new CancellationTokenSource();
		Console.CancelKeyPress += (_, Event) => {
			Event.Cancel = true;
			CtSource.Cancel();
		};
		if (Args.Length == 0) {
			PrintUsage();
			return;
		}

		var Ct = CtSource.Token;
		var CallerPath = OwnPath();
		var CallerDir = Path.GetDirectoryName(CallerPath)!;
		// Program.cs is at <root>/Ngan.Dict/Ngan.Dict.Scripts; walk up from its caller path, never from cwd.
		var Root = Path.GetFullPath(CallerDir / "../..").Replace('\\', '/');
		switch (Args[0]) {
			case nameof(PublishWeb):
				await PublishWeb.Main(Root, Ct);
				break;
			case nameof(PublishWin):
				await PublishWin.Main(Root, Ct);
				break;
			case nameof(PublishLinux):
				await PublishLinux.Main(Root, Ct);
				break;
			case nameof(PublishAndroid):
				await PublishAndroid.Main(Root, Ct);
				break;
			case nameof(PublishAndroidNativeAot):
				await PublishAndroidNativeAot.Main(Root, Ct);
				break;
			case nameof(CpAssets):
				await CpAssets.Main(Root, Ct);
				break;
			default:
				throw new ArgumentException($"Unknown Ngan.Dict script: {Args[0]}.", nameof(Args));
		}
	}

	/// 列出可由 dotnet run -- <入口> 呼叫的腳本名稱。
	private static void PrintUsage() {
		Console.Error.WriteLine("Usage: dotnet run --project Ngan.Dict/Ngan.Dict.Scripts -- <entry>");
		Console.Error.WriteLine("Entries: PublishWeb, PublishWin, PublishLinux, PublishAndroid, PublishAndroidNativeAot, CpAssets");
	}

	/// 讓編譯器把這個 dispatcher 原始檔路徑填入；CLR 的 Main 本身不會填 CallerFilePath。
	private static string OwnPath([CallerFilePath] string CallerPath = "") {
		return CallerPath;
	}
}
