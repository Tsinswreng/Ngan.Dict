namespace Ngan.Dict.Scripts;

/// 用 Roslyn 分析 Ngan.Dict.Core 專案、收集標記接口之實現者、
/// 生成 AppJsonCtx / CoreDictMapper 的註冊代碼（單文件）。
/// 替代舊的 GenAppJsonCtx.sh + Tsinswreng.CsIfaceGen.Cli 流程。
internal static partial class GenAppJsonCtx{
	/// 執行生成：載入專案 → 收集實現者 → 組裝單文件 → 寫入 Tsinswreng.SrcGen/。
	internal static partial Task Main(string Root, CancellationToken Ct);
}
