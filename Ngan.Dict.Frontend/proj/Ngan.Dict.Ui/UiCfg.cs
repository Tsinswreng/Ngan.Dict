using Avalonia;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Ngan.Dict.Core.Infra.Cfg;
using Tsinswreng.CsCfg;

namespace Ngan.Dict.Ui;


public partial class UiCfg{
	protected static UiCfg? _Inst = null;
	public static UiCfg Inst => _Inst??= new UiCfg();

	public UiCfg(){
		BaseFontSize = KeysClientCfg.Ui.BaseFontSize.GetFrom(AppCfg.Inst);
		MainColor = ResolveThemeBrush();
	}


	public f64 BaseFontSize { get; set; } = 16.0;
	public f64 WindowWidth {get;set;}= 400;
	public f64 WindowHeight {get;set;}= 700;
	public IBrush? MainColor {get;set;}= DfltMainColor;
	public IBrush? DelBtnBg {get;set;} = new SolidColorBrush(Color.FromRgb(210, 56, 56));
	public IBrush ForegroundColor {get;set;}= Brushes.White;
	public IBrush BackgroundColor {get;set;}= Brushes.Black;
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
