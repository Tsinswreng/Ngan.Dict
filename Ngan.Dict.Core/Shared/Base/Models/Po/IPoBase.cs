//#define Impl
namespace Ngaq.Core.Shared.Base.Models.Po;

using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Tools;
using Tsinswreng.CsTempus;

public partial interface IPoBase
	:I_ShallowCloneSelf
	,IAppSerializable
	,I_DelAt
{
	/// 留與觸發器或攔截器、改實體保存旹自動改ᵣ「改ˡ時」ˇ
	public UnixMs DbCreatedAt{get;set;}
	/// 留與觸發器或攔截器、增實體旹自動改ᵣ「添ˡ時」ˇ
	public UnixMs DbUpdatedAt{get;set;}
}

public interface IBizCreateUpdateTime{
	#region IBizCreateUpdateTime
	/// 理則ₐ實體ˇ增ʹ時、如于單詞、則始記于文本單詞表中之時 即其CreatedAt、非 存入數據庫之時
	/// 潙null旹示與InsertedBy同。亦可早於InsertedAt。
	public UnixMs BizCreatedAt{get;set;}
	#if Impl
		= new();
	#endif
	/// 理則ₐ實體ˇ改ʹ時
	/// 如ʃ有ʹ子實體ˋ變˪、則亦宜改主實體或聚合根ʹUpdatedAt
	public UnixMs BizUpdatedAt{get;set;}
	#if Impl
		= Tempus.Zero;
	#endif

	#endregion IBizCreateUpdateTime

}


public static class ExtnBizTime{
	extension(IBizCreateUpdateTime z){
		public UnixMs LatestBizTime{
			get{
				if(z.BizUpdatedAt.IsNullOrDefault()){
					return z.BizCreatedAt;
				}
				return z.BizUpdatedAt;
			}
		}
	}
	extension<TPo>(TPo z)
		where TPo : IBizCreateUpdateTime
	{
		public TPo Touch(UnixMs? NeoUpdTime = null){
			NeoUpdTime??=UnixMs.Now();
			z.BizUpdatedAt = NeoUpdTime.Value;
			return z;
		}
	}
	extension<TPo>(IEnumerable<TPo> z)
		where TPo : IBizCreateUpdateTime
	{
		public IEnumerable<TPo> Touch(UnixMs? NeoUpdTime = null){
			return z.Select(x=>{
				x.Touch(NeoUpdTime);
				return x;
			});
		}
	}
	
	extension<TPo>(IAsyncEnumerable<TPo> z)
		where TPo : IBizCreateUpdateTime
	{
		public IAsyncEnumerable<TPo> Touch(UnixMs? NeoUpdTime = null){
			return z.Select(x=>{
				x.Touch(NeoUpdTime);
				return x;
			});
		}
	}
}
