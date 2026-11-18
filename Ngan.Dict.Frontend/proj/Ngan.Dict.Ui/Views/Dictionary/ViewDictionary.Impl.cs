namespace Ngan.Dict.Ui.Views.Dictionary;

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Ui.Icons;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Ngan.Dict.Ui.Tools;
using Ngan.Dict.Ui.Views.Dictionary.LlmRawOutputEdit;
using Ngan.Dict.Ui.Views.Dictionary.NormLangTag;
using Ngan.Dict.Ui.Views.Dictionary.NormLangTagEdit;
using Ngan.Dict.Ui.Views.Dictionary.SimpleWord;
using Ngan.Dict.Ui.Views.Word.WordEditV2;
using Ngan.Dict.Ui.Views.Word.WordManage.NormLang.NormLangPage;
using Ngan.Dict.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using DictK = Ngan.Dict.Ui.Infra.I18n.KeysUiI18nCommon;

/// ViewDictionary 的實現。
public partial class ViewDictionary{
	/// View 只能有無參構造器；Vm 在這裏解析。
	public partial ViewDictionary(){
		vm = App.DiOrMk<VmDictionary>();
	}

	/// 四列縱向版面：快捷標籤列／語言列／搜索列／結果區。
	protected override partial object Build(VmDictionary vm){
		Root.Rows("Auto,Auto,Auto,*");
		Root.Children(
			MkSrcLangTagBar(vm)
			,MkLangBar(vm)
			,MkSearchBar(vm)
			,MkResultArea(vm)
		);
		return Root;
	}

	/// 控件樹建好後：掛 Vm 事件與初始資料載入。
	protected override partial void OnAfterInitialized(){
		vm.OnOpenWordEdit = OpenWordEditPage;
		vm.PropertyChanged += OnVmPropertyChanged;
		Fire(vm.InitLang(default));
		Fire(vm.LoadSrcLangTags(default));
	}

	/// 語言列：源語言｜互換｜目標語言。
	protected partial Control MkLangBar(VmDictionary vm){
		var SrcBtn = MkLangButton()
			.Content(vm, x => x.SrcLangDisplay)
		;
		SrcBtn.Click += (_, _) => OpenNormLangSelector(true);
		var TgtBtn = MkLangButton()
			.Content(vm, x => x.TgtLangDisplay)
		;
		TgtBtn.Click += (_, _) => OpenNormLangSelector(false);
		return new StackGrid(IsRow: false)
			.Cols("*,Auto,*")
			.ZIndex(20)
			.Children(SrcBtn, MkLangSwapButton(), TgtBtn)
		;
	}

	/// 搜索列：查詞｜輸入框｜快速保存｜選單。
	protected partial Control MkSearchBar(VmDictionary vm){
		var Input = new TextBox()
			.With(t => {
				_SearchTextBox = t;
				t.Watermark = I[DictK.InputNewWordToSearch];
				t.KeyBindings.Add(new KeyBinding {
					Gesture = new(Key.Enter),
					Command = new RelayCommand(() => _SearchBtn?.PerformClick()),
				});
			})
			.Text(vm, x => x.Input, BindingMode.TwoWay)
		;
		var Search = new OpBtn()
			.With(b => {
				_SearchBtn = b;
				InitToolbarBtn(b, Icons.Search().ToIcon(), Ct => vm.Lookup(Ct), UiCfg.Inst.MainColor);
			})
		;
		var Save = new OpBtn()
			.With(b => {
				_SaveToWordBtn = b;
				ToolTip.SetTip(b, I[DictK.SaveToUserWordLibrary]);
				InitToolbarBtn(b, Icons.BookmarkOutlineAdd().ToIcon(), QuickSaveOrFreeAdd);
			})
		;
		var Menu = new OpBtn()
			.With(b => {
				_MenuBtn = b;
				InitToolbarBtn(b, Icons.Menu().ToIcon());
				b._Button.Click += (_, _) => OpenTitleMenuNear(b._Button);
			})
		;
		return new StackGrid(IsRow: false)
			.Cols("1*,8*,1*,1*")
			.ZIndex(20)
			.Children(Search, Input, Save, Menu)
		;
	}

