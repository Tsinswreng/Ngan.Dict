using Avalonia.Controls;
using Ngaq.Ui.Tools;
using Tsinswreng.CsCore;

namespace Ngaq.Ui.Infra;

[Doc(@$"
使用 {nameof(ToolView.WithTitle)} 時、實現了 此接口的控件、在創建的頂標題欄中 右側會有菜單按鈕
")]
public interface I_MkTitleMenu{
	public Control MkTitleMenu();
}
