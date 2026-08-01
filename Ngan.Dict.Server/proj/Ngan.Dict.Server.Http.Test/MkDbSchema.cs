
using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Server.Db.TswG;
using Ngan.Dict.Server.Db.TswG.Migrations;
using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.Kv.Svc;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Backend.Db.TswG;
using Ngan.Dict.Backend.Word.Dao;
using Tsinswreng.CsSql;
using Ngan.Dict.Server.Http;
namespace Ngan.Dict.Server.Http.Test;

public class MkDbSchema{
	public static async Task InitDb(str[] args){
		var app = NgaqWeb.InitApp(args);
		var fullInit = app.Services.GetRequiredService<FullInit>();
		await fullInit.Up(default);
		System.Console.WriteLine("done");
	}

	public static async Task MigrateDb(str[] args){
		var app = NgaqWeb.InitApp(args);
		var migrator = app.Services.GetRequiredService<MigrationRunner>();
		await migrator.Up(default);
		System.Console.WriteLine("migrate done");
	}
}