	/// 左邊可橫向滾動的快捷標籤列，右邊固定的編輯入口。
	protected partial Control MkSrcLangTagBar(VmDictionary vm){
		var Scroll = new ScrollViewer()
			.HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
			.VerticalScrollBarVisibility(ScrollBarVisibility.Disabled)
			.Content(_SrcLangTagList)
		;
		_SrcLangTagScroll = Scroll;
		var Edit = new OpBtn()
			.With(b => {
				_SrcLangTagEditBtn = b;
				b.BtnContent = Icons.Edit().ToIcon();
				b._Button.StretchCenter();
				b.SetExe(_ => {
					OpenSrcLangTagEditor();
					return Task.FromResult<nil>(NIL);
				});
			})
		;
		RebuildSrcLangTags();
		return new StackGrid(IsRow: false)
			.Cols("*,Auto")
			.Children(Scroll, Edit)
		;
	}

	/// 建立單個源語言快捷 Tag，並接通直接切換源語言的事件。
	protected partial Control MkSrcLangTag(VmDictionary vm, VmNormLangTag Tag){
		var View = new ViewNormLangTag();
		View.SetVm(Tag);
		View.OnSelected += X => vm.ApplySrcNormLangTag(X);
		return View;
	}

	/// 按 Vm 當前集合重建橫向快捷 Tag 控件列。
	protected partial void RebuildSrcLangTags(){
		_SrcLangTagList = new StackPanel {
			Orientation = Orientation.Horizontal,
		};
		if(_SrcLangTagScroll is not null){
			_SrcLangTagScroll.Content = _SrcLangTagList;
		}
		foreach(var Tag in vm.SrcLangTags){
			_SrcLangTagList.Children.Add(MkSrcLangTag(vm, Tag));
		}
	}

	/// 打開快捷標籤編輯頁。
	protected partial void OpenSrcLangTagEditor(){
		var View = new ViewNormLangTagEdit();
		View.Vm.FromTags(vm.SrcLangTags);
		View.Vm.SetOnSaved(Tags => {
			_ = vm.ReplaceSrcLangTags(Tags);
		});
		ViewNavi?.GoTo(ToolView.WithTitle(Todo.I18n("Edit source language shortcuts"), View));
	}

	/// 建立共用樣式的語言選擇按鈕。
	protected partial Button MkLangButton(){
		return new Button()
			.HorizontalContentAlignment(HAlign.Center)
			.VerticalContentAlignment(VAlign.Center)
			.MinHeight(24)
			.Padding(new Thickness(0))
		;
	}

	/// 建立交換源語言與目標語言的按鈕。
	protected partial Button MkLangSwapButton(){
		var Btn = new Button()
			.Content("⇄")
			.VerticalAlignment(VAlign.Center)
			.HorizontalAlignment(HAlign.Center)
			.MinHeight(24)
			.Padding(new Thickness(0))
		;
		Btn.Click += (_, _) => vm.SwapLang();
		return Btn;
	}

	/// 初始化工具列按鈕的外觀與命令。
	protected partial nil InitToolbarBtn(OpBtn Btn, Control Content, Func<CT, Task<nil>>? Exe, IBrush? Background){
		Btn._Button.StretchCenter();
		Btn.BtnContent = Content;
		if(Background is not null){
			Btn._Button.Background = Background;
		}
		if(Exe is not null){
			Btn.SetExe(Exe);
		}
		return NIL;
	}

	/// 未查詞時顯示的使用說明。
	protected partial str GuideText(){
		return I[DictK.DictionaryUsageGuide_];
	}

