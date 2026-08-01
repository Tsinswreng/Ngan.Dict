namespace Ngaq.Ui.Views.BottomBar;

using Avalonia;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Media;
using Avalonia.Controls.Templates;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Ctx = VmBottomBar;
using Tsinswreng.Avln.Grid;

public partial class ViewBottomBar
	:UserControl
{

	public Ctx? Ctx{
		get{return DataContext as Ctx;}
		set{DataContext = value;}
	}

	public ObservableCollection<Btn_Control> Items{get;set;} = new ObservableCollection<Btn_Control>();


	public ViewBottomBar(){
		Ctx = new Ctx();
		Style();
		Render();
		InitConstructedHandlers();
	}

	protected void InitConstructedHandlers(){
		// 當 Items 被添加時，如果尚未選中則預設選中第一個
		Items.CollectionChanged += (s, e) =>{
			if (Cur.Content == null && Items.Count > 0){
				Cur.Content = Items[0].GetOrCreateControl();
			}
			UpdateSelectedHighlight();
		};
		// 當控件加入視覺樹時再嘗試設置初始選中（保險）
		this.AttachedToVisualTree += (s,e)=>{
			if (Cur.Content == null && Items.Count > 0){
				Cur.Content = Items[0].GetOrCreateControl();
			}
			UpdateSelectedHighlight();
		};
	}

	public partial class Cls_{
		public str BarItem = nameof(BarItem);
	}
	public Cls_ Cls{get;set;} = new Cls_();

	protected nil Style(){
		return NIL;
	}

	GridStack Root = new(IsRow:true);

	public ItemsControl ItemsControl{get;set;} = new ItemsControl();

	public ContentControl Cur{get; protected set;} = new ContentControl();

	// 缓存用于高亮的主题画刷
	protected IBrush? ThemeBrush{get; set;} = null;


	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(18, GUT.Star),
				new(1, GUT.Star),
			]);
		});
		{{
			Root.A(Cur)
			.A(new ItemsControl(), o=>{
				o.ItemsSource = Items;
				o.ItemsPanel = new FuncTemplate<Panel?>(()=>{
					return new UniformHorizontalPanel();
				});
				o.ItemTemplate = new FuncDataTemplate<Btn_Control>((Btn_Control, b)=>{
					var Ans = Btn_Control.Button;
					Ans.Click += (s,e)=>{
						Cur.Content = Btn_Control.GetOrCreateControl();
						UpdateSelectedHighlight();
					};
					// 初始時綁定 DataContext 方便查找
					Ans.DataContext = Btn_Control;
					return Ans;
				});
			});
			// 當 Cur.Content 改變時更新高亮
			Cur.GetObservable(ContentControl.ContentProperty)
				.Subscribe(_=>{
					UpdateSelectedHighlight();
				});
			// 取得主題畫刷
			ThemeBrush = UiCfg.Inst.MainColor;
			// 初次刷新
			UpdateSelectedHighlight();
		}}
		return NIL;
	}

	protected void UpdateSelectedHighlight(){
		if (Items == null) return;
		foreach(var it in Items){
			var b = it.Button;
			if (it.Control is not null && it.Control == Cur.Content){
				if (ThemeBrush != null){
					b.Background = ThemeBrush;
					b.Foreground = Brushes.White;
				} else {
					// 若無主題畫刷，回退到淺藍高亮
					b.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x78, 0xD4));
					b.Foreground = Brushes.White;
				}
			} else {
				// 恢復為預設（清除本地值以回退樣式）
				b.ClearValue(Button.BackgroundProperty);
				b.ClearValue(Button.ForegroundProperty);
			}
		}
	}

	/// <summary>
	/// 切換到底欄中對應的目標頁面（按控件實例匹配）。
	/// </summary>
	/// <param name="Target">目標內容控件。</param>
	/// <returns>空值。</returns>
	public nil SelectControl(Control Target){
		foreach(var item in Items){
			var control = item.GetOrCreateControl();
			if(object.ReferenceEquals(control, Target)){
				Cur.Content = control;
				UpdateSelectedHighlight();
				return NIL;
			}
		}

		// 若目標不在底欄列表中，仍嘗試顯示，避免調用方無感失敗。
		Cur.Content = Target;
		UpdateSelectedHighlight();
		return NIL;
	}


}



public partial class UniformHorizontalPanel : Panel{
	protected override Size MeasureOverride(Size availableSize){
		f64 maxHeight = 0;
		i32 count = Children.Count;
		if (count == 0)
			return new Size();

		f64 itemWidth = availableSize.Width / count; // 均分宽度

		foreach (var child in Children){
			child.Measure(new Size(itemWidth, availableSize.Height));
			if (child.DesiredSize.Height > maxHeight){
				maxHeight = child.DesiredSize.Height;
			}
		}

		return new Size(availableSize.Width, maxHeight);
	}

	protected override Size ArrangeOverride(Size finalSize){
		i32 count = Children.Count;
		if (count == 0){
			return finalSize;
		}

		f64 itemWidth = finalSize.Width / count;

		f64 x = 0;
		foreach (var child in Children){
			// 按均分宽度排列，每个元素宽度相同，高度为自己的DesiredSize.Height（或者 finalSize.Height）
			Rect rect = new Rect(x, 0, itemWidth, finalSize.Height);
			child.Arrange(rect);
			x += itemWidth;
		}

		return finalSize;
	}
}
