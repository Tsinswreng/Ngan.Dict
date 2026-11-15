= 文件結構說明 2025-08-24T23:02:08.531+08:00

- `bin/`
- `DiCore.cs` 依賴注入配置（`ILogger`／配置四寬度／`IJsonSerializer`／`ITypeInfoSrc`）
- `GenAppJsonCtx`（Ngan.Dict.Scripts 之入口）生成 AppJsonCtx 之註冊代碼（輸出至 `Tsinswreng.SrcGen/Ngan.Dict.Gen.g.cs`）
- `Infra/AnswerBox.cs` `IAnswer<T>` 的引用型實現（庫裏的 `Answer<T>` 是 struct，過不了 `AddErr` 的 class 約束）
- `Infra/AppTypeInfoSrc.cs` 型別元資料來源（Tsinswreng.CsRefl；取代舊 Tsinswreng.Srefl 之 CoreDictMapper）
- `Infra/AppJsonCtx.cs` JSON 源生成上下文（選項與轉換器清單；清單由生成器產出）
- `Infra/Cfg/` 配置：`AppCfg`（CsCfg 雙源疊加）／`KeysClientCfg`（CsKeyNode 鍵族）
- `Infra/Errors/` 異常鍵族 `KeysErr` 與 `AppErrTags`
- `Infra/Log/AppLog.cs` 日誌門面（`DelegatingLogger`，各宿主在入口指定內層）
- `Infra/Url/` 儲存區與遠端基地址抽象
- `Infra/YamlMd/` 內置的 YamlMd 解析（原 `Tsinswreng.CsYamlMd`，供 LLM 詞典響應解析）
- `GlobalUsing.cs`
- `Shared/` 業務共享層（實體、DTO、Svc 接口）
- `Tools/`
- `Tsinswreng.SrcGen/` 生成物（勿手改；改完 DTO／實體要重跑 `GenAppJsonCtx`）
- `UInt128Template.typedid` 強類型 Id 的源生成模板（`IAppIdU128`、UInt128 底、對外 Low64Base）
- `README.typ`
- `obj/`
