namespace Ngaq.Ui.CodeTemplate.Sample;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;
using Ctx = VmSample;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
public partial class ViewSample
	:AppViewBase<Ctx>
{
	public ViewSample(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
		Loaded += (s,e)=>{
			//只在構造函數中 做 UI初始化相關操作
			//如果希望在進入界面的時候做一些其他的初始化操作 如調用接口獲取數據/耗時操作 等、應放在Loaded回調中
		};
	}



	//大多數場景下我們用GridStack作爲視圖的根節點。
	//GridStack支持 或全爲行 或全爲列 的佈局 不建議同時設置行和例。
	//每次Add時會自動設置行號或列號 因此不要手動設計行/列號
	//優先使用GridStack、別用原生Grid 除非你要顯示手動設置行和列
	GridStack Root = new(IsRow: true);//IsRow: true 表示行佈局
	//視圖的初始化羅輯寫在Render裏
	public void Render(){
		this.SetContent(Root.Grid);
		Root.SetRowDefs([
			new(1, GUT.Auto),
			new(2, GUT.Star),
			new(30, GUT.Pixel),
			//....略
		]);
		//也可以寫 Root.Grid.RowDefinitions = new("Auto,2*,30");
		//GridStack 與 所有的Panel都有 A<TControl>(TControl C, Action<TControl>? FnInit=null)擴展方法。

		Root
		//可鏈式調用
		// lambda的形參以一到兩個字母爲宜
		.A(new TextBox(), o=>{
			o.AcceptsReturn = true;
			//下面展示綁定寫法。本項目使用AOT兼容的綁定寫法。不准用new Binding(字符串)
			//功能最全但不是最常用的寫法:
			o.Bind(
				TextBox.TextProperty
				,CBE.Mk<Ctx>(x=>x.Cnt1)//對應Ctx(即ViewModel)中的成員
				#if false //可加其他可選命名參數如:
				,Converter:
				,ConverterParameter:
				,Mode:
				#endif
			);

			//寫法二。比上面的寫法稍簡單點 但仍然不是最簡單的寫法 也不是最常用的寫法。
			//這種寫法一般用于綁定源不是Ctx的情況(如ItemTemplate中)。當綁定源是Ctx時優先用寫法三
			o.CBind<Ctx>(TextBox.TextProperty, x=>x.Cnt1);

			//寫法三。大部分情況下、綁定源都是Ctx 故此時則採用簡便寫法如下:
			Ctx.Bind(o, o=>o.Text, x=>x.Cnt1);
			//上面的第二個參數優先用o=>o.Text的寫法。如當o爲TextBox時o=>o.Text即等於TextBox.TextProperty。
			// 不得已時再用 類名.XxxProperty的寫法

			//所有綁定 不需要顯示指定BindingMode的 就別寫BindingMode !
		}).A(new Button(), b=>{
			//初始化ContentControl.Content時使用SetContent擴展方法、不要直接給Content賦值
			b.SetContent(new TextBlock(), t=>{
				t.Text = I[K.ButtonOne];//項目要支持i18n。禁止直接硬編碼。臨時硬編碼要寫成這樣: t.Text = Todo.I18n("按鈕1");
			});
//你也可以直接給o.Content賦值 o.Content = new TextBlock(){Text="按鈕一"};
//按鈕綁定事件的寫法
			b.Click += (s,e)=>{
				Ctx?.Click1();
			};
		}).A(new ScrollViewer(), Sv=>{
			Sv.SetContent(new StackPanel(), Sp=>{
				Sp.A(new OpBtn(), o=>{
					o._Button.Content = I[K.CallBackendService];
					o.SetExe(Ct=>Ctx?.CallService(Ct));
				}).A(MkList());
			});
		})
		;
	}

//列表示例
//當View中縮進層次過多旹、可將Render中的部分代碼抽到一個單獨的函數中、把控件返回出去、再在Render中調用
//嵌套層數深時 或有高度重複的代碼時纔抽取！不要爲了抽而抽
	ItemsControl MkList(){
		var R = new ItemsControl();
		R.SetItemTemplate<str>((ele, ns)=>{
			return new TextBox{Text=ele};
		}).SetItemsPanel(()=>{
			return new VirtualizingStackPanel();
		});
		R.Bind(R.PropItemsSource, CBE.Mk<Ctx>(x=>x.List));
		return R;
	}

	#region Style
	//樣式(非必選)示例
	//樣式簡單旹建議直接和控件一起初始化。有[需批量設置樣式]等需求旹再用Style
	public partial class Cls{//類名枚舉
		public const string MenuBtn = nameof(MenuBtn);
	}
	public void Style(){
		Styles.A(
			//傳統寫法。通常要寫完整的類名.XxxProperty、比較冗長且無編譯期類型檢查。不推薦
			new Style(x=>
				x.Is<Button>()
				.Class(Cls.MenuBtn)//禁止硬編碼字符串作類名
			).Set(
				VerticalAlignmentProperty
				,VAlign.Stretch
			).Set(//.Set也能且必須鏈式調用
				HorizontalAlignmentProperty
				,HAlign.Center
			)
		).A(
			Sty.Is<Button>(//Sty下有Is和OfType
				x=>x.Class(Cls.MenuBtn)
				//此寫法即等價于 new Style(x=>x.Is<Button>().Class(Cls.MenuBtn))、但更簡潔且有編譯期類型檢查
			).Set(
				//更推薦的簡便寫法、不用寫ClsName.XxxProperty
				x=>x.HorizontalAlignment, HAlign.Center
			).Set(
				x=>x.VerticalAlignment, VAlign.Stretch
			)
		).A(//Styles也必須使用.A鏈式調用
			Sty.Is<ContentControl>()
			.Set(
				//可以在Style中設置綁定
				x=>x.CornerRadius, CBE.Mk<Ctx>(
					x=>x.Cnt1,
					Converter: new FnConvtr<int, CornerRadius>(cnt=>{
						return new CornerRadius(cnt);
					})
				)
			)
		)
		;
	}

	#endregion Style

	//導航示例
	public void SampleNavi(Button Btn, Func<Control> MkTargetView){
		//帶標題跳轉
		Btn.Click += (s,e)=>{
			//若不要標題就直接傳進Goto
			// 界面跳轉 只能在View層操作。不要在Vm層做頁面跳轉 或使用任何View層的東西
			var TargetView = MkTargetView(); //用Func<>實現延遲加載
			ViewNavi?.GoTo(ToolView.WithTitle(I[K.TopbarTitle], TargetView));
			_ = @$"如果你希望頂欄標題有自定義菜單按鈕 需讓TargetView實現{nameof(I_MkTitleMenu)}";
		};
	}

	public void SampleIcon(){
		//基礎示例
		Avalonia.Controls.Shapes.Path addIcon = Icons.Add();
		var b1 = new Button();
		b1.SetContent(addIcon);
		// Svgs下 可用的圖標 在 Ngaq.Frontend/proj/Ngaq.Ui/Icons/Icons.Decl.cs
		// 禁止閱讀 Icons.Impl.cs !!!!
		//圖標接文字示例:
		var b2 = new Button();
		b2.SetContent(Icons.Add().WithText("Add"));
	}


	[Doc(@$"使用Tsinswreng.Avln.Dsl。
	要求:
	- 在lambda中 用o.Xxx = Yyy的寫法初始化 加入控件樹的子控件、而不是使用屬性初始化塊
	- 組織子控件並加入控件樹時、代碼塊的嵌套 要和 樹的邏輯結構 保持一致
	")]
	public void SampleOfUsingDsl(){
		var 正確示例 = (Panel p)=>{
			//所有Panel都能用.A()擴展方法
			p.A(new TextBox(), t=>{
				//在這裏通過o.Xxx初始化
				t.Text = I[K.CorrectExample];
			})
			//多次.A()時必須使用鏈試調用寫法p.A().A()...、不允許p.A();p.A();...
			.A(new Control())//第二個Action<>可省略不傳
			.A(new Button(), b=>{
				b.BorderThickness = new(0);//能直接寫 = new(...); 的 就不要寫 = new SomeType(...);
				b.HAlign(x=>x.Center);
				// 給ContentControl的Content賦值時必須使用SetContent擴展方法、禁止直接賦值
				b.SetContent(new TextBlock(), t=>{
					t.FontSize = UiCfg.Inst.BaseFontSize*1.2;
				});
			}).A(new Border(), b=>{ // 組織子控件並加入控件樹時、代碼塊的嵌套 要和 樹的邏輯結構 保持一致
				//Border必須使用SetChild、禁止使用 b.Child=xxx 的寫法
				b.SetChild(new ScrollViewer(), sv=>{//lambda中的形參名稱長度不得多于2字符
					sv.SetContent(new StackPanel(), sp=>{
						sp.A(new TextBlock())
						.A(new TextBox());
					});
				});
			})
			;
		};

		var 錯誤示例 = (Panel p)=>{
			var textBox = new TextBox{
				Text = "錯誤示例"
			};
			//絕對禁止寫 panel.Children.Add() !
			//一律用 panel.A()
			p.Children.Add(textBox);
			p.Children.Add(new Control());
			var btn = new Button();
			var textBlock = new TextBlock(){
				FontSize = 20
			};
			btn.Content = textBlock;
			//錯誤原因: 完全未按Dsl層級寫法來
			// 未使用A和SetContent擴展方法;
			// 初始化控件屬性寫法不符合規範; 硬編碼字體大小; 硬編碼文本未使用Todo.I18n
			// 代碼塊的嵌套 未和 樹的邏輯結構 保持一致
		};//以上代碼是*錯誤示例*！
	}

	public void SampleNoDupliStyle(){
		var 錯誤示例 = (Panel P)=>{
			P.A(new Button(), b=>{
				b.Background = Brushes.Gray;
				b.VAlign(x=>x.Center);
				b.HAlign(x=>x.Center);
				b.SetContent(I[K.File]);
			}).A(new Button(), b=>{
				b.Background = Brushes.Gray;
				b.VAlign(x=>x.Center);
				b.HAlign(x=>x.Center);
				b.SetContent(I[K.Edit]);
			})
			;
		};
		var 正確示例1 = (Panel P)=>{
			var _Btn = ()=>{
				var o = new Button();
				o.Background = Brushes.Gray;
				o.VAlign(x=>x.Center);
				o.HAlign(x=>x.Center);
				return o;
			};
			P.A(_Btn(), b=>{
				b.SetContent(I[K.File]);
			}).A(_Btn(), o=>{
				o.SetContent(I[K.Edit]);
			})
			;
		};
		var 更推薦的正確示例2 = (Panel P)=>{
			P.Styles.A(
				Sty.Is<Button>(
					x=>x.Class(Cls.MenuBtn)
				).Set(x=>x.HorizontalAlignment, HAlign.Center)
				.Set(x=>x.VerticalAlignment, VAlign.Center)
				.Set(x=>x.Background, Brushes.Gray)
			);
			P.A(new Button(), o=>{
				o.Classes.Add(Cls.MenuBtn);
				o.SetContent(I[K.File]);
			}).A(new Button(), o=>{
				o.Classes.Add(Cls.MenuBtn);
				o.SetContent(I[K.Edit]);
			});
		};
	}


}

