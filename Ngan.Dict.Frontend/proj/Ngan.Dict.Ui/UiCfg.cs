using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Ngan.Dict.Core.Infra.Cfg;
using Tsinswreng.CsCfg;

namespace Ngan.Dict.Ui;


public partial class UiCfg{
	/// 界面基礎字級在 Avalonia 主題資源裏的鍵名。
	/// 這是自訂的主題資源鍵（不是某個控件屬性），故集中在此聲明一次，避免各處硬編碼字符串。
	public const str KeyContentThemeFontSize = nameof(ContentControl.FontSize);

	protected static UiCfg? _Inst = null;
	public static UiCfg Inst => _Inst??= new UiCfg();

	public UiCfg(){
		BaseFontSize = AppCfg.Inst.Get(KeysClientCfg.Ui.BaseFontSize);
		MainColor = ResolveThemeBrush();
	}


	public f64 BaseFontSize { get; set; } = 16.0;
	public f64 WindowWidth {get;set;}= 400;
	public f64 WindowHeight {get;set;}= 700;
	public IBrush? MainColor {get;set;}= DfltMainColor;
	public IBrush? DelBtnBg {get;set;} = new SolidColorBrush(Color.FromRgb(210, 56, 56));
	public IBrush ForegroundColor {get;set;}= Brushes.White;
	public IBrush BackgroundColor {get;set;}= Brushes.Black;

	/// 次要文字（原文、讀音、例句譯文、備註）。
	public IBrush MutedTextBrush {get;set;} = new SolidColorBrush(Color.FromArgb(0xB3, 0xFF, 0xFF, 0xFF));

	/// 最弱一級文字（說明、時間戳、例句標記）。
	/// 三級色階（主／次／弱）比只有白與灰更容易建立層次，且不必動到主題。
	public IBrush FaintTextBrush {get;set;} = new SolidColorBrush(Color.FromArgb(0x73, 0xFF, 0xFF, 0xFF));

	/// 頁面內容的水平內距。所有頁面共用一個值，避免各頁各寫一套。
	public double PagePaddingH {get;set;} = 14;

	/// 頁面內容的垂直內距（上下各留這麼多）。
	public double PagePaddingV {get;set;} = 10;

	/// 組與組之間（例如詞頭區與義項區）的間距。
	public double GapSection {get;set;} = 14;

	/// 同一組內元素之間的間距。
	public double GapItem {get;set;} = 6;

	/// 例句相對義項正文的縮排量。
	public double IndentContent {get;set;} = 12;

	public static IBrush DfltMainColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x78, 0xD7));

	protected IBrush ResolveThemeBrush(){
		var app = Application.Current;
		if (app == null){
			return DfltMainColor;
		}

		// 嘗試主題資源鍵 (Avalonia first, then Windows)
		var keys = new[] {
			"ThemeAccentBrush",
			"ThemeAccentBrush2",
			"ThemeAccentBrush3",
			"ThemeAccentBrush4",
			"AccentBrush",
			"SystemControlHighlightAccentBrush",
			"SystemControlForegroundBaseHighBrush",
			"SystemAccentColorBrush",
			"AccentColorBrush",
		};
		foreach(var k in keys){
			if (app.Resources.TryGetValue(k, out var val) && val is IBrush b){
				return b;
			}
		}
		return DfltMainColor;
	}
}
