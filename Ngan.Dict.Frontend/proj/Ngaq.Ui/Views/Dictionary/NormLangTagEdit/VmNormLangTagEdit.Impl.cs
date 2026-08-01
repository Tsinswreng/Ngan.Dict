namespace Ngaq.Ui.Views.Dictionary.NormLangTagEdit;

using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Ui.Views.Dictionary.NormLangTag;
using Tsinswreng.CsCfg;
using Ctx = VmNormLangTagEdit;

/// 詞典源語言快捷標籤編輯 ViewModel 的函數實現。
public partial class VmNormLangTagEdit{
	protected partial VmNormLangTagEdit(){
		Cfg = AppCfg.Inst;
	}

	public static partial Ctx Mk(){
		return new Ctx();
	}

	public partial VmNormLangTagEdit(ICfgAccessor? Cfg){
		this.Cfg = Cfg ?? AppCfg.Inst;
	}

	public partial nil FromTags(IEnumerable<VmNormLangTag> Tags){
		this.Tags.Clear();
		foreach(var Tag in Tags){
			this.Tags.Add(CloneTag(Tag));
		}
		return NIL;
	}

	public partial nil SetOnSaved(Action<IList<VmNormLangTag>> FnOnSaved){
		OnSaved = FnOnSaved;
		return NIL;
	}

	public partial nil Add(PoNormLang Po){
		if(Contains(Po)){
			return NIL;
		}
		var Tag = VmNormLangTag.Mk();
		Tag.FromCfg(new(){Type = Po.Type, Code = Po.Code ?? ""}, Po);
		Tags.Add(Tag);
		return NIL;
	}

	public partial nil MoveUp(VmNormLangTag Tag){
		var Index = Tags.IndexOf(Tag);
		if(Index > 0){
			Tags.Move(Index, Index - 1);
		}
		return NIL;
	}

	public partial nil MoveDown(VmNormLangTag Tag){
		var Index = Tags.IndexOf(Tag);
		if(Index >= 0 && Index < Tags.Count - 1){
			Tags.Move(Index, Index + 1);
		}
		return NIL;
	}

	public partial nil Remove(VmNormLangTag Tag){
		Tags.Remove(Tag);
		return NIL;
	}

	public async partial Task<nil> Save(CT Ct){
		var SavedTags = Tags.Select(CloneTag).ToList();
		await DictionarySrcLangTagCfg.Save(Cfg, SavedTags.Select(X=>X.ToCfg()), Ct);
		OnSaved?.Invoke(SavedTags);
		return NIL;
	}

	private static partial VmNormLangTag CloneTag(VmNormLangTag Tag){
		var R = VmNormLangTag.Mk();
		R.Type = Tag.Type;
		R.Code = Tag.Code;
		R.Text = Tag.Text;
		R.NativeName = Tag.NativeName;
		R.IsSelected = Tag.IsSelected;
		return R;
	}

	private partial bool Contains(PoNormLang Po){
		return Tags.Any(X=>X.IsLang(Po.Type, Po.Code ?? ""));
	}
}
