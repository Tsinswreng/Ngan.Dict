namespace Ngan.Dict.Ui.Views.Dictionary;

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
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
		// 臨時診斷：把各層寬度寫進日誌，確認是「頁面比視窗寬」還是「按鈕沒排上」。
		vm.OnOpenWordEdit = OpenWordEditPage;
		vm.PropertyChanged += OnVmPropertyChanged;
		Fire(vm.InitLang(default));
		Fire(vm.LoadSrcLangTags(default));
	}

	/// 語言列：源語言 → 目標語言，靠左緊湊排列。
	/// *為何不再左右拉開*：拉開之後三塊各自貼邊，看不出「這一組是語言設定」；
	/// 靠左連成一串、中間配一個小箭頭，讀起來就是「從 A 到 B」。
	protected partial Control MkLangBar(VmDictionary vm){
		var SrcBtn = MkLangButton()
			.Content(vm, x => x.SrcLangDisplay)
		;
		SrcBtn.Click += (_, _) => OpenNormLangSelector(true);
		var TgtBtn = MkLangButton()
			.Content(vm, x => x.TgtLangDisplay)
		;
		TgtBtn.Click += (_, _) => OpenNormLangSelector(false);
		var Row = new StackPanel{
			Orientation = Orientation.Horizontal,
			Spacing = UiCfg.Inst.GapItem,
			HorizontalAlignment = HAlign.Left,
			Margin = new Thickness(
				UiCfg.Inst.PagePaddingH
				,UiCfg.Inst.GapItem
				,UiCfg.Inst.PagePaddingH
				,0
			),
		};
		Row.ZIndex = 20;
		Row.Children(SrcBtn, MkLangSwapButton(), TgtBtn);
		return Row;
	}

	/// 搜索列：查詞｜輸入框｜複製｜快速保存｜選單。
	/// 輸入框支援多行（貼整段翻譯），但送出仍是 Enter：Enter 查詞、Shift+Enter 換行。
	/// *為何用原生 Grid 並顯式指定列號*：本頁的 StackGrid 在該處只把前兩個子項排進列，
	/// 其後（保存／選單）實測不渲染；此處需要穩定的多列排版，故退回首選方案之外的做法。
	protected partial Control MkSearchBar(VmDictionary vm){
		var Input = new TextBox()
			.With(t => {
				_SearchTextBox = t;
				t.Watermark = I[DictK.InputNewWordToSearch];
				t.AcceptsReturn = true;
				t.TextWrapping = TextWrapping.Wrap;
				t.MinHeight = UiCfg.Inst.BaseFontSize * 1.8;
				t.MaxHeight = UiCfg.Inst.BaseFontSize * 7;
				// 底線式輸入框：去掉外框與底色，只留一條下劃線。
				// 默認的方框輸入框和旁邊三個按鈕並排時是四個盒子，這是這一行最醜的地方。
				t.Background = Brushes.Transparent;
				t.BorderThickness = new Thickness(0, 0, 0, 1);
				t.BorderBrush = UiCfg.Inst.FaintTextBrush;
				t.Padding = new Thickness(0, 2, 0, 4);
				t.FontSize = UiCfg.Inst.BaseFontSize;
				t.Margin = new Thickness(0, 0, UiCfg.Inst.GapItem, 0);
				// 用「隧道」階段攔 Enter：TextBox 自己會在 KeyDown 的類處理器裏把 Enter 當換行插入，
				// 掛普通 KeyDown（冒泡）攔不住。隧道先於類處理器，故能可靠地把它變成「送出」。
				t.AddHandler(
					InputElement.KeyDownEvent
					,(s, e) => {
						// Shift+Enter 不攔，交給 TextBox 插入換行。
						if(e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)){
							e.Handled = true;
							_SearchBtn?.Command?.Execute(null);_SearchBtn?.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
						}
					}
					,RoutingStrategies.Tunnel
				);
			})
			.Text(vm, x => x.Input, BindingMode.TwoWay)
		;
		var Search = MkBarBtn(Icons.Search().ToIcon(), () => Fire(vm.Lookup(default)), UiCfg.Inst.MainColor);
		_SearchBtn = Search;
		var Save = MkBarBtn(Icons.BookmarkOutlineAdd().ToIcon(), () => Fire(QuickSaveOrFreeAdd(default)));
		ToolTip.SetTip(Save, I[DictK.SaveToUserWordLibrary]);
		_SaveToWordBtn = Save;
		var Menu = MkBarBtn(Icons.Menu().ToIcon(), null);
		Menu.Click += (_, _) => OpenTitleMenuNear(Menu);
		_MenuBtn = Menu;
		// 按鈕在左、輸入框吃剩餘寬度。
		// *為何用 DockPanel 而不是 StackGrid*：本頁的 StackGrid 在「自適應列之後還有 Auto 列」時，
		// 那些後續列的按鈕不會被排版（實測整顆消失）；DockPanel 逐個停靠、沒有行列號，
		// 對「固定寬按鈕 ＋ 自適應輸入框」這種一維排布反而更穩、也更簡單。
		var Bar = new DockPanel{
			LastChildFill = true,
			Margin = new Thickness(
				UiCfg.Inst.PagePaddingH
				,UiCfg.Inst.GapItem
				,UiCfg.Inst.PagePaddingH
				,UiCfg.Inst.GapItem
			),
		};
		Bar.ZIndex = 20;
		Bar.Children(Search, Save, Menu, Input);
		return Bar;
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
		// 小字、次級色、無底無邊：語言設定是次要操作，不該比搜索框還重。
		return new Button()
			.HorizontalContentAlignment(HAlign.Center)
			.VerticalContentAlignment(VAlign.Center)
			.MinHeight(22)
			.Padding(new Thickness(UiCfg.Inst.GapItem*0.6, 0))
			.Background(Brushes.Transparent)
			.BorderThickness(new Thickness(0))
			.FontSize(UiCfg.Inst.BaseFontSize*0.85)
			.Foreground(UiCfg.Inst.MutedTextBrush)
		;
	}

	/// 建立交換源語言與目標語言的按鈕。
	/// 用文字箭頭而不是圖標按鈕：它只是「A → B」這個短語裏的一個符號，不該有按鈕的重量。
	protected partial Button MkLangSwapButton(){
		var Btn = new Button()
			.Content("→")
			.Background(Brushes.Transparent)
			.BorderThickness(new Thickness(0))
			.Foreground(UiCfg.Inst.FaintTextBrush)
			.FontSize(UiCfg.Inst.BaseFontSize*0.85)
			.VerticalAlignment(VAlign.Center)
			.HorizontalAlignment(HAlign.Center)
			.MinHeight(22)
			.Padding(new Thickness(UiCfg.Inst.GapItem*0.4, 0))
		;
		Btn.Click += (_, _) => vm.SwapLang();
		return Btn;
	}

	/// 建立搜索列上的圖標按鈕。
	/// *為何不用 `OpBtn`*：`OpBtn` 的內容是「Grid ＋ Button」的組合，
	/// 在本頁的 DockPanel／Auto 度量下會收斂成零尺寸而整顆消失；原生 Button 度量穩定。
	/// 需要「執行中可取消」的只有查詞按鈕，其餘兩個本來就沒有取消語義。
	protected partial Button MkBarBtn(Control Icon, Action? OnClick, IBrush? Background = null){
		var Size = UiCfg.Inst.BaseFontSize * 1.15;
		Icon.Width = Size;
		Icon.Height = Size;
		var Btn = new Button{
			Content = Icon,
			Width = UiCfg.Inst.BaseFontSize * 1.8,
			Height = UiCfg.Inst.BaseFontSize * 1.6,
			Padding = new Thickness(0),
			Background = Brushes.Transparent,
			BorderThickness = new Thickness(0),
			HorizontalContentAlignment = HAlign.Center,
			VerticalContentAlignment = VAlign.Center,
		};
		if(Background is not null){
			Btn.Background = Background;
		}
		if(OnClick is not null){
			Btn.Click += (_, _) => OnClick();
		}
		return Btn;
	}

	/// 初始化工具列按鈕的外觀與命令。
	/// *為何要顯式給圖標尺寸*：`Icon` 自身沒有內在大小，靠父容器約束；
	/// 在 Auto 列裏沒有約束來源，會被量成零尺寸，按鈕隨之消失（保存／選單／複製都踩過）。
	protected partial nil InitToolbarBtn(OpBtn Btn, Control Content, Func<CT, Task<nil>>? Exe, IBrush? Background){
		Btn._Button.StretchCenter();
		var IconSize = UiCfg.Inst.BaseFontSize * 1.15;
		Content.Width = IconSize;
		Content.Height = IconSize;
		// 按鈕自身也要有明確尺寸：OpBtn 的內層是「Grid ＋ Button」，
		// 在 Auto 列（按內容量測）裏量不出期望尺寸時會塌成 0，按鈕整體不顯示。
		var BtnSize = UiCfg.Inst.BaseFontSize * 2;
		Btn.Width = BtnSize;
		Btn.Height = BtnSize;
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
			_SaveToWordBtn.Background = Brushes.Green;
		}
		return NIL;
	}

	/// 恢復快速保存按鈕的默認樣式。
	protected partial nil ResetQuickSaveBtnToDefaultStyle(){
		if(_SaveToWordBtn is not null){
			_SaveToWordBtn.ClearValue(Button.BackgroundProperty);
			_SaveToWordBtn.ClearValue(Button.BorderBrushProperty);
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
		_ResultView.HorizontalAlignment = HAlign.Stretch;
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
					// 關掉橫向滾動：結果卡的寬度必須等於視口寬度。
					// 否則 ScrollViewer 會以無限寬度量測內容，卡片被撐到比視窗還寬：
					// 文字右端被切掉、右上角的複製按鈕也會被推到視口外看不見。
					.With(o => {
						o.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
						o.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
					})
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
				// 翻譯路徑沒有可讀的串流文本（串流內容是 JSON 片段），故另外給一個生成中提示。
				,new TextBlock()
					.With(t => {
						t.Text = Todo.I18n("Translating…");
						t.Foreground = Brushes.Gray;
						t.VerticalAlignment = VerticalAlignment.Top;
						t.Margin = new Thickness(0, UiCfg.Inst.BaseFontSize * 0.5, 0, 0);
					})
					.IsVisible(vm, x => x.ShowTranslating)
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
		// 複製放在選單裏：工具列已經有查詞／保存／選單三個按鈕，
		// 再塞一個會擠掉右側，而複製不是每輪都要按的動作。
		Menu.Items.Add(MkMenuItem(I[DictK.CopyResult], () => Fire(vm.CopyResult(default))));
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
		_SearchBtn?.Command?.Execute(null);_SearchBtn?.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
	}
}
