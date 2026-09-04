using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Shared.Sync;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Backend.Test.Domains.Word;

/// OrdSyncJnWordByBizId_2 的測試:直接轉發原 OrdSyncJnWordByBizId 的用例(場景與斷言照搬、
/// 僅把被測入口換成 _2 版),不自創新場景。註冊入口見 _TestISvcWordV2.cs。
public partial class TestISvcWordV2{
	void RegisterBizSyncJnWordByBizId_2(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2_2.OrdSyncJnWordByBizId_2)];

		// 轉發入口:同一注入實例按新接口取用(後端 SvcWordV2 同時實現 ISvcWordV2_2)。
		ISvcWordV2_2 Svc2(){
			return (ISvcWordV2_2)SvcWordV2;
		}

		R("BizSyncJnWordByBizId_2_WhenLocalNotExist_Should_InsertAndReturnAuditDto", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_bizsync_add_" + Guid.NewGuid().ToString("N");
			var remote = MkSyncInput(owner, token + "_h1", "en", token + "_d1");
			try{
				var dtos = await ToList(Svc2().OrdSyncJnWordByBizId_2(MkUserCtx(owner), AsyE(remote), CT.None));
				Assert.IsTrue(dtos.Count == 1, "BizSyncJnWordByBizId_2 should return one dto for one input item");
				Assert.IsTrue(ReferenceEquals(dtos[0].DiffResult, EDiffByBizIdResultForSync.LocalNotExist), "BizSyncJnWordByBizId_2 should mark fresh remote as LocalNotExist");

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == remote.Head && x.Lang == remote.Lang)
						.ToList();
					Assert.IsTrue(words.Count == 1, "BizSyncJnWordByBizId_2 should insert word when local does not exist");
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("BizSyncJnWordByBizId_2_ReSyncSameData_Should_NotDuplicateRows", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_bizsync_resync_" + Guid.NewGuid().ToString("N");
			var remote = MkSyncInput(owner, token + "_h1", "en", token + "_d1");
			try{
				_ = await ToList(Svc2().OrdSyncJnWordByBizId_2(MkUserCtx(owner), AsyE(remote), CT.None));
				var dtos2 = await ToList(Svc2().OrdSyncJnWordByBizId_2(MkUserCtx(owner), AsyE(remote), CT.None));
				Assert.IsTrue(dtos2.Count == 1, "BizSyncJnWordByBizId_2 should still return one dto on re-sync");

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == remote.Head && x.Lang == remote.Lang)
						.ToList();
					Assert.IsTrue(words.Count == 1, "BizSyncJnWordByBizId_2 should not duplicate root rows for same biz-id");
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("BizSyncJnWordByBizId_2_Should_KeepOwnerIsolation", async(o)=>{
			var ownerA = new IdUser();
			var ownerB = new IdUser();
			var token = "ut_wv2_bizsync_owner_" + Guid.NewGuid().ToString("N");
			var remoteA = MkSyncInput(ownerA, token + "_h1", "en", token + "_d1");
			var localB = new PoWord{Id = new IdWord(), Owner = ownerB, Head = remoteA.Head, Lang = remoteA.Lang};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(localB), CT.None);
					return NIL;
				});

				_ = await ToList(Svc2().OrdSyncJnWordByBizId_2(MkUserCtx(ownerA), AsyE(remoteA), CT.None));
				await RunNoTxn(async(Ctx)=>{
					var wordsA = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == ownerA && x.Head == remoteA.Head && x.Lang == remoteA.Lang)
						.ToList();
					var wordsB = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == ownerB && x.Head == remoteA.Head && x.Lang == remoteA.Lang)
						.ToList();
					Assert.IsTrue(wordsA.Count == 1 && wordsB.Count == 1, "BizSyncJnWordByBizId_2 should isolate by owner");
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(ownerA, token);
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdHardDelById(Ctx, AsyE(localB.Id), CT.None);
					return NIL;
				});
			}
		});
	}
}