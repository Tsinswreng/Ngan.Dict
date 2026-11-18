namespace Ngan.Dict.Ui.Views.Settings;

using Avalonia.Controls;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Views.Settings.Lang;
using Ctx = Ngan.Dict.Ui.Infra.AppVmBase;
using Ngan.Dict.Ui.Infra.I18n;
using Ngan.Dict.Ui.Icons;

public partial class ViewCfgUi
	:LegacyAppViewBase<Ctx>
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

