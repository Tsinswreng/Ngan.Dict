namespace Ngaq.Ui.Views.Word.WordInfo;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Tools;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.StrokeText;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.StrokeText;
using Tsinswreng.AvlnTools;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Ctx = VmWordInfo;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewWordInfo
	: AppViewBase<Ctx>
{

	public ViewWordInfo() {
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
		DataContextChanged += (S, E) => {
			OnCtxChanged();
		};
		OnCtxChanged();
	}


	/// 樣式類名枚舉，避免直接硬編碼 class 字符串。
	public partial class Cls {
		public const str LightGray = nameof(LightGray);
	}


	public Color Gray = Colors.LightGray;
	/// splitter 用 1px 保持可拖拽，同時避免 0.5px 在不同縮放下看起來發虛變粗。
	public const double SplitterThickness = 1;

	public GridStack Root { get; set; } = new(IsRow: true);
	Grid? MainGrid;
	Control? Splitter;
	Control? SidePropsPane;
	Ctx? SubscribedCtx;
	StrokeTextBlock? HeadTextCtrl;
	IList<StrokeTextBlock> DescriptionTextCtrls { get; } = [];

	protected nil Style() {
		Styles.A(
			Sty.Is<Control>(
				x => x.Class(Cls.LightGray)
			).Set(
				ForegroundProperty
				, new SolidColorBrush(Gray)
			)
		);
		return NIL;
	}

	/// 統一使用描邊文字，與學習頁和卡片頁的視覺語言保持一致。
	protected StrokeTextBlock TxtBox() {
		var R = new StrokeTextBlock {
			Foreground = Brushes.White,
			Stroke = Brushes.Black,
			StrokeThickness = 3,
			UseVirtualizedRender = true,
			TextWrapping = TextWrapping.Wrap,
		};
		return R;
	}

	/// prop key 專用弱化字樣，避免與內容文字搶視線。
	protected StrokeTextBlock SubTxt() {
		var R = TxtBox();
		R.Foreground = new SolidColorBrush(Gray);
		R.StrokeThickness = 1.5;
		R.FontSize = UiCfg.Inst.BaseFontSize * 0.85;
		return R;
	}

	/// 側欄 prop 值專用字樣：保留描邊，但略收細，避免與標題擠在一起。
	protected StrokeTextBlock SideValueTxt() {
		var R = TxtBox();
		R.StrokeThickness = 1.5;
		R.FontSize = UiCfg.Inst.BaseFontSize * 0.95;
		return R;
	}

	protected nil Render() {
		this.SetContent(Root.Grid, o => {
			o.SetRowDefs([
				new(3, GUT.Auto),
				new(100, GUT.Star),
			]);
		});

		Root
		//詞頭框
		.A(new Border(), o => {
			o.BorderThickness = new(0, 1, 0, 1);
			o.BorderBrush = new SolidColorBrush(Gray);
			o.SetChild(TxtBox(), x => {
				HeadTextCtrl = x;
				Ctx.Bind(x, x=>x.Text, Vm => Vm.Head);
				x.FontSize = UiCfg.Inst.BaseFontSize * 1.4;
				x.StrokeThickness = 4;
				x.Styles.Add(new Style().NoMargin().NoPadding());
			});
		})
		.A(MkMainContent());

		Root.Add();
		return NIL;
	}

	/// DataContext 變更時重新訂閱當前 Vm，避免仍盯着構造時的臨時 Ctx。
	protected nil OnCtxChanged() {
		if (SubscribedCtx is not null) {
			SubscribedCtx.PropertyChanged -= OnCtxPropertyChanged;
			SubscribedCtx = null;
		}
		if (Ctx is not null) {
			SubscribedCtx = Ctx;
			SubscribedCtx.PropertyChanged += OnCtxPropertyChanged;
		}
		DescriptionTextCtrls.Clear();
		SyncSidePaneVisibility();
		return NIL;
	}

	/// 其他 prop 集合變更後同步刷新右欄顯示狀態。
	protected void OnCtxPropertyChanged(object? Sender, PropertyChangedEventArgs E) {
		if (
			E.PropertyName == nameof(Ctx.DescriptionWordProps)
			|| E.PropertyName == nameof(Ctx.SideWordProps)
		) {
			if(E.PropertyName == nameof(Ctx.DescriptionWordProps)){
				DescriptionTextCtrls.Clear();
			}
			SyncSidePaneVisibility();
		}
	}

	protected nil NotifyOutputChanged(str PropertyName){
		Dispatcher.UIThread.Post(() => {
			RaisePropertyChanged(PropertyName);
		});
		return NIL;
	}

	/// 主內容區爲左右分欄：左側 description，右側其他 prop，中間細分隔條可拖拽。
	/// 主內容容器負責左側 description、右側其他 prop 與中間拖拽分隔條。
	Control MkMainContent() {
		var Root = new GridStack(IsRow: false);
		MainGrid = Root.Grid;
		MainGrid.SetColDefs([
			new(1, GUT.Star),
			new(SplitterThickness, GUT.Pixel),
			new(1, GUT.Auto),
		]);
		Splitter = MkColumnSplitter();
		SidePropsPane = new Border();
		Root
		.A(new Border(), o => {
			o.SetChild(new ScrollViewer(), Sv => {
				Sv.SetContent(new ItemsControl(), L => {
					Ctx.Bind(L, L=>L.ItemsSource, x => x.DescriptionWordProps);
					L.ItemTemplate = new FuncDataTemplate<PoWordProp>((Prop, b) => {
						return MkPropCard(Prop, IsDescriptionPane: true);
					});
				});
			});
		})
		.A(Splitter)
		.A(SidePropsPane, o => {
			if (o is not Border B) {
				return;
			}
			B.BorderThickness = new(0);
			B.MinWidth = UiCfg.Inst.BaseFontSize * 2.2;
			B.HAlign(x=>x.Stretch);
			B.SetChild(new ScrollViewer(), Sv => {
				Sv.HAlign(x=>x.Stretch);
				Sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
				Sv.SetContent(new ItemsControl(), L => {
					L.HAlign(x=>x.Stretch);
					L.SetItemsPanel(() => {
						return new StackPanel {
							Orientation = Orientation.Vertical,
							HorizontalAlignment = HAlign.Stretch,
						};
					});
					Ctx.Bind(L, L=>L.ItemsSource, x => x.SideWordProps);
					L.ItemTemplate = new FuncDataTemplate<PoWordProp>((Prop, b) => {
						return MkPropCard(Prop, IsDescriptionPane: false);
					});
				});
			});
		});
		MainGrid.GetObservable(BoundsProperty).Subscribe(_ => {
			SyncSidePaneVisibility();
		});
		return Root.Grid;
	}

	/// 沒有其他 prop 時，收起右欄與分隔條，避免留白。
	protected nil SyncSidePaneVisibility() {
		if (MainGrid is null || Splitter is null || SidePropsPane is null) {
			return NIL;
		}
		var HasAny = HasSideProps();
		Splitter.IsVisible = HasAny;
		SidePropsPane.IsVisible = HasAny;
		MainGrid.ColumnDefinitions[1].Width = HasAny
			? new GridLength(SplitterThickness, GUT.Pixel)
			: new GridLength(0, GUT.Pixel);
		MainGrid.ColumnDefinitions[2].Width = HasAny
			? new GridLength(1, GUT.Auto)
			: new GridLength(0, GUT.Pixel);
		if (SidePropsPane is Border SideBorder) {
			SideBorder.MinWidth = HasAny ? UiCfg.Inst.BaseFontSize * 2.2 : 0;
			SideBorder.MaxWidth = HasAny ? CalcSidePaneMaxWidth() : 0;
		}
		return NIL;
	}

	/// 側欄最大寬度不得超過主內容區的一半，避免比 description 區更寬。
	protected double CalcSidePaneMaxWidth() {
		var TotalWidth = MainGrid?.Bounds.Width ?? 0;
		if (TotalWidth <= 0) {
			return 220;
		}
		var MaxWidth = (TotalWidth - 2) / 2;
		return Math.Max(120, MaxWidth);
	}

	/// 右欄只負責展示 description 之外的屬性。
	protected bool HasSideProps() {
		return Ctx?.SideWordProps?.Count > 0;
	}

	/// 分隔條保持細線寬度，僅用於拖拽分欄，不搶畫面。
	Control MkColumnSplitter() {
		var R = new GridSplitter();
		R.ResizeDirection = GridResizeDirection.Columns;
		R.HorizontalAlignment = HAlign.Stretch;
		R.VerticalAlignment = VAlign.Stretch;
		R.Width = SplitterThickness;
		R.MinWidth = SplitterThickness;
		R.Background = new SolidColorBrush(Color.FromArgb(180, Gray.R, Gray.G, Gray.B));
		return R;
	}

	/// 所有 WordProp 的卡片統一用同一套頭部結構，右上角固定放貼邊編輯圖標。
	Control MkPropCard(PoWordProp Prop, bool IsDescriptionPane) {
		if (IsDescriptionPane) {
			return MkDescriptionPropCard(Prop);
		}
		return MkSidePropCard(Prop);
	}

	/// description 不顯示灰字標題，正文與編輯圖標分列避免互相覆蓋。
	Control MkDescriptionPropCard(PoWordProp Prop) {
		var R = new Border();
		var ValueTxt = MkPropValueTxt(IsDescriptionPane: true);
		DescriptionTextCtrls.Add(ValueTxt);
		R.BorderThickness = new(0, 1, 0, 0);
		R.BorderBrush = new SolidColorBrush(Gray);
		R.Padding = new(8, 8);
		R.HorizontalAlignment = HAlign.Stretch;
		R.SetChild(new Grid(), o => {
			o.SetColDefs([
				new(1, GUT.Star),
				new(1, GUT.Auto),
			]);
			o
			.A(ValueTxt, o => {
				o.Text = GetPropValueText(Prop);
				o.HorizontalAlignment = HAlign.Left;
				o.VerticalAlignment = VAlign.Top;
			})
			.A(MkEditBtn(Prop), o => {
				Grid.SetColumn(o, 1);
				o.Margin = new(8, 0, 0, 0);
			});
		});
		return R;
	}

	/// 側欄保留 prop 名稱標題，並讓卡片整欄拉伸，保證分隔線等寬。
	Control MkSidePropCard(PoWordProp Prop) {
		var R = new Border();
		R.BorderThickness = new(0, 0, 0, 1);
		R.BorderBrush = new SolidColorBrush(Gray);
		R.Padding = new(8, 8);
		R.HorizontalAlignment = HAlign.Stretch;
		R.SetChild(new Grid(), o => {
			o.SetRowDefs([
				new(1, GUT.Auto),
				new(1, GUT.Auto),
			]);
			o.SetColDefs([
				new(1, GUT.Star),
				new(1, GUT.Auto),
			]);
			o
			.A(SubTxt(), o => {
				o.Text = TranslatePropKey(Prop.KStr);
				o.HorizontalAlignment = HAlign.Left;
				o.VerticalAlignment = VAlign.Top;
			})
			.A(MkEditBtn(Prop), o => {
				Grid.SetColumn(o, 1);
			})
			.A(MkPropValueTxt(IsDescriptionPane: false), o => {
				o.Text = GetPropValueText(Prop);
				o.HorizontalAlignment = HAlign.Left;
				Grid.SetRow(o, 1);
				Grid.SetColumnSpan(o, 2);
			});
		});


		return R;
	}

	/// 編輯按鈕只放圖標，並去掉額外邊距讓其緊貼卡片右上角。
	Button MkEditBtn(PoWordProp Prop) {
		var R = new Button();
		R.SetContent(Icons.Edit().ToIcon());
		R.HorizontalAlignment = HAlign.Right;
		R.VerticalAlignment = VAlign.Top;
		R.Padding = new(0);
		R.Margin = new(0);
		R.MinWidth = 0;
		R.MinHeight = 0;
		R.Background = Brushes.Transparent;
		R.Click += (Sender, E) => {
			OpenPropEditor(Prop);
		};
		return R;
	}

	/// description 區使用正文字樣，其餘 prop 保持側欄字樣。
	StrokeTextBlock MkPropValueTxt(bool IsDescriptionPane) {
		if (IsDescriptionPane) {
			var R = TxtBox();
			R.FontSize = UiCfg.Inst.BaseFontSize;
			return R;
		}
		return SideValueTxt();
	}

	/// 內置 prop 鍵顯示 i18n，未知鍵保留原始文本以兼容自定義 prop。
	protected str TranslatePropKey(str? Key) {
		return Key switch {
			var x when x == KeysProp.Inst.summary => I[K.Summary],
			var x when x == KeysProp.Inst.description => I[K.Descr],
			var x when x == KeysProp.Inst.note => I[K.Note],
			var x when x == KeysProp.Inst.tag => I[K.Tag],
			var x when x == KeysProp.Inst.source => I[K.Source],
			var x when x == KeysProp.Inst.alias => I[K.Alias],
			var x when x == KeysProp.Inst.pronunciation => I[K.Pronunciation],
			var x when x == KeysProp.Inst.weight => I[K.Weight],
			var x when x == KeysProp.Inst.learn => I[K.Learn],
			var x when x == KeysProp.Inst.usage => I[K.Usage],
			var x when x == KeysProp.Inst.example => I[K.Example],
			var x when x == KeysProp.Inst.relation => I[K.Relation],
			var x when x == KeysProp.Inst.Ref => I[K.Ref],
			_ => Key ?? "",
		};
	}

	/// 單條 prop 值完整展示，與編輯頁的字段語義保持一致。
	protected str GetPropValueText(PoWordProp Prop) {
		// 發音字段有時會把內部結構序列化進 VStr；展示時只取對用戶有意義的 Text。
		if (Prop.KStr == KeysProp.Inst.pronunciation && Prop.VType == EKvType.Str && Prop.VStr is not null) {
			return GetPronunciationDisplayText(Prop.VStr);
		}
		if (Prop.VType == EKvType.Str) {
			return Prop.VStr ?? "";
		}
		if (Prop.VType == EKvType.I64) {
			return Prop.VI64.ToString();
		}
		if (Prop.VF64 != 0) {
			return Prop.VF64.ToString();
		}
		if (Prop.VBinary is not null && Prop.VBinary.Length > 0) {
			return $"<binary:{Prop.VBinary.Length}>";
		}
		return Prop.VStr ?? "";
	}

	/// pronunciation 的 JSON 值只展示 Text，避免把原始結構直接渲染到頁面上。
	protected str GetPronunciationDisplayText(str RawText) {
		// step 1: 優先把原值當 JSON 對象解析，並只讀取展示字段。
		try {
			using var Document = JsonDocument.Parse(RawText);
			if (
				Document.RootElement.ValueKind == JsonValueKind.Object
				&& Document.RootElement.TryGetProperty(PronunciationJsonTextKey, out var TextElement)
				&& TextElement.ValueKind == JsonValueKind.String
			) {
				return TextElement.GetString() ?? RawText;
			}
		} catch (JsonException) {
			// step 2: 非 JSON 或 JSON 結構不符合約定時，保持舊值原樣展示。
		}
		return RawText;
	}

	/// pronunciation JSON 的文本字段名，集中成常量避免散落魔法字串。
	const str PronunciationJsonTextKey = "Text";

	/// 先進入現有 WordEditV2，再自動打開對應 prop 的編輯框，沿用原有保存流程。
	protected nil OpenPropEditor(PoWordProp Prop) {
		if (Ctx?.WordForLearn?.JnWord is null) {
			return NIL;
		}
		// step 1: 建立並初始化既有編輯頁，保持保存流程完全復用。
		var EditView = new ViewWordEditV2();
		if (EditView.Ctx is null) {
			return NIL;
		}
		EditView.Ctx.FromJnWord(Ctx.WordForLearn.JnWord);
		EditView.Ctx.TabIndex = 1;
		EditView.Loaded += OnLoaded;
		ViewNavi?.GoTo(ToolView.WithTitle(Ctx.Head, EditView));
		return NIL;

		void OnLoaded(object? Sender, RoutedEventArgs E) {
			// step 2: 等編輯頁 rows 準備好後，再精確命中對應 prop 並打開現有編輯框。
			EditView.Loaded -= OnLoaded;
			var Row = EditView.Ctx?.WordPropPage.Rows.FirstOrDefault(x => IsSameProp(x.Raw, Prop));
			if (Row is not null) {
				EditView.Ctx?.WordPropPage.RequestEdit(Row);
			}
		}
	}

	/// 優先按 Id 匹配；新建/無 Id 的情況退化到 key 與值匹配。
	protected bool IsSameProp(PoWordProp A, PoWordProp B) {
		if (!A.Id.IsNullOrDefault() && !B.Id.IsNullOrDefault()) {
			return A.Id == B.Id;
		}
		return A.KType == B.KType
			&& A.KStr == B.KStr
			&& A.KI64 == B.KI64
			&& A.VType == B.VType
			&& A.VStr == B.VStr
			&& A.VI64 == B.VI64
			&& A.VF64 == B.VF64;
	}
}