	/// 有結果時快速保存，否則進入自由加詞頁。
	protected async partial Task<nil> QuickSaveOrFreeAdd(CT Ct){
		if(vm.HasLookupStarted){
			if(!vm.CanQuickSaveCurrentLookup || vm.LastReqLlmDict is null || vm.LastRespLlmDict is null){
				vm.ShowToast(I[DictK.CompleteDictionaryQueryBeforeSave]);
				return NIL;
			}
			if(vm.HasQuickSavedCurrentLookup){
				return NIL;
			}
			if(await vm.QuickSaveToWord(Ct)){
				MarkQuickSaveBtnAsSaved();
			}
			return NIL;
		}
		return OpenFreeAddWordPage();
	}

	/// 經單詞編輯頁保存；沒有結果則進自由加詞頁。
	protected async partial Task<nil> OpenSaveViaWordEditOrFreeAdd(CT Ct){
		if(vm.LastReqLlmDict is not null && vm.LastRespLlmDict is not null){
			return await vm.ToWordEdit(Ct);
		}
		return OpenFreeAddWordPage();
	}

	/// 標記快速保存按鈕為已保存。
	protected partial nil MarkQuickSaveBtnAsSaved(){
		if(_SaveToWordBtn is not null){
			_SaveToWordBtn._Button.Background = Brushes.Green;
		}
		return NIL;
	}

	/// 恢復快速保存按鈕的默認樣式。
	protected partial nil ResetQuickSaveBtnToDefaultStyle(){
		if(_SaveToWordBtn is not null){
			_SaveToWordBtn._Button.ClearValue(Button.BackgroundProperty);
			_SaveToWordBtn._Button.ClearValue(Button.BorderBrushProperty);
		}
		return NIL;
	}

	/// 跟隨 Vm 狀態更新按鈕外觀。
	protected partial void OnVmPropertyChanged(object? Sender, PropertyChangedEventArgs E){
		if(E.PropertyName == nameof(VmDictionary.SrcLangTags)){
			RebuildSrcLangTags();
			return;
		}
		if(E.PropertyName != nameof(VmDictionary.HasQuickSavedCurrentLookup)){
			return;
		}
		if(vm.HasQuickSavedCurrentLookup){
			MarkQuickSaveBtnAsSaved();
			return;
		}
		ResetQuickSaveBtnToDefaultStyle();
	}

	/// 結果區：使用提示與簡明查詞結果互斥顯示。
	protected partial Control MkResultArea(VmDictionary vm){
		_ResultView = new ViewSimpleWord();
		_ResultView.SetVm(vm.Result);
		var Area = new Grid()
			.With(g => {
				g.ZIndex = 0;
				g.ClipToBounds = true;
				// 結果區與上方工具欄留固定縫隙，避免邊界像素落到 ScrollViewer 命中區。
				g.Margin = new Thickness(0, UiCfg.Inst.BaseFontSize * 0.35, 0, 0);
			})
			.Children(
				new ScrollViewer()
					.Content(_ResultView)
					.IsVisible(vm, x => x.ShowLookupResult)
				,new TextBlock()
					.With(t => {
						t.Foreground = Brushes.LightGray;
						t.TextWrapping = TextWrapping.Wrap;
						t.VerticalAlignment = VerticalAlignment.Top;
						t.Margin = new Thickness(0, UiCfg.Inst.BaseFontSize * 0.5, 0, 0);
					})
					.IsVisible(vm, x => x.ShowUsageGuide)
			)
		;
		_ResultArea = Area;
		return Area;
	}

	/// 打開源語言／目標語言選擇頁。
	protected partial void OpenNormLangSelector(bool IsSrc){
		var View = new ViewNormLangPage();
		if(View.Ctx is not null){
			View.Ctx.Input = IsSrc ? vm.SrcLang : vm.TgtLang;
		}
		if(IsSrc){
			View.Ctx?.SetSelectMode(Po => {
				vm.ApplySrcNormLang(Po);
				View.ViewNavi?.Back();
			});
		}else{
			View.Ctx?.SetSelectMode(Po => {
				vm.ApplyTgtNormLang(Po);
				View.ViewNavi?.Back();
			});
		}
		ViewNavi?.GoTo(ToolView.WithTitle(
			IsSrc ? I[DictK.SelectDictionarySrcLang] : I[DictK.SelectDictionaryTgtLang],
			View
		));
	}

