namespace Ngaq.Ui.Converters;

using Avalonia.Data;
using Ngaq.Core.Infra;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsTempus;

public class ConvtrTempus{
	protected static ConvtrTempus? _Inst = null;
	public static ConvtrTempus Inst => _Inst??= new ConvtrTempus();

	public FnConvtr<obj?, obj?> Int64{get;protected set;}
	public FnConvtr<obj?, obj?> Iso{get;protected set;}
	ConvtrTempus(){
		Int64 = new FnConvtr<obj?, obj?>(
			(tempusO)=>{
				if(tempusO is UnixMs t){
					return t.Value;
				}
				return BindingNotification.UnsetValue;
			}
			,(i64O)=>{
				try{
					var I64 = Convert.ToInt64(i64O);
					return new UnixMs(I64);
				}catch{}
				return BindingNotification.UnsetValue;
			}
		);
		Iso = new FnConvtr<obj?, obj?>(
			(tempus)=>{
				if(tempus is UnixMs t){
					return t.ToIso();
				}
				return BindingNotification.UnsetValue;
			}
			,(iso)=>{
				if(iso is str strIso && UnixMs.TryFromIso(strIso, out var R)){
					return R;
				}
				// 轉不過 → 告訴綁定引擎「維持現狀」
				return BindingNotification.UnsetValue;   // 或 UndefinedValue
			}
		);
	}
}
