namespace Ngan.Dict.Backend.Domains.Word.Dao;

using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;

public partial class DaoWordV2
{
	/// 批量按 (Owner, Head, Lang) 查單詞主體(含軟刪)——批原語版(`_2` 後綴)。
	/// 與流式版的差別只在形狀:入參收一批 {nameof(IList)}、出參直接回位置對齊的 {nameof(IList<PoWord?>)}、
	/// 查無補 null——物化時機 = 批邊界(由調用方 SqlFlow.Batches 切好的批),批內不再有業務 ToListAsync。
	/// 位置對齊語義與底層流式版完全一致(RunDupliSql 同構批量、每元素一條等值 SQL)。
	/// <param name="Ctx">數據庫上下文</param>
	/// <param name="Owner">詞庫擁有者</param>
	/// <param name="Keys">同批的 (Head,Lang) 列表</param>
	/// <param name="Ct">取消令牌</param>
	/// <returns>與 Keys 位置一一對應的主體列表、查無補 null</returns>
	public async Task<IList<PoWord?>> BatGetPoWordByOwnerHeadLangWithDel_2(
		IDbFnCtx Ctx,
		IdUser Owner,
		IList<Head_Lang> Keys,
		CT Ct
	){
		var rows = await BatGetPoWordByOwnerHeadLangWithDel(
			Ctx, Owner, ToolAsyE.ToAsyE(Keys), Ct
		).ToListAsync(Ct);
		return rows;
	}

	/// 批量按 Id 取完整聚合(根+Props+Learns、含軟刪)——批原語版(`_2` 後綴)。
	/// 入參收一批 Id、出參直接回位置對齊的 {nameof(IList<JnWord?>)}、查無補 null。
	/// #See[{nameof(BatGetPoWordByOwnerHeadLangWithDel_2)}]
	/// <param name="Ctx">數據庫上下文</param>
	/// <param name="Ids">同批的根 Id 列表(調用方應先去重)</param>
	/// <param name="Ct">取消令牌</param>
	/// <returns>與 Ids 位置一一對應的聚合列表、查無補 null</returns>
	public async Task<IList<JnWord?>> BatGetJnWordByIdWithDel_2(
		IDbFnCtx Ctx,
		IList<IdWord> Ids,
		CT Ct
	){
		var rows = await BatGetJnWordByIdWithDel(
			Ctx, ToolAsyE.ToAsyE(Ids), Ct
		).ToListAsync(Ct);
		return rows;
	}
}