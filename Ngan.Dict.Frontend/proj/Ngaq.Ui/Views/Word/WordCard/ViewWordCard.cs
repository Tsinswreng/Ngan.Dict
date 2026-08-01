namespace Ngaq.Ui.Views.Word.WordCard;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Ui.Converters;
using Ngaq.Ui.StrokeText;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.Avln.StrokeText;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Views.Word.WordEditV2;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangPage;
using Tsinswreng.AvlnTools.Controls;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Tsinswreng.Avln.Grid;

using Ctx = VmWordListCard;
public partial class ViewWordListCard
	:AppViewBase<Ctx>
	,IViewWordListCard
{
	public StrokeTextBlock? IndexTextCtrl;
	public StrokeTextBlock? LangTextCtrl;
	public StrokeTextBlock? LearnHistoryTextCtrl;
	public StrokeTextBlock? LastLearnedTimeTextCtrl;
	public StrokeTextBlock? WeightTextCtrl;
	public StrokeTextBlock? HeadTextCtrl;
	public SwipeLongPressBtn? HostClickBtn{get;set;}

	public str? IndexText{
		get{return IndexTextCtrl?.Text;}
	}

	public str? LangText{
		get{return LangTextCtrl?.Text;}
	}

	public str? HeadText{
		get{return HeadTextCtrl?.Text;}
	}

	public str? LearnHistoryText{
		get{return LearnHistoryTextCtrl?.Text;}
	}

	public str? LastLearnedTimeText{
		get{return LastLearnedTimeTextCtrl?.Text;}
	}

	public str? WeightText{
		get{return WeightTextCtrl?.Text;}
	}

	public IBrush? HeadFontColor{
		get{return HeadTextCtrl?.Foreground;}
	}

	public IBrush? LearnedColor{
		get{return HostClickBtn?.BorderBrush;}
	}

	public Task<nil> Click(CT Ct){
		HostClickBtn?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
		return Task.FromResult<nil>(NIL);
	}

	public static ContextMenu MkWordCardCtxMenu(
		ViewModelBase? Ctx
		,IJnWord? JnWord
	){
		var R = new ContextMenu();
		R.Items.A(new MenuItem(), o=>{

			o.Header = Icons.Edit().WithText(AppI18n.Inst[K.Edit]);
			o.Click += (s,e)=>{
				if(AnyNull(JnWord)){
					MainView.Inst.ShowDialog(AppI18n.Inst[K.NoWordSelected]);
					return;
				}
				var editView = new ViewWordEditV2();
				if(editView.Ctx is null){
					return;
				}
				editView.Ctx.FromJnWord(JnWord);
				MgrViewNavi.Inst.ViewNavi?.GoTo(ToolView.WithTitle(JnWord.Head, editView));
			};
		})
		.A(new MenuItem(), o=>{
			o.Header = Icons.VolHigh().WithText(AppI18n.Inst[K.Pronounce]);
			o.Click += async(s,e)=>{
				if(Ctx is IWordCardMenuAction Action){
					var R = await Action.PronounceWord(JnWord, default);
					HandlePronounceResult(Ctx, R);
					return;
				}
				MainView.Inst.ShowDialog(AppI18n.Inst[K.CurrentPageNoPronounceAction]);
			};
		});
		return R;
	}

	public static nil HandlePronounceResult(
		ViewModelBase? Ctx,
		DtoWordCardPronounceResult? R
	){
		if(R is null){
			MainView.Inst.ShowDialog(AppI18n.Inst[K.PronounceFailed]);
			return NIL;
		}
		if(R.Status == EWordCardPronounceStatus.Played){
			return NIL;
		}
		if(R.Status == EWordCardPronounceStatus.UserLangNotMapped){
			var BtnGoCfg = new Button{
				Content = AppI18n.Inst[K.GoConfigureUserLang],
			};
			BtnGoCfg.Click += (bs,be)=>{
				var View = new ViewUserLangPage();
				if(View.Ctx is not null){
					View.Ctx.Input = R.WordLang;
					_ = View.Ctx.InitSearch(default);
				}
				MgrViewNavi.Inst.ViewNavi?.GoTo(ToolView.WithTitle(AppI18n.Inst[K.UserLang], View));
			};
			MainView.Inst.ShowDialog(
				AppI18n.Inst[K.WordLangNotMappedCannotPronounce],
				[BtnGoCfg]
			);
			return NIL;
		}
		if(R.Status == EWordCardPronounceStatus.NoWordSelected){
			MainView.Inst.ShowDialog(AppI18n.Inst[K.NoWordSelected]);
			return NIL;
		}
		if(R.Status == EWordCardPronounceStatus.WordLangEmpty){
			MainView.Inst.ShowDialog(AppI18n.Inst[K.WordLangIsEmpty]);
			return NIL;
		}
		if(R.Status == EWordCardPronounceStatus.ServiceUnavailable){
			MainView.Inst.ShowDialog(AppI18n.Inst[K.ServiceUnavailable]);
			return NIL;
		}
		if(R.Status == EWordCardPronounceStatus.Failed){
			if(R.Error is not null){
				Ctx?.HandleErr(R.Error);
			}else{
				MainView.Inst.ShowDialog(AppI18n.Inst[K.PronounceFailed]);
			}
			return NIL;
		}
		return NIL;
	}

	public ViewWordListCard(Ctx Ctx){
		this.Ctx = Ctx;
		Style();
		Render();
	}

	public ViewWordListCard(){
		Ctx = new Ctx();
		Style();
		Render();
	}

	protected StrokeTextBlock TxtBox(){
		var R = new StrokeTextBlock{
			Fill = Brushes.White,
			Stroke = Brushes.Black,
			StrokeThickness = 5
		};
		R.VerticalAlignment = VAlign.Center;
		R.Styles.Add(new Style().NoMargin().NoPadding());
		return R;
	}

	public partial class Cls{
		public const str InInfoGrid = nameof(InInfoGrid);
	}

	public GridStack Root{get;set;} = new GridStack(IsRow:true);

	protected nil Style(){
		//Styles.Add(SugarStyle.GridShowLines());
		Styles.A(
			Sty.Is<TextBlock>()
			.Set(
				x=>x.Effect
				,new DropShadowDirectionEffect{
					Color = Colors.Black
					,BlurRadius = 4
					,ShadowDepth = 4
					//,Direction = 315 //高度 315是左上角
					,Direction = 330
					,Opacity = 0.5
				}
			)
		);
		return NIL;
	}

	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(4, GUT.Auto),
				new(8, GUT.Auto),
			]);
		});

		var LangGrid = new GridStack(IsRow:false);
		Root.A(LangGrid.Grid, o=>{
			o.SetColDefs([
				new(1, GUT.Star),
				new(0.3, GUT.Star),
				new(4, GUT.Star),
				new(13, GUT.Star),
			]);
			LangGrid.A(TxtBox(), o=>{
				IndexTextCtrl = o;
				o.FontSize = UiCfg.Inst.BaseFontSize*0.8;
				Ctx.Bind(o, o=>o.Text,x=>x.Index);
			}).A(TxtBox(), o=>{
				o.Text = "　";
			}).A(TxtBox(), o=>{
				LangTextCtrl = o;
				o.VerticalAlignment = VAlign.Center;
				Ctx.Bind(o, o=>o.Text, x=>x.Lang);
				o.Foreground = Brushes.LightGray;
			}).A(_InfoGrid());
		});

		var HeadBox = new GridStack(IsRow:false);
		Root.A(HeadBox.Grid, o=>{
			o.SetColDefs([
				new(1, GUT.Star),
			]);
			HeadBox.A(TxtBox(), o=>{
				HeadTextCtrl = o;
				o.VerticalAlignment = VAlign.Center;
				o.FontSize = UiCfg.Inst.BaseFontSize*1.2;
				Ctx.Bind(o, o=>o.Text, x=>x.Head);
				Ctx.Bind(o, o=>o.Foreground,x=>x.FontColor);
			});
		});

		return NIL;
	}

	Control _InfoGrid(){
		var R = new GridStack(IsRow:false);
		{var o = R.Grid;
			o.Classes.Add(Cls.InInfoGrid);
			o.SetColDefs([
				new(9, GUT.Star),
				new(3, GUT.Star),//last review time
				new(1, GUT.Star),//tab
				new(7, GUT.Star),//weight
			]);
		}
		{{
			var RecordType = (ELearn Learn)=>{
				var R = new TextBlock{};
				Ctx.Bind(
					R,R=>R.Text
					,x=>x.Learn_Records
					,Converter: new ConvMultiDictValueCnt<ELearn, ILearnRecord>()
					,ConverterParameter: Learn
				);
				return R;
			};
			var Colon = ()=>new TextBlock(){Text = ":"};

			R.A(TxtBox(), o=>{
				LearnHistoryTextCtrl = o;
				Ctx.Bind(
					o, o=>o.Text,x=>x
					,Converter: new FnConvtr<Ctx?, str>((x)=>x?.ToLearnHistoryRepr()??"")
				);
			});

			//LastReviewTime
			R.A(TxtBox(), o=>{
				LastLearnedTimeTextCtrl = o;
				Ctx.Bind(
					o,o=>o.Text,x=>x.LastLearnedTime
					,Converter: new FnConvtr<i64, str>(x=>{
						var Now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
						var Diff = Now - x;
						return Ctx.FormatUnixMsDiff(Diff);
					})
				);
			})
			.A(new TextBlock{Text = "\t"})
			.A(TxtBox(), o=>{
				WeightTextCtrl = o;
				Ctx.Bind(
					o,o=>o.Text,x=>x.Weight
					,Converter: new FnConvtr<f64?,str>((x,p)=>
						Ctx.FmtNum(x??0, 1)
					)
				);
			});//Weight
		}}

		return R.Grid;
	}
}




