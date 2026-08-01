namespace Ngaq.Ui.Views.Word.WordManage.WordSyncV2;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.WordSync;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmWordSyncV2;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;


/// 單詞同步 V2 頁面。
/// 提供：
/// - 文件導入/導出（含文件選擇器）
/// - 雲端 Push / Pull
public partial class ViewWordSyncV2
	:AppViewBase<Ctx>
	,I_MkTitleMenu
{

	public ViewWordSyncV2(){
		Ctx = App.GetRSvc<Ctx>();
		Style();
		Render();
	}

	public partial class Cls_{

	}
	public Cls_ Cls{get;set;} = new Cls_();

	protected nil Style(){
		return NIL;
	}

	OpBtn CenterOpBtn(){
		var r = new OpBtn();
		r._Button.StretchCenter();
		return r;
	}

	Button CenterBtn(){
		var r = new Button();
		r.StretchCenter();
		return r;
	}

	GridStack Root = new(IsRow:true);
	/// 渲染頁面主體：分成雲端同步區與本地文件同步區。
	protected nil Render(){
		this.SetContent(Root.Grid);
		Root.A(new StackPanel(), Sp=>{
			Sp.Spacing = UiCfg.Inst.BaseFontSize * 0.5;
			Sp
			.A(MkLocalFileSyncSection())
			.A(MkCloudSyncSection())
			;
		});
		return NIL;
	}

	/// 建立帶標題和邊框的區塊容器。
	/// <param name="Title">區塊標題。</param>
	/// <param name="FnFillBody">填充區塊內容的回調。</param>
	/// <returns>區塊控件。</returns>
	Control MkSection(str Title, Action<StackPanel> FnFillBody){
		var bdr = new Border();
		bdr.BorderBrush = UiCfg.Inst.MainColor;
		bdr.BorderThickness = new(1);
		bdr.Padding = new(
			UiCfg.Inst.BaseFontSize * 0.6,
			UiCfg.Inst.BaseFontSize * 0.5
		);

		var body = new StackPanel();
		body.Spacing = UiCfg.Inst.BaseFontSize * 0.4;
		body.A(new TextBlock(), o=>{
			o.Text = Title;
			o.FontSize = UiCfg.Inst.BaseFontSize * 1.1;
		})
		;
		FnFillBody(body);
		bdr.Child = body;
		return bdr;
	}

	/// 雲端備份同步區：僅包含 Push / Pull。
	/// <returns>區塊控件。</returns>
	Control MkCloudSyncSection(){
		return MkSection(I[K.CloudBackupSync], Body=>{
			Body.A(CenterOpBtn(), o=>{
				o.SetExe((Ct)=>Ctx?.PushAsy(Ct)!);
				o.BtnContent = ToolIcon.IconWithTitle(
					Icons.CloudUpload().ToIcon(),
					I[K.Push]
				);
			})
			.A(CenterOpBtn(), o=>{
				o.SetExe((Ct)=>Ctx?.PullAsy(Ct));
				o.BtnContent = ToolIcon.IconWithTitle(
					Icons.CloudDownload().ToIcon(),
					I[K.Pull]
				);
			})
			;
		});
	}

	/// 本地文件導入導出區：包含路徑輸入、文件選擇與導入導出按鈕。
	/// <returns>區塊控件。</returns>
	Control MkLocalFileSyncSection(){
		return MkSection(I[K.LocalFileImportExport], Body=>{
			Body.A(new TextBlock(), o=>{
				o.Text = I[K.ExportPath];
			})
			.A(MkExportPathRow())
			.A(CenterOpBtn(), o=>{
				o.BtnContent = ToolIcon.IconWithTitle(
					Icons.DatabaseExport().ToIcon(),
					I[K.Export]
				);
				o.SetExe((Ct)=>Ctx?.ExportAsy(Ct));
			})
			.A(new Border(), o=>{
				o.Height = UiCfg.Inst.BaseFontSize * 0.35;
			})
			.A(new TextBlock(), o=>{
				o.Text = I[K.ImportPath];
			})
			.A(MkImportPathRow())
			.A(CenterOpBtn(), o=>{
				o.BtnContent = ToolIcon.IconWithTitle(
					Icons.DatabaseImport().ToIcon(),
					I[K.Import]
				);
				o.SetExe((Ct)=>Ctx?.ImportAsy(Ct));
			})
			;
		});
	}

	Control MkImportPathRow(){
		var row = new GridStack(IsRow: false);
		row.ColDefs.AddRange([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		row.A(new TextBox(), o=>{
			o.CBind<Ctx>(o.PropText, x=>x.PathImport);
		})
		.A(CenterBtn(), o=>{
			o.Content = Icons.FolderOutlinepenOutline();
			o.Click += async (s,e)=>{
				var path = await PickImportPathAsy();
				if(!str.IsNullOrWhiteSpace(path) && Ctx is not null){
					Ctx.PathImport = path;
				}
			};
		});
		return row.Grid;
	}

	Control MkExportPathRow(){
		var row = new GridStack(IsRow: false);
		row.ColDefs.AddRange([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		row.A(new TextBox(), o=>{
			o.CBind<Ctx>(o.PropText, x=>x.PathExport);
		})
		.A(CenterBtn(), o=>{
			o.Content = Icons.FolderOutlinepenOutline();
			o.Click += async (s,e)=>{
				var path = await PickExportPathAsy();
				if(!str.IsNullOrWhiteSpace(path) && Ctx is not null){
					Ctx.PathExport = path;
				}
			};
		});
		return row.Grid;
	}

	/// 打開導入文件選擇器。
	/// <returns>選中文件絕對路徑；取消時返回 null。</returns>
	async Task<str?> PickImportPathAsy(){
		var provider = TopLevel.GetTopLevel(this)?.StorageProvider;
		if(provider is null){
			Ctx?.ShowDialog(I[K.StorageProviderUnavailable]);
			return null;
		}
		var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions{
			Title = I[K.SelectImportFile],
			AllowMultiple = false,
		});
		foreach(var file in files){
			return ToPath(file);
		}
		return null;
	}

	/// 打開導出文件保存選擇器。
	/// <returns>目標文件絕對路徑；取消時返回 null。</returns>
	async Task<str?> PickExportPathAsy(){
		var provider = TopLevel.GetTopLevel(this)?.StorageProvider;
		if(provider is null){
			Ctx?.ShowDialog(I[K.StorageProviderUnavailable]);
			return null;
		}
		var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions{
			Title = I[K.SelectExportFile],
			SuggestedFileName = "words-sync-v2.bin",
		});
		return file is null ? null : ToPath(file);
	}

	/// 把 Avalonia Storage 路徑轉成字符串路徑。
	/// <param name="File">文件句柄。</param>
	/// <returns>可用路徑字符串。</returns>
	static str ToPath(IStorageFile File){
		if(File.Path.IsFile){
			return File.Path.LocalPath;
		}
		return File.Path.AbsolutePath;
	}

	/// <summary>
	/// 頂欄菜單：跳轉到舊版備份同步頁。
	/// </summary>
	/// <returns>標題菜單控件。</returns>
	public Control MkTitleMenu(){
		var menu = new ContextMenu();
		menu.Items.A(new MenuItem(), o=>{
			var title = I[K.GoToLegacyBackupSync];
			o.Header = title;
			o.Click += (s,e)=>{
				ViewNavi?.GoTo(
					ToolView.WithTitle(
						I[K.WordsLibBackupEtSync],
						new ViewWordSync()
					)
				);
			};
		});
		return menu;
	}
}

