namespace Ngaq.Ui.Views.Word.Learn;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Settings.LearnWord;
using Ngaq.Ui.Views.Word.WordCard;
using Ngaq.Ui.Views.Word.WordInfo;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.StrokeText;
using Tsinswreng.AvlnTools.Controls;
using Tsinswreng.AvlnTools.Dsl;
using Ctx = VmLearnWords;
using K = Infra.I18n.KeysUiI18nCommon;
using PC = PsdCls;

public partial class ViewLearnWords
	:AppViewBase<Ctx>
{
	public OpBtn? StartBtn{get;set;}
	public OpBtn? SaveBtn{get;set;}
	public OpBtn? ResetBtn{get;set;}
	public OpBtn? SettingsBtn{get;set;}
	public ItemsControl? WordListItemsCtrl{get;set;}
	public ViewWordInfo? WordInfoCtrl{get;set;}
	public ObservableCollection<IViewWordListCard> WordCardCtrls{get;set;} = [];
	protected Ctx? SubscribedCtx;
	protected ObservableCollection<VmWordListCard>? SubscribedVmWordCards;


	public IList<IViewWordListCard>? WordListCards{
		get=>WordCardCtrls;
	}

	public ViewLearnWords(){
		Ctx = App.GetRSvc<Ctx>();
		if(Ctx is not null){
			Ctx.OnAutoPronounceResult += OnAutoPronounceResult;
		}
		Style();
		Menu = _Menu();
		Render();
		DataContextChanged += (s,e)=>{
			OnCtxChanged();
		};
		Loaded += (s,e)=>{
			_OnLoad();
		};

		OnCtxChanged();
		InitIView();
	}

	//來自IView的成員初始化都寫在此
	void InitIView(){
		WordCardCtrls.CollectionChanged += (s,e)=>{
			RaisePropertyChanged(nameof(WordListCards));
		};
		WordInfoCtrl?.PropertyChanged += (s,e)=>{
			if(
				e.PropertyName == nameof(VmWordInfo.Head)
				|| e.PropertyName == nameof(VmWordInfo.Descrs)
			){
				RaisePropertyChanged(nameof(WordInfo));
			}
		};
	}


	public GridStack Root = new GridStack(IsRow: true);
	Panel Menu;


	void OnAutoPronounceResult(DtoWordCardPronounceResult Result){
		ViewWordListCard.HandlePronounceResult(Ctx, Result);
	}

	protected nil OnCtxChanged(){
		if(SubscribedCtx is not null){
			SubscribedCtx.PropertyChanged -= OnCtxPropertyChanged;
			UnsubscribeVmWordCards(SubscribedVmWordCards);
			SubscribedCtx = null;
			SubscribedVmWordCards = null;
		}
		if(Ctx is not null){
			SubscribedCtx = Ctx;
			SubscribedCtx.PropertyChanged += OnCtxPropertyChanged;
			SubscribeVmWordCards(Ctx.WordCards);
		}
		return NIL;
	}

	protected void OnCtxPropertyChanged(object? Sender, PropertyChangedEventArgs E){
		if(E.PropertyName == nameof(Ctx.WordCards) && Ctx is not null){
			UnsubscribeVmWordCards(SubscribedVmWordCards);
			SubscribeVmWordCards(Ctx.WordCards);
		}
	}

	protected nil SubscribeVmWordCards(ObservableCollection<VmWordListCard>? WordCards){
		SubscribedVmWordCards = WordCards;
		if(SubscribedVmWordCards is not null){
			SubscribedVmWordCards.CollectionChanged += OnVmWordCardsChanged;
		}
		return NIL;
	}

	protected nil UnsubscribeVmWordCards(ObservableCollection<VmWordListCard>? WordCards){
		if(WordCards is not null){
			WordCards.CollectionChanged -= OnVmWordCardsChanged;
		}
		return NIL;
	}

	protected void OnVmWordCardsChanged(object? Sender, NotifyCollectionChangedEventArgs E){
		if(E.Action == NotifyCollectionChangedAction.Reset){
			WordCardCtrls.Clear();
		}
	}





	public partial class Cls{
		public const str MenuBtn = nameof(MenuBtn);
	}


	protected nil Style(){
		Styles.A(Sty.Is<Control>(x=>
			x.Class(Cls.MenuBtn)
		).Set(
			x=>x.VerticalAlignment
			, VAlign.Stretch
		));
		return NIL;
	}

	protected IBrush Shade(IBrush originalBrush, ContentControl top){
		var overlayBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)); // 半透明黑，alpha可调
		var overlayGrid = new Grid{
			Width = top.Bounds.Width,
			Height = top.Bounds.Height,
			Children ={
				new Border { Background = originalBrush },
				new Border { Background = overlayBrush }
			}
		};
		// ?VisualBrush 包裹 overlayGrid
		var combinedBrush = new VisualBrush{
			Visual = overlayGrid,
			Stretch = Stretch.Fill
		};
		return combinedBrush;

	}

	protected nil _OnLoad(){
		var t = TopLevel.GetTopLevel(this);
		if(t == null){return NIL;}
		Ctx.Bind(
			t, t=>t.Background, x=>x.BgBrush, Source: Ctx//必須寫Source:、因是給top綁定、其本ʹDataContext非this.Ctx
			,Converter: new FnConvtr<IBrush, nil>((oldBrush, arg)=>{
				return Shade(oldBrush, t);
			})
		);
		t.GetObservable(TopLevel.BoundsProperty).Subscribe(_ =>{
			// 触发 Background 的刷新或重设，保证ImageBrush用UniformToFill渲染
			if(Ctx is null){return;}
			if(t.Background is TileBrush tb){
				//TODO 改成可配置的
				tb.Stretch = Stretch.UniformToFill;
			}
			//Background = Shade(Ctx.BgBrush, top);
			//top.Background.Stretch = Stretch.Uniform;
		});
		return NIL;
	}

	StrokeTextBlock _Txt(){
		var R = new StrokeTextBlock();
		R.Foreground = Brushes.White;
		return R;
	}

	protected Panel _Menu(){
		var R = new GridStack(IsRow: true);
		var Row1 = new GridStack(IsRow: false);
		R.A(Row1.Grid, (r)=>{
			r.SetColDefs([
				new(100, GUT.Star),
				new(100, GUT.Star),
				new(100, GUT.Star),
				new(100, GUT.Star),
			]);
			var T = (str t)=>{
				var R = new StrokeTextBlock{
					Text = " "+t,
					FontSize = UiCfg.Inst.BaseFontSize*0.8,
					StrokeThickness = 2.0,
				};
				return R;
			};
			var Hc = (params Control[] Ctrls)=>{
				return HoriCloseCtrls.Mk(Ctrls);
			};
			var Btn = ()=>{
				var R = new OpBtn{};
				R.Classes.Add(Cls.MenuBtn);
				R._Button.VerticalAlignment = VAlign.Stretch;
				R._Button.VerticalContentAlignment = VAlign.Stretch;
				R._Button.StretchCenter();
				R._Button.Background = Brushes.Transparent;
				R.Padding = R._Button.Margin = new(0);
				return R;
			};
			Row1.A(Btn(), o=>{
				StartBtn = o;
				o._Button.SetContent(Hc(
					Icons.StartLearn()//▶️
					,T(I[K.Start])
				));
				o.SetExe((Ct)=>Ctx?.LoadEtStartAsy(Ct));
			}).A(Btn(), o=>{ //📁 💾
				SaveBtn = o;
				o._Button.SetContent(Hc(
					Icons.Save()
					,T(I[K.Save])
				));
				o.SetExe((Ct)=>Ctx?.SaveEtRestart(Ct));
 				o._Button.Styles.Add(
					Sty.Is<Button>(
						//x=>x.OfType<Button>().Template().OfType<ContentPresenter>() //改僞類樣式旹纔需?
						x=>x
					).Set(
						x=>x.Background
						,CBE.Mk<Ctx>(
							x=>x.IsSaved
							,Converter: new FnConvtr<bool, IBrush?>((b)=>{
								if(!b){
									return UiCfg.Inst.MainColor;
								}
								return o._Button.Background;
							})
						)
					)
				);
			}).A(Btn(), o=>{
				ResetBtn = o;
				o._Button.SetContent(Hc(//🔄
					Icons.ResetLearnStatus()
					,T(I[K.Reset])
				));
				o.SetExe((Ct)=>Ctx?.Reset(Ct));
			}).A(Btn(), o=>{
				SettingsBtn = o;
				o._Button.SetContent(Hc(//⚙️
					Icons.Setting()
					,T(I[K.Settings])
				));
				o._Button.Click += (s,e)=>{
					ViewNavi?.GoTo(
						ToolView.WithTitle(
							I[K.LearnWordSettings]
							,new ViewCfgLearnWord()
						)
					);
				};
			})
			;
		});

		return R.Grid;
	}


	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(1, GUT.Auto),//overlay
				new(1, GUT.Auto),
				new(4, GUT.Star),
				new(1, GUT.Auto),//GridSplitter
				new(5, GUT.Star),
			]);
		});
		Root
		.A(new Border(), o=>{
			//背景圖遮蔽
			o.Background = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.35), 0, 0, 0));
			o.ZIndex = -1;
		}).A(_Menu())
		.A(new ScrollViewer(), Scr=>{
			Scr.SetContent(_ListWordCard(), o=>{
				Ctx.Bind(o,
					ItemsControl.ItemsSourceProperty
					,x=>x.WordCards, Mode: BindingMode.TwoWay
				);
			});
		}).A(new GridSplitter(), o=>{
			o.GrayBarWith3Dots();
		}).A(_WordInfo(), o=>{
			WordInfoCtrl = o;
			Ctx.Bind(o, o.PropDataContext,x=>x.CurWordInfo, Mode: BindingMode.TwoWay);
		});
		return NIL;
	}

	//TODO 分頁加載以代虛擬化
	Control _ListWordCard(){
		var Ic = new ItemsControl();
		WordListItemsCtrl = Ic;
		Ic.SetItemsPanel(()=>{
			return new VirtualizingStackPanel();
		});
		Ic.SetItemTemplate<VmWordListCard>((VmWordCard,b)=>{
			var Grid = new Grid();
			Grid.SetRowDefs([
				new(1,GUT.Auto)
			]);
			Grid.A(new SwipeLongPressBtn(), b=>{
				b.HCAlign(x=>x.Stretch);
				b.Styles.Add(new Style().NoMargin().NoPadding());
				b.Background = Brushes.Transparent;
				b.CBind<VmWordListCard>(
					BorderBrushProperty
					,(x) => x.LearnedColor
				);
				b.BorderThickness = new(4,0,0,0);
				b.LongPressDurationMs = Ctx?.CfgUi.LongPressDurationMs?? b.LongPressDurationMs;
				b.ContextMenu = ViewWordListCard.MkWordCardCtxMenu(Ctx, VmWordCard?.WordForLearn?.JnWord);
				b.OnLongPressed += (s,e)=>{
					b.ContextMenu.Open();
				};
				b.LongPressDurationMs = 500;
				StyBtnWordCard(b.Styles);
				b.SetContent(new ViewWordListCard(VmWordCard), w=>{
					w.HostClickBtn = b;
					WordCardCtrls.Add(w);
					w.VAlign(x=>x.Stretch)
					.HAlign(x=>x.Stretch);
					w.Background = Brushes.Transparent;
					w.CBind<VmWordListCard>(w.Prop(x=>x.DataContext),x=>x);
					b.Click += (s,e)=>{
						if(w?.Ctx is not null){
							Ctx?.ClickWordCard(w.Ctx);
						}
					};
				});
			});
			return Grid;
		});
		return Ic;
	}

	ViewWordInfo _WordInfo(){
		var R = new ViewWordInfo();
		return R;
	}

	Styles StyBtnWordCard(Styles s){
		s.A(
			Sty.Is<Button>(
				x=>x.Class(PC.pointerover)
				.Template()
				.OfType<ContentPresenter>()
			).Set(
				x=>x.BorderBrush
				,CBE.Mk<VmWordListCard>(x=>x.LearnedColor)
			)
		).A(
			Sty.Is<Button>(x=>x
				.Class(PC.pressed)
				.Template()
				.OfType<ContentPresenter>()
			).Set(
				x=>x.BorderBrush
				,CBE.Mk<VmWordListCard>(x=>x.LearnedColor)
			)
		)
		;
		return s;
	}

	protected async Task<nil> ClickBtn(OpBtn? Btn, CT Ct){
		if(Btn is null){
			return NIL;
		}
		return await Btn.ClickAndWaitDone(Ct);
	}
}
