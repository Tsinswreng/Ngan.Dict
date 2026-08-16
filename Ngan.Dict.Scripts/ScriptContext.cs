using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// 讓全部 Ngan.Dict 腳本從任意呼叫目錄取得一致的倉庫與專案路徑。
internal static partial class ScriptContext{
	/// Ngan 工作區根目錄，必須包含 Ngan.Dict 與 ExternalRsrc。
	internal static readonly string Root = FindRoot();

	/// Ngan.Dict 專案根目錄。
	internal static readonly string Dict = Root / "Ngan.Dict";

	/// 前端 projects 的共同父目錄。
	internal static readonly string FrontendProjects = Dict / "Ngan.Dict.Frontend/proj";

	/// 將命令兩條輸出流轉送到目前終端，並讓非零退出碼依 X 的規則中止腳本。
	internal static partial Task<CommandExit> XTerm(string Text, CommandOptions? Options, CancellationToken Ct);

	/// 遞迴複製 Source 下的內容到 Destination，而非額外嵌套 Source 的最後一段名稱。
	internal static partial Task<object?> CopyContents(string Source, string Destination, CancellationToken Ct);

	/// 在當前工作目錄與腳本組件位置向上尋找 Ngan 根目錄。
	private static string FindRoot() {
		var FromCurrent = FindRootFrom(Pwd());
		if (FromCurrent is not null)
			return FromCurrent;
		var FromAssembly = FindRootFrom(AppContext.BaseDirectory);
		if (FromAssembly is not null)
			return FromAssembly;
		throw new DirectoryNotFoundException("Cannot find the Ngan workspace root containing Ngan.Dict and ExternalRsrc.");
	}

	/// 逐層查找兩個工作區識別目錄，避免把 scripts 的啟動目錄當作隱含契約。
	private static string? FindRootFrom(string Start) {
		for (var DirectoryPath = new DirectoryInfo(Start); DirectoryPath is not null; DirectoryPath = DirectoryPath.Parent) {
			var Candidate = DirectoryPath.FullName.Replace('\\', '/');
			if (Exists(Candidate / "Ngan.Dict") && Exists(Candidate / "ExternalRsrc"))
				return Candidate;
		}
		return null;
	}
}
