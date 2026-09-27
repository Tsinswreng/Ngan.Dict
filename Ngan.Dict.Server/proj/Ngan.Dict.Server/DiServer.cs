namespace Ngan.Dict.Server;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsSql;
using Tsinswreng.CsSql.Postgres;
using Tsinswreng.CsSql.Postgres.Di;
using Ngan.Dict.Server.Db.User;
using Ngan.Dict.Core.Model.Sys.Po.Password;
using Ngan.Dict.Server.Infra.Cfg;
using Ngan.Dict.Server.Db.TswG;
using Npgsql;
using Ngan.Dict.Core.Models.Sys.Po.Password;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Server.Db.TswG.Migrations;
using Ngan.Dict.Backend.Db.TswG;
using Ngan.Dict.Server.Domains.User.Svc;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Server.Domains.User.Dao;
using Ngan.Dict.Core.Shared.User.Models.Po.RefreshToken;
using Ngan.Dict.Core.Model.Sys.Po.RefreshToken;
using Ngan.Dict.Core.Shared.User.Svc;
using Tsinswreng.CsCfg;
using Ngan.Dict.Backend.Domains.Word.Svc;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Backend.Word.Dao;
using Tsinswreng.CsTools;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Model.Po.Kv;
using StackExchange.Redis;
using Tsinswreng.CsRefl;
using Ngan.Dict.Core.Shared.Word.Models.Po.UserLang;
using Ngan.Dict.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngan.Dict.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Tsinswreng.CsCore;
using Ngan.Dict.Backend.Di;

public static class DiBiz{
	
	[Doc("數據庫與ORM基礎設施")]
	static IServiceCollection SetupTswgSqlAdo(this IServiceCollection z){
		//CsSql 固定配方(連接池/命令/事務/表管理器)一次註冊;此處剩下的是業務自己的註冊(遷移/整理)
		z.AddCsSqlPostgres(new PostgresCfg{
			DataSource = ServerDb.Inst.DataSource
			,TblMgr = ServerTblMgr.Inst
		});

		z.AddRepoScoped<SchemaHistory, i64>();
		// 通用遷移執行邏輯在 CsSql；此處僅聲明 Biz 端遷移清單
		z.AddScoped<IMigrationMgr>(sp=>
			new MigrationMgr(
				TblMgr: sp.GetRequiredService<ITblMgr>()
				,SqlCmdMkr: sp.GetRequiredService<ISqlCmdMkr>()
			)
			.UseServerMigrations()
		);
		z.AddTransient<MigrationRunner>();

		z.AddTransient<FullInit>();

		return z;
	}

	static IServiceCollection AddRepoScoped<TEntity, TId>(
		this IServiceCollection z
	)where TEntity:class, new()
	{
		z.AddScoped<IRepo<TEntity, TId>, AppRepo<TEntity, TId>>();
		return z;
	}
	public static IServiceCollection SetupServer(this IServiceCollection z){
		SetupRepos(z);
		SetupUser(z);
		z.SetupCommonBackend();
		z.AddSingleton<ICfgAccessor>(ServerCfg.Inst);
		z.SetupTswgSqlAdo();
		z.AddSingleton<ITypeInfoSrc>(AppTypeInfoSrc.Inst);
		
		z.AddRepoScoped<PoWord, IdWord>();
		z.AddRepoScoped<PoWordLearn, IdWordLearn>();
		z.AddRepoScoped<PoWordProp, IdWordProp>();

		z.AddScoped<ISvcUser>(sp=>sp.GetRequiredService<SvcUser>());
		z.AddRepoScoped<PoRefreshToken, IdRefreshToken>();

		z.AddScoped<DaoToken>();
		z.AddScoped<ISvcToken, SvcToken>();

		z.AddScoped<ISvcWord, SvcWord>();
		z.AddScoped<ISvcParseWordList, SvcParseWordList>();
		z.AddScoped<DaoWord, DaoWord>();

		// 配置 Redis 连接
		var Cfg = ServerCfg.Inst;
		var port = Cfg.Get(KeysServerCfg.Redis.Port);
		var host = Cfg.Get(KeysServerCfg.Redis.Host);
		var password = Cfg.Get(KeysServerCfg.Redis.Password);
		var instanceName = Cfg.Get(KeysServerCfg.Redis.InstanceName);

		var redisConfig = new ConfigurationOptions{
			AbortOnConnectFail = false
		};
		redisConfig.EndPoints.Add(host, port);
		if(!str.IsNullOrWhiteSpace(password)){
			redisConfig.Password = password;
		}

		z.AddStackExchangeRedisCache(o =>{
			o.ConfigurationOptions = redisConfig;
			o.InstanceName = instanceName;
		});

		var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig);
		z.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);


		// var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
		// // 可选：根据需要配置其他选项，例如：
		// configurationOptions.AbortOnConnectFail = false; // 防止连接失败时立即中止
		// configurationOptions.Ssl = true; // 如果需要 SSL 连接
		// z.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configurationOptions));
		return z;
	}
	
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
	
	static IServiceCollection SetupUser(this IServiceCollection z){
		z.AddRepoScoped<PoUser, IdUser>();
		z.AddRepoScoped<PoPassword, IdPassword>();
		z.AddTransient<DaoUser>();
		z.AddTransient<SvcUser>();
		return z;
	}
	
	

/*
.AddStackExchangeRedisCache(opt=>{
			var RedisConnStr = CfgItems.Redis.Host.GetFrom(Cfg)+":"+CfgItems.Redis.Port.GetFrom(Cfg);
			opt.Configuration = RedisConnStr;
			opt.InstanceName = CfgItems.Redis.InstanceName.GetFrom(Cfg);
		});
 */
}
