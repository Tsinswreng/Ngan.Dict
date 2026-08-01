using Tsinswreng.Avln.Navi;

namespace Ngaq.Ui.Infra;

public partial interface I_GetViewNavi{
	public IViewNavi? GetViewNavi();
}


public partial class MgrViewNavi:I_GetViewNavi{
	protected static MgrViewNavi? _Inst = null;
	public static MgrViewNavi Inst => _Inst??= new MgrViewNavi();

	public IViewNavi? ViewNavi{get;set;}
	public IViewNavi GetViewNavi(){
		if(ViewNavi == null){
			ViewNavi = new ViewNavi();
		}
		return ViewNavi;
	}
}


