namespace Ngaq.Core.Shared.Audio;

using Ngaq.Core.Infra.Errors;
using Tsinswreng.CsErr;

/// 音頻模塊統一錯誤工廠。
/// 用於把「播放階段失敗」統一映射為 KeysErr.Common.AudioPlayFailed。
public static class ToolAudioErr{
	/// 創建播放失敗異常。
	/// <param name="Inner">底層異常，可空。</param>
	/// <param name="DebugArgs">調試參數，不面向終端用戶。</param>
	/// <returns>帶有統一錯誤鍵的 AppErr。</returns>
	public static AppErr MkAudioPlayFailedErr(Exception? Inner = null, params obj?[] DebugArgs){
		var Err = KeysErr.Audio.AudioPlayFailed.ToErr();
		if(Inner is not null){
			Err.AddErr(Inner);
		}
		if(DebugArgs.Length > 0){
			Err.AddDebugArgs(DebugArgs);
		}
		return Err;
	}
}
