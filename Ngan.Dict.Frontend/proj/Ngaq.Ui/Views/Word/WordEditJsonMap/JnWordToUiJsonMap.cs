using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools.JsonMap;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTools;

namespace Ngaq.Ui.Views.Word.WordEditJsonMap;
using W = Ngaq.Core.Shared.Word.Models.Po.Word.PoWord;
public class JnWordToUiJsonMap{
	public static IUiJsonMap MkPoWord(IDictionary<str, obj?> Dict){
		var PathToUiMap = new Dictionary<str, IUiJsonMapItem>();
		var R = new UiJsonMap(){
			PathToUiMap = PathToUiMap,
			Raw = new JsonNode(Dict),
		};
		foreach(var (k,v) in Dict){
			var u = new UiJsonMapItem(R, k){
				DisplayName = UiT(k),
				Descr = UiT(""),
			};
			var oldGet = u.FnGet;

			u.FnGet = ()=>{
				var v = oldGet();
				if(v is UnixMs t){
					return t.ToIso();
				}
				if(v is IIdUInt128 id){
					return id.ToString();
				}
				if(v is IdDel del){
					return del.Value;
				};
				return v;
			};
			var oldSet = u.FnSet;
			u.FnSet = (v)=>{
				if(v is not str s){
					return false;
				}
				if( u.Type?.IsAssignableTo(typeof(UnixMs))==true ){
					return oldSet(UnixMs.FromIso(s));
				}
				if( u.Type?.IsAssignableTo(typeof(IdWord))==true ){
					return IdWord.FromLow64Base(s);
				}
				if(u.Type == typeof(IdDel)){
					return oldSet(IdDel.FromUnixMs(long.Parse(s)));
				}
				return true;
			};
			PathToUiMap[k] = u;
		}

		return R;
	}

	public static UiText UiT(str Raw){
		return UiText.FromRawText(Raw);
	}

}
