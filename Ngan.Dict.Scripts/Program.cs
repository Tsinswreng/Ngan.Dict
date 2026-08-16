namespace Ngan.Dict.Scripts;

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
		switch (Args[0]) {
			case nameof(PublishWeb):
				await PublishWeb.Main(Ct);
				break;
			case nameof(PublishWin):
				await PublishWin.Main(Ct);
				break;
			case nameof(PublishLinux):
				await PublishLinux.Main(Ct);
				break;
			case nameof(PublishAndroid):
				await PublishAndroid.Main(Ct);
				break;
			case nameof(PublishAndroidNativeAot):
				await PublishAndroidNativeAot.Main(Ct);
				break;
			case nameof(CpAssets):
				await CpAssets.Main(Ct);
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
}
