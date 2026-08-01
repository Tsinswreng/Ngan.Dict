namespace Ngaq.Ui.Views.Word;

/// 行記錄的 DML 狀態，用於原子化保存。
public enum EDmlState{
	/// 未變動（從數據庫加載後未修改）。
	Unchanged = 0,
	/// 前端新增，保存時走 BatAdd。
	Added = 1,
	/// 已修改，保存時走 BatUpd。
	Modified = 2,
	/// 已標記刪除，保存時走 Del。
	Removed = 3,
}
