namespace Ngaq.Ui.Views.Word.WordManage.SearchWords;

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordCard;
using Ngaq.Ui.Views.Word.WordEditV2;
using Ngaq.Ui.Views.Word.WordManage.AddWord;
using Ngaq.Ui.Views.Word.WordManage.SearchWords.SearchedWordCard;
using Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangPage;
using Ngaq.Ui.Views.Word.WordManage.WordSyncV2;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsTools;
using Ctx = VmSearchWords;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewSearchWords{
	public partial ViewSearchWords(){
		Ctx = App.GetRSvc<Ctx>();
		InitStyle();
		Render();
	}
	partial void InitStyle(){}
	partial void Render(){
		this.SetContent(Root.Grid);
		Root.SetRowDefs([new(1, GUT.Auto), new(1, GUT.Star), new(1, GUT.Auto)]);
		SearchGrid.Grid.SetColDefs([new(6, GUT.Auto), new(1, GUT.Star), new(1, GUT.Auto)]);
		Root.A(SearchGrid.Grid);
		SearchGrid.A(new OpBtn(), o=>{
			SearchBtn = o;
			o.BtnContent = Icons.Search().ToIcon();
			o.SetExe(Ct=>Ctx?.InitSearch(Ct));
			o._Button.StretchCenter();
			o._Button.Background = UiCfg.Inst.MainColor;
		}).A(new TextBox(), o=>{
			SearchInputCtrl = o;
			o.Watermark = I[K.SearchUserWords];
			o.CBind<Ctx>(o.PropText, x=>x.Input);
			o.KeyBindings.Add(new KeyBinding{Gesture = new(Key.Enter), Command = new RelayCommand(()=>SearchBtn?.PerformClick())});
		}).A(new OpBtn(), o=>{
			FreeAddBtn = o;
			o.BtnContent = Icons.Add().ToIcon();
			o.SetExe(Ct=>Task.FromResult(OpenFreeAddWordPage()));
			o._Button.StretchCenter();
		});
		Root.A(new ScrollViewer(), o=>{
			WordListScroll = o;
			o.SetContent(MkWordList());
		}).A(MkPageBar(), o=>o.HorizontalAlignment = HAlign.Center);
	}
	private partial Control MkWordList(){
		var Result = new ItemsControl();
		WordListCtrl = Result;
		Result.SetItemsPanel(()=>new VirtualizingStackPanel());
		Result.SetItemTemplate<DtoWordSearchHit>((Hit, _)=>{
			var Button = new Button();
			var Card = new ViewSearchedWordCard();
			if(Hit is null){return Card;}
			Card.Ctx?.FromSearchHit(Hit);
			Button.SetContent(Card);
			var JnWord = VmSearchedWordCard.GetJnWordFromHit(Hit);
			if(JnWord is not null){Button.ContextMenu = ViewWordListCard.MkWordCardCtxMenu(Ctx, JnWord);}
			Button.Click += (_, _)=>OpenWordEditor(Hit);
			Button.Styles.A(new Style().Set(BackgroundProperty, Brushes.Transparent)).A(new Style().NoMargin().NoPadding());
			return Button;
		});
		Result.CBind<Ctx>(Result.PropItemsSource, x=>x.GotWords);
		return Result;
	}
	private partial void OpenWordEditor(DtoWordSearchHit Hit){
		var Target = new ViewWordEditV2();
		var JnWord = VmSearchedWordCard.GetJnWordFromHit(Hit);
		if(AnyNull(Target.Ctx, JnWord)){return;}
		Target.Ctx.FromJnWord(JnWord);
		ViewNavi?.GoTo(ToolView.WithTitle(JnWord.Head, Target));
	}
	private partial Control MkPageBar(){
		var Result = new ViewPageBar();
		PageBarView = Result;
		if(Ctx is not null){Result.Ctx = Ctx.PageBar;}
		return Result;
	}
	public partial Control MkTitleMenu(){
		var Menu = new ContextMenu();
		Menu.Items.A(MkMenuItem(I[K.AddWordsFromText], ()=>new ViewAddWord()))
			.A(MkMenuItem(I[K.UserLang], ()=>new ViewUserLangPage()))
			.A(MkMenuItem(I[K.WordsLibBackupEtSync], ()=>new ViewWordSyncV2()));
		return Menu;
	}
	private partial MenuItem MkMenuItem(str Title, Func<Control> MkView){
		var Result = new MenuItem{Header = Title};
		Result.Click += (_, _)=>ViewNavi?.GoTo(ToolView.WithTitle(Title, MkView()));
		return Result;
	}
	private partial nil OpenFreeAddWordPage(){
		var View = new ViewWordEditV2();
		if(View.Ctx is null){Ctx?.ShowDialog(I[K.WordEditorContextIsNull]); return NIL;}
		View.Ctx.InitFreeAddDraft();
		ViewNavi?.GoTo(ToolView.WithTitle(I[K.AddWords], View));
		return NIL;
	}
}
