namespace Ngaq.Ui.Views.Settings;

using Avalonia.Controls;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Settings.Lang;
using Tsinswreng.AvlnTools.Dsl;
using Ctx = Ngaq.Ui.Infra.ViewModelBase;
using Tsinswreng.AvlnTools.Tools;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.Icons;

public partial class ViewCfgUi
	:AppViewBase<Ctx>
{

	public ViewCfgUi(){
		Ctx = new Ctx();
		Style();
		Render();
	}

	public  partial class Cls{}


	protected nil Style(){
		return NIL;
	}

	protected nil Render(){
		var _Item = ViewSettings.FnSettingItem(ViewNavi);
		this.SetContent(new StackPanel(), S=>{
			S.A(_Item(I[KeysUiI18nCommon.FontSize], new ViewCfgFont(), Icons.FontOutline()));
			S.A(_Item(I[KeysUiI18nCommon.Language], new ViewCfgLang(), Icons.EarthAmericasSolid()));
		});
		return NIL;
	}


}

