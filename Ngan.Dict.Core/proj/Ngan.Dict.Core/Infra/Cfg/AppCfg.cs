namespace Ngan.Dict.Core.Infra.Cfg;

using Tsinswreng.CsCore;
using Ngan.Dict.Core.Infra.Url;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCfg.Json;

[Doc($$"""
本應用的客戶端配置門面（CsCfg 的雙源疊加）。

*雙源*：兩份 JSONC 配置檔疊在一起，只讀那份在前——

1. `Ngan.Dict.jsonc`（出廠）：開發者手寫、進版本庫、可帶註釋。可以不存在，不存在就是空源。
2. `Ngan.Dict.Rw.jsonc`（日常）：GUI 保存的那份，唯一可寫源。首次啟動時自動建立。

*為何要分兩份*：手寫那份要能放註釋與排版；程序存檔是整份重寫，註釋一定保不住。
兩件事放同一份檔案裡必然互相破壞，故分開。

*同一個鍵兩邊都有時*：出廠那份贏。這是「出廠設定」該有的樣子；
`TryGet` 會吐出命中的源，設定頁據此顯示「此項由配置文件接管」。

*哪些事屬於本類而不屬於庫*：檔名、誰只讀誰可寫、日常檔位置由哪個鍵決定——這是本應用的策略。

#Eg[
	```cs
	// 宿主在建立容器之前接好，之後任何地方都能用
	AppCfg.Inst.Setup(Di.GetRSvc<I_GetBaseDir>());

	var Lang = AppCfg.Inst.Get(KeysClientCfg.Lang);
	AppCfg.Inst.SetNoSave(KeysClientCfg.Lang, "en");
	await AppCfg.Inst.Save(default);
	```
]
""")]
public partial class AppCfg : LayeredCfgAccessor {

	[Doc("全局唯一實例。宿主的 `DiXxx` 在建容器之前呼叫 Setup。")]
	public static AppCfg Inst { get; } = new();

	[Doc("開發者手寫的出廠配置檔名，位於應用儲存區下。可以不存在。")]
	public const str RoCfgFileName = "Ngan.Dict.jsonc";

	[Doc("出廠那份源的技術標識；設定頁據此顯示「這一項由誰提供」。")]
	public const str RoSrcName = "file";

	[Doc("日常那份源的技術標識。")]
	public const str RwSrcName = "gui";

	[Doc("出廠那份的讀者；`Setup` 之後才有值。對外只給讀，這一層不該有人以為它寫得進去。")]
	public ICfgReader? RoCfg { get; private set; }

	[Doc("日常那份的訪問器；`Setup` 之後才有值。要寫配置就走這裡，不要自己去開檔。")]
	public ICfgAccessor? RwCfg { get; private set; }

	[Doc("兩份配置檔的實際絕對路徑，供日誌與診斷顯示。")]
	public str RoCfgPath { get; private set; } = "";

	[Doc("日常配置檔的實際絕對路徑，供日誌與診斷顯示。")]
	public str RwCfgPath { get; private set; } = "";

	[Doc($$"""
	按應用儲存區接上兩個源並疊起來。宿主各呼叫一次，必須早於任何取配置的地方。

	日常配置檔的位置由引導鍵 {{nameof(KeysClientCfg.RwCfgPath)}} 決定，所以必須先讀過出廠那份。

	#Prm[{{nameof(BaseDir)}}][應用儲存區根目錄；桌面是執行檔目錄，Android 是外部私有目錄。]
	#Throw[{{nameof(IOException)}}][檔案開不起來時；那屬於部署錯誤，不靜默降級。]
	""")]
	public partial void Setup(I_GetBaseDir BaseDir);
}