	/// 建立標題選單。
	public partial Control MkTitleMenu(){
		var Menu = new ContextMenu();
		Menu.Items.Add(MkMenuItem(Todo.I18n("Edit & Save"), () => Fire(OpenSaveViaWordEditOrFreeAdd(default))));
		Menu.Items.Add(MkMenuItem(I[DictK.Help], () => ViewNavi?.GoTo(
			ToolView.WithTitle(I[DictK.Help], new TextBox {
				Classes = { App.Cls.RoTextBox },
				Text = GuideText(),
			})
		)));
		Menu.Items.Add(MkMenuItem(I[DictK.NormLang], () => ViewNavi?.GoTo(
			ToolView.WithTitle(I[DictK.NormLangManage], new ViewNormLangPage())
		)));
		Menu.Items.Add(MkMenuItem(I[DictK.ConfigureLangMapping], () => ViewNavi?.GoTo(
			ToolView.WithTitle(I[DictK.ConfigureLangMapping], new ViewNormLangToUserLangPage())
		)));
		Menu.Items.Add(MkMenuItem(I[DictK.ViewLlmRawOutput], () => {
			var View = new ViewLlmRawOutputEdit();
			View.Vm.SetRawOutput(vm.LastLlmRawOutput);
			View.Vm.SetOnConfirm((Raw, Ct) => vm.ReparseFromRawOutput(Raw, Ct));
			ViewNavi?.GoTo(ToolView.WithTitle(I[DictK.ViewLlmRawOutput], View));
		}));
		return Menu;
	}

	/// 建立帶標題與點擊動作的選單項目。
	protected partial MenuItem MkMenuItem(str Header, Action OnClick){
		var Item = new MenuItem();
		Item.Header = Header;
		Item.Click += (_, _) => OnClick();
		return Item;
	}

	/// 在錨點附近打開標題選單。
	protected partial void OpenTitleMenuNear(Control? Anchor){
		if(Anchor is null){
			return;
		}
		if(MkTitleMenu() is ContextMenu Menu){
			// 顯式傳錨點，避免 ContextMenu 內部取到空 PlacementTarget。
			Menu.Open(Anchor);
		}
	}

	/// 用指定單詞草稿打開合併模式的單詞編輯頁。
	protected partial nil OpenWordEditPage(JnWord JnWord){
		var View = new ViewWordEditV2();
		if(View.Ctx is null){
			vm.ShowDialog(I[DictK.WordEditorContextIsNull]);
			return NIL;
		}
		View.Ctx.FromJnWord(JnWord);
		View.Ctx.SaveMode = VmWordEditV2.ESaveMode.Merge;
		var Title = str.IsNullOrWhiteSpace(JnWord.Word.Head) ? I[DictK.WordEdit] : JnWord.Word.Head;
		ViewNavi?.GoTo(ToolView.WithTitle(Title, View));
		return NIL;
	}

	/// 打開預填源語言的空白加詞頁。
	protected partial nil OpenFreeAddWordPage(){
		var View = new ViewWordEditV2();
		if(View.Ctx is null){
			vm.ShowDialog(I[DictK.WordEditorContextIsNull]);
			return NIL;
		}
		View.Ctx.InitFreeAddDraft(vm.SrcLang);
		ViewNavi?.GoTo(ToolView.WithTitle(I[DictK.AddWords], View));
		return NIL;
	}

	/// 可選填入搜索文本後觸發查詞。
	public partial void ClickLookupBtn(str? SearchText){
		if(SearchText is not null && _SearchTextBox is not null){
			_SearchTextBox.Text = SearchText;
		}
		_SearchBtn?.PerformClick();
	}
}
