using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;

//using Microsoft.CodeAnalysis.Text;
using Tsinswreng.CsSql;
using Ngan.Dict.Backend.Db.TswG;
using Ngan.Dict.Backend.Sql;
using Ngan.Dict.Core.Infra;
using Tsinswreng.CsSql.Sqlite;
using Tsinswreng.CsSql.Sqlite.Di;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Infra.Url;
using Tsinswreng.CsRefl;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngan.Dict.Core.Shared.Word.Models.Po.UserLang;
using Ngan.Dict.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngan.Dict.Core.Shared.Dictionary.Models.Po.NormLang;
using Tsinswreng.CsCore;


namespace Ngan.Dict.Backend.Di;

[Doc("僅限本地(客戶端)後端使用、勿用于Server")]
public static class DiLocal{

	public static IServiceCollection SetupLocal(this IServiceCollection z){
		z.AddSingleton<I_GetBaseDir>(BaseDirMgr.Inst);
		z.SetupDbCfg().SetupRepos().SetupCommonBackend();
		//Core詞典映射(型別元資料來源)
		z.AddSingleton<ITypeInfoSrc>(AppTypeInfoSrc.Inst);
		return z;
	}



//數據庫配置
	static IServiceCollection SetupDbCfg(this IServiceCollection z){
//CsSql 固定配方(連接/命令/事務/表管理器)一次註冊;此處剩下的是業務自己的註冊(遷移/初始化)
z.AddCsSqlSqlite(new SqliteCfg{
	Connection = LocalDb.Inst.DbConnection
	,TblMgr = LocalTblMgr.Inst
});
//遷移用空白 TblMgr 工廠
z.AddSingleton<IMkrTblMgr, LocalMkrTblMgr>();
//數據庫初始化器
z.AddTransient<DbIniter>();
// 通用遷移執行邏輯在 CsSql；此處僅聲明 Local 端遷移清單
z.AddSingleton<IMigrationMgr>(sp=>
	new MigrationMgr(
		TblMgr: sp.GetRequiredService<ITblMgr>()
		,SqlCmdMkr: sp.GetRequiredService<ISqlCmdMkr>()
	)
	.UseLocalMigrations(sp)
);
return z;
	}

	static IServiceCollection AddRepoScoped<TEntity, TId>(
		this IServiceCollection z
	)where TEntity:class, new()
	{
		//z.AddScoped<IRepo<TEntity, TId>, EfRepo<TEntity, TId>>();
		z.AddScoped<IRepo<TEntity, TId>, AppRepo<TEntity, TId>>();
		return z;
	}

//倉儲
	static IServiceCollection SetupRepos(this IServiceCollection z){
z.AddRepoScoped<SchemaHistory, i64>();
z.AddRepoScoped<PoWord, IdWord>();
z.AddRepoScoped<PoWordProp, IdWordProp>();
z.AddRepoScoped<PoWordLearn, IdWordLearn>();
z.AddRepoScoped<PoUserLang, IdUserLang>();
z.AddRepoScoped<PoNormLang, IdNormLang>();
z.AddRepoScoped<PoNormLangToUserLang, IdNormLangToUserLang>();
z.AddRepoScoped<PoKv, IdKv>();
z.AddRepoScoped<PoStudyPlan, IdStudyPlan>();
z.AddRepoScoped<PoWeightArg, IdWeightArg>();
z.AddRepoScoped<PoWeightCalculator, IdWeightCalculator>();
z.AddRepoScoped<PoPreFilter, IdPreFilter>();
//z.AddScoped<IRunInTxn, AdoTxnRunner>();
return z;
	}


//服務類

}
