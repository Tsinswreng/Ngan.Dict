using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.Kv.Svc;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Word.Svc;
using Ngan.Dict.Backend.Db.TswG;
using Ngan.Dict.Backend.Word.Dao;
using Tsinswreng.CsCore;
using Tsinswreng.CsPage;
using Tsinswreng.CsSql;
namespace Ngan.Dict.Test.Try;

public class TryRepoBat{
	public IServiceProvider SvcProvdr;
/* 
BatchSize=50{
Pg:
SlctManyInIds: 1507ms
BatSlctById: 4628ms
SlctOneById: 23890ms
allIds.Count: 113372

Sqlite:
SlctManyInIds: 158ms
BatSlctById: 741ms
SlctOneById: 568ms
allIds.Count: 13304
}

 */
public async Task Run(CT Ct){
	var RepoWord = SvcProvdr.GetRequiredService<IRepo<PoWord, IdWord>>();
	var Ctx = new DbFnCtx();
	var fnPageAll = await RepoWord.FnPageAll(Ctx, Ct);
	var all = await fnPageAll(PageQry.SlctI64Max(), Ct);
	var data = all.DataAsyE.OrEmpty();
	var allIds = await data.Select(x=>x.Id).ToListAsync(Ct);
	var sw = Stopwatch.StartNew();
	{
		sw.Restart();
		var r = await RepoWord.SlctManyInIdsWithDel(Ctx, allIds, Ct);
		var list = await r.ToListAsync(Ct);
		sw.Stop();
		Console.WriteLine($"SlctManyInIds: {sw.ElapsedMilliseconds}ms");
	}
	{
		sw.Restart();
		var r = await RepoWord.BatSlctById(Ctx, allIds, Ct);
		var list = await r.ToListAsync(Ct);
		sw.Stop();
		Console.WriteLine($"BatSlctById: {sw.ElapsedMilliseconds}ms");
	}
	{
		var slctOne = await RepoWord.FnSlctOneById(Ctx, Ct);
		sw.Restart();
		foreach(var id in allIds){
			var po = await slctOne(id, Ct);
		}
		sw.Stop();
		Console.WriteLine($"SlctOneById: {sw.ElapsedMilliseconds}ms");
	}
	System.Console.WriteLine("allIds.Count: "+ allIds.Count);
}

}
