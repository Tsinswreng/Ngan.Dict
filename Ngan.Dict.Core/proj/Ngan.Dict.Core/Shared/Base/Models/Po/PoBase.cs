namespace Ngan.Dict.Core.Shared.Base.Models.Po;

using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Tools;
using Tsinswreng.CsCore;
using Tsinswreng.CsEntity;
using Tsinswreng.CsRefl;
using Tsinswreng.CsSql;
using Tsinswreng.CsTempus;

[Doc($$"""
{{nameof(IPoBase)}} 的默認實現：直接繼承 CsEntity 的 {{nameof(BasePoBase)}}，
三共同列、淺複製與軟刪列都不再在本項目重複聲明。
""")]
public partial class PoBase : BasePoBase, IPoBase
{
}

[Doc($$"""
在 {{nameof(PoBase)}} 之上再帶「業務層創建／修改時間」的實體基底。

*為何業務時間與數據庫時間要分開*：{{nameof(IBizCreateUpdateTime.BizCreatedAt)}} 是
「這條記錄在業務上何時產生」（例如單詞首次進入生詞表的時刻），可以早於入庫；
{{nameof(IBasePoBase.DbCreatedAt)}} 只是這一行被寫進數據庫的時刻。
""")]
public partial class PoBaseBizTime : PoBase, IBizCreateUpdateTime
{
	// CsEntity 的 IBizCreateUpdateTime 只在庫自己編譯時（Impl 符號）帶默認實現，
	// 消費方得自己給成員，故這裡顯式聲明並給初值。
	[Doc("業務層的創建時間；與 DbCreatedAt 不同，可以早於入庫。")]
	public virtual UnixMs BizCreatedAt{get;set;} = new();

	[Doc("業務層的修改時間；子實體變更時也宜由聚合根一起改。")]
	public virtual UnixMs BizUpdatedAt{get;set;} = UnixMs.Zero;
}


public static class ExtnPoBase {
	extension<T>(T z)
		where T : IPoBase
	{
		[Doc(@$"用Equals比較 而非等号。
		推薦用 {nameof(Extn.EqObj)}來比較、因佢能比較null、不致空指針。
		")]
		public obj? Id_() {
			if(AppTypeInfoSrc.Inst.TryGet<T>(z, nameof(I_Id<>.Id), out var Id)){
				return Id;
			}
			throw new InvalidOperationException($@"{typeof(T)} {z} does not have {nameof(I_Id<>)}
			or it is not registered in {nameof(AppTypeInfoSrc)}
			");
		}
		public bool Id_(obj? Id) {
			return AppTypeInfoSrc.Inst.TrySet<T>(z, nameof(I_Id<>.Id), Id);
		}

	}
}
