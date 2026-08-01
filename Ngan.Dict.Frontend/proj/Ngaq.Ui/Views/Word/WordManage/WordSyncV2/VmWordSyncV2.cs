namespace Ngaq.Ui.Views.Word.WordManage.WordSyncV2;

using System.IO;
using System.IO.Abstractions;
using Avalonia.Threading;
using Ngaq.Client.Word.Svc;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Tsinswreng.CsCfg;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 單詞同步 V2 頁面的 ViewModel。
public partial class VmWordSyncV2: ViewModelBase{
	ClientWordSyncV2? ClientWordSyncV2;
	ISvcWordV2? SvcWordV2;
	IFrontendUserCtxMgr? UserCtxMgr;
	ICfgAccessor? Cfg;
	IFileSystem? FileSystem;

	/// 構造函數（依賴注入）。
	/// <param name="ClientWordSyncV2">雲端同步客戶端。</param>
	/// <param name="SvcWordV2">本地單詞 V2 服務。</param>
	/// <param name="UserCtxMgr">前端用戶上下文管理器。</param>
	/// <param name="Cfg">配置訪問器。</param>
	public VmWordSyncV2(
		ClientWordSyncV2? ClientWordSyncV2
		,ISvcWordV2? SvcWordV2
		,IFrontendUserCtxMgr? UserCtxMgr
		,ICfgAccessor? Cfg
		,IFileSystem? FileSystem
	){
		this.ClientWordSyncV2 = ClientWordSyncV2;
		this.SvcWordV2 = SvcWordV2;
		this.UserCtxMgr = UserCtxMgr;
		this.Cfg = Cfg;
		this.FileSystem = FileSystem;

		if(Cfg is not null){
			PathExport = Cfg.Get(KeysClientCfg.Word.WordsPackExportPath)??"";
			PathImport = Cfg.Get(KeysClientCfg.Word.WordsPackImportPath)??"";
		}
	}

	/// 供設計期使用的保底構造。
	protected VmWordSyncV2(){

	}

	/// 導出文件路徑。
	public str PathExport{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 導入文件路徑。
	public str PathImport{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 上傳本地詞庫到雲端。
	/// <param name="Ct">取消令牌。</param>
	/// <returns>成功返回 NIL。</returns>
	public async Task<nil> PushAsy(CT Ct){
		try{
			if(ClientWordSyncV2 is null){
				return NIL;
			}
			await Task.Run(async()=>{
				await ClientWordSyncV2.Push(Ct);

			});
			Dispatcher.UIThread.Post(()=>{
				ShowToast(I18n[K.Push]+" "+I18n[K.Saved]);
			});
		}catch (System.Exception Ex){
			HandleErr(Ex);
		}
		return NIL;
	}

	/// 從雲端拉取詞庫到本地。
	/// <param name="Ct">取消令牌。</param>
	/// <returns>成功返回 NIL。</returns>
	public async Task<nil> PullAsy(CT Ct){
		if(ClientWordSyncV2 is null){
			return NIL;
		}
		try{
			await Task.Run(async()=>{
				await ClientWordSyncV2.Pull(Ct);
			});
			Dispatcher.UIThread.Post(()=>{
				ShowToast(I18n[K.Pull]+" "+I18n[K.Saved]);
			});
		}catch(Exception Ex){
			HandleErr(Ex);
		}
		return NIL;
	}

	/// 導出本地詞庫到文件（含軟刪）。
	/// <param name="Ct">取消令牌。</param>
	/// <returns>成功返回 NIL。</returns>
	public async Task<nil> ExportAsy(CT Ct){
		if(AnyNull(SvcWordV2, UserCtxMgr, FileSystem)){
			return NIL;
		}
		if(str.IsNullOrWhiteSpace(PathExport)){
			ShowDialog(I18n[K.InvalidPath]);
			return NIL;
		}
		if(FileSystem.File.Exists(PathExport)){
			ShowDialog(I18n[K.FileAlreadyExistsNoOverwriteChangePath]);
			return NIL;
		}
		try{
			// 先確保目標文件可建立，避免導出後才發現路徑不合法。
			await Task.Run(async()=>{
				if(!TryEnsureExportFile(PathExport)){
					Dispatcher.UIThread.Post(()=>{
						ShowDialog(I18n[K.InvalidPath]);
					});
					return;
				}

				using var packed = await SvcWordV2.PackAllWordWithDel(UserCtxMgr.GetDbUserCtx(), Ct);
				using var file = FileSystem.File.Open(PathExport, FileMode.Create, FileAccess.Write, FileShare.None);
				await packed.CopyToAsync(file, Ct);

				Cfg?.Set(KeysClientCfg.Word.WordsPackExportPath, PathExport);
				Cfg?.Save(default);
			});
			Dispatcher.UIThread.Post(()=>{
				ShowToast(I18n[K.Export]+" "+I18n[K.Saved]);
			});

		}catch(Exception Ex){
			HandleErr(Ex);
		}
		return NIL;
	}

	/// 從文件導入詞庫並按 BizId 規則同步到本地。
	/// <param name="Ct">取消令牌。</param>
	/// <returns>成功返回 NIL。</returns>
	public async Task<nil> ImportAsy(CT Ct){
		if(AnyNull(SvcWordV2, UserCtxMgr, FileSystem)){
			return NIL;
		}
		if(str.IsNullOrWhiteSpace(PathImport) || !FileSystem.File.Exists(PathImport)){
			ShowDialog(I18n[K.InvalidPath]);
			return NIL;
		}

		try{
			await Task.Run(async()=>{
				using var file = FileSystem.File.OpenRead(PathImport);
				await foreach(var _ in SvcWordV2.OrdSyncJnWordByBizIdFromStream(
					UserCtxMgr.GetDbUserCtx(),
					file,
					Ct
				)){
					// 消費枚舉以觸發實際同步。
				}

				Cfg?.Set(KeysClientCfg.Word.WordsPackImportPath, PathImport);
				Cfg?.Save(default);
			});
			Dispatcher.UIThread.Post(()=>{
				ShowToast(I18n[K.Import]+" "+I18n[K.Saved]);
			});

		}catch(Exception Ex){
			HandleErr(Ex);
		}
		return NIL;
	}

	/// 在抽象文件系統上確保導出路徑可寫，兼容多平台測試與Web兜底。
	bool TryEnsureExportFile(str FilePath){
		if(AnyNull(FileSystem)){
			return false;
		}
		try{
			var dir = FileSystem.Path.GetDirectoryName(FilePath);
			if(!str.IsNullOrWhiteSpace(dir)){
				FileSystem.Directory.CreateDirectory(dir);
			}
			if(!FileSystem.File.Exists(FilePath)){
				using var _ = FileSystem.File.Create(FilePath);
			}
			return FileSystem.File.Exists(FilePath);
		}catch{
			return false;
		}
	}
}
