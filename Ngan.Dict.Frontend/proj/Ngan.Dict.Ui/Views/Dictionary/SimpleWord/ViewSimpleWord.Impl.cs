namespace Ngan.Dict.Ui.Views.Dictionary.SimpleWord;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Ui;
using Ngan.Dict.Ui.Icons;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;

/// ViewSimpleWord 的實現。
///
/// 排版原則（改樣式時照這個來）：
/// 一、層次只靠三級文字色（主／次／弱）與三種字號（1.6 詞頭、1.0 正文、0.85 註釋）建立，不用裝飾色塊。
/// 二、間距一律從 `UiCfg` 的語義節點取（頁邊距／組間距／組內距／縮排），不在各處寫死數字。
/// 三、全應用禁圓角，故不使用任何 CornerRadius；需要分組時用留白與縮排，不用框。
public partial class ViewSimpleWord{

	/// View 只能有無參構造器。
	/// *為何不在這裏自己解析 Vm*：本視圖顯示的是「詞典頁那一個查詞結果」，
	/// 必須用父層傳進來的同一份 Vm 實例（流式過程中是就地改屬性，不會換對象），
	/// 自己 `DiOrMk` 會拿到另一個空實例，界面就永遠不更新。
	/// 由父層在建構後、掛進視覺樹前呼叫 `SetVm`。
	public partial ViewSimpleWord(){
	}

	/// 同一塊位置放兩張卡：詞典卡與翻譯卡，由 `Kind` 互斥顯示。
	/// 兩張都建好再切可見性，好處是切換時不必重建控件樹、也不會丟掉已綁定的狀態。
	protected override partial object Build(VmSimpleWord vm){
		return new Grid().Children(
			MkDictCard(vm).IsVisible(vm, x => x.IsDict)
			,MkTranslateCard(vm).IsVisible(vm, x => x.IsTranslate)
		);
	}

	/// 詞典卡：詞頭＋讀音同排，其下逐義項排列。
	/// *為何詞頭與讀音同排*：兩者是一個整體（「這個詞怎麼念」），分行會讓讀音看起來像第二個標題。
	protected partial Control MkDictCard(VmSimpleWord vm){
		var Card = new StackPanel(){ Orientation = Orientation.Vertical };
		Card.Margin = CardMargin();
		Card.Children(
			MkHeadRow(vm)
			,MkSenses(vm)
				.Margin(new Thickness(0, UiCfg.Inst.GapSection, 0, 0))
		);
		return Card;
	}

	/// 詞頭列：左邊詞頭、右邊讀音（底線對齊，讀音較小且弱化）。
	protected partial Control MkHeadRow(VmSimpleWord vm){
		var Row = new StackPanel{
			Orientation = Orientation.Horizontal,
			VerticalAlignment = VAlign.Bottom,
		};
		Row.Children(
			new SelectableTextBlock()
				.With(t => {
					_Head = t;
					t.FontSize = UiCfg.Inst.BaseFontSize * 1.6;
					t.FontWeight = FontWeight.SemiBold;
					t.VerticalAlignment = VAlign.Bottom;
					t.Margin = new Thickness(0, 0, UiCfg.Inst.GapSection, 0);
				})
				.Text(vm, x => x.Head)
			,MkPronunciations(vm)
		);
		return Row;
	}

	/// 建立允許選取且自動換行的文本控件。
	/// 選取能力是必要的：查詞結果常被複製到別處，沒有複製按鈕時它就是唯一出口。
	protected partial SelectableTextBlock Txt(){
		return new SelectableTextBlock().TextWrapping(TextWrapping.Wrap);
	}

	/// 讀音列表：橫排，每個讀音「小播放鈕 ＋ 文字」，整體靠左並與詞頭底線對齊。
	protected partial Control MkPronunciations(VmSimpleWord vm){
		var List = new ItemsControl()
			.With(l => _Pronunciations = l)
			.ItemsSource(vm, x => x.Pronunciations)
			.ItemsPanel(new StackPanel{
				Orientation = Orientation.Horizontal,
				VerticalAlignment = VAlign.Bottom,
			})
			.VerticalAlignment(VAlign.Bottom)
		;
		List.ItemTemplate = new FuncDataTemplate<Pronunciation>((P, _) => {
			// 虛擬化回收容器時模板會被以 null 呼叫。
			if(P is null){
				return null;
			}
			return MkPronunciationRow(vm, P);
		});
		return List;
	}

	/// 單行讀音：極小的播放鈕 ＋ 弱化的讀音文字。
	protected partial Control MkPronunciationRow(VmSimpleWord vm, Pronunciation P){
		var Row = new StackPanel(){
			Orientation = Orientation.Horizontal,
			VerticalAlignment = VAlign.Bottom,
		};
		Row.Children(
				new OpBtn()
					.With(b => {
						var Icon = Icons.VolHigh().ToIcon();
						Icon.Height = UiCfg.Inst.BaseFontSize*0.62;
						Icon.Width = UiCfg.Inst.BaseFontSize*0.62;
						b.BtnContent = Icon;
						// 讀音旁的播放鈕比工具列按鈕再小一號，避免搶過詞頭。
						b._Button.Width = UiCfg.Inst.BaseFontSize*1.2;
						b._Button.Height = UiCfg.Inst.BaseFontSize*1.2;
						b._Button.HorizontalAlignment = HAlign.Left;
						b.HorizontalAlignment = HAlign.Left;
						b.VerticalAlignment = VAlign.Bottom;
						b.SetExe(Ct => vm.PlayHead(Ct));
					})
				,Txt()
					.With(t => {
						t.Text = P.Text;
						t.FontSize = UiCfg.Inst.BaseFontSize*0.92;
						t.Foreground = UiCfg.Inst.MutedTextBrush;
						t.VerticalAlignment = VAlign.Bottom;
						t.Margin = new Thickness(UiCfg.Inst.BaseFontSize*0.25, 0, UiCfg.Inst.GapItem*2, 0);
					})
		);
		return Row;
	}

	/// 義項列表：一項一塊，塊間距比塊內間距大一級。
	protected partial Control MkSenses(VmSimpleWord vm){
		var List = new ItemsControl()
			.With(l => _Senses = l)
			.ItemsSource(vm, x => x.Senses)
			.ItemsPanel(new StackPanel{
				Orientation = Orientation.Vertical,
				Spacing = UiCfg.Inst.GapSection,
			})
		;
		List.ItemTemplate = new FuncDataTemplate<DictSense>((Sense, _) => {
			if(Sense is null){
				return null;
			}
			return MkSenseBlock(Sense);
		});
		return List;
	}

	/// 單個義項：詞性標籤與釋義，其下縮排排列例句。
	protected partial Control MkSenseBlock(DictSense Sense){
		var Rows = new StackPanel(){ Orientation = Orientation.Vertical };
		if(!str.IsNullOrWhiteSpace(Sense.Pos)){
			Rows.Children(MkPosLabel(Sense.Pos));
		}
		Rows.Children(
			Txt()
				.With(t => {
					t.FontSize = UiCfg.Inst.BaseFontSize;
					t.LineHeight = UiCfg.Inst.BaseFontSize*1.6;
					t.Margin = new Thickness(0, 0, 0, UiCfg.Inst.GapItem*0.5);
				})
				.Text(Sense.Meaning)
		);
		if(Sense.Examples.Count > 0){
			Rows.Children(MkExamples(Sense));
		}
		return Rows;
	}

	/// 詞性標籤：主色小字＋輕微字距，不加底色也不加邊框（禁圓角的前提下，小方塊色塊會顯得髒）。
	protected partial TextBlock MkPosLabel(str Pos){
		return new TextBlock()
			.Text(Pos)
			.FontSize(UiCfg.Inst.BaseFontSize*0.78)
			.Foreground(UiCfg.Inst.MainColor)
			.LetterSpacing(0.6)
			.Margin(new Thickness(0, 0, 0, UiCfg.Inst.GapItem*0.5))
		;
	}

	/// 例句區：整體相對釋義縮排，例句原文用次級色、譯文用最弱一級。
	protected partial Control MkExamples(DictSense Sense){
		var List = new ItemsControl()
			.ItemsSource(Sense.Examples)
			.ItemsPanel(new StackPanel{
				Orientation = Orientation.Vertical,
				Spacing = UiCfg.Inst.GapItem,
			})
			.Margin(new Thickness(UiCfg.Inst.IndentContent, UiCfg.Inst.GapItem, 0, 0))
		;
		List.ItemTemplate = new FuncDataTemplate<SenseExample>((E, _) => {
			if(E is null){
				return null;
			}
			var Block = new StackPanel(){ Orientation = Orientation.Vertical };
			Block.Children(
				new SelectableTextBlock()
					.TextWrapping(TextWrapping.Wrap)
					.FontSize(UiCfg.Inst.BaseFontSize*0.88)
					.LineHeight(UiCfg.Inst.BaseFontSize*1.5)
					.Foreground(UiCfg.Inst.MutedTextBrush)
					.Text(E.Src)
			);
			if(!str.IsNullOrWhiteSpace(E.Dst)){
				Block.Children(
					new SelectableTextBlock()
						.TextWrapping(TextWrapping.Wrap)
						.FontSize(UiCfg.Inst.BaseFontSize*0.88)
						.LineHeight(UiCfg.Inst.BaseFontSize*1.5)
						.Foreground(UiCfg.Inst.FaintTextBrush)
						.Text(E.Dst)
				);
			}
			return Block;
		});
		return List;
	}

	/// 翻譯卡：原文（弱）／譯文（強調，左側主色豎線）／逐句對照／備註。
	/// *為何譯文用豎線而不是底色塊*：全應用禁圓角，色塊會出現四角很硬的方框；
	/// 一條 3px 的豎線足以把「結果」從「輸入」裏分出來，又不增加視覺重量。
	protected partial Control MkTranslateCard(VmSimpleWord vm){
		var Source = Txt()
			.With(t => {
				_SourceText = t;
				t.FontSize = UiCfg.Inst.BaseFontSize*0.92;
				t.LineHeight = UiCfg.Inst.BaseFontSize*1.55;
				t.Foreground = UiCfg.Inst.FaintTextBrush;
			})
			.Text(vm, x => x.SourceText)
			// 有逐句對照時不再重複整段原文：對照的每一行已含原文。
			.IsVisible(vm, x => x.ShowSourceText)
		;

		var Translation = new Border()
			.BorderBrush(UiCfg.Inst.MainColor)
			.BorderThickness(new Thickness(3, 0, 0, 0))
			.Padding(new Thickness(UiCfg.Inst.BaseFontSize*0.75, 0, 0, 0))
			.Child(
				Txt()
					.With(t => {
						_Translation = t;
						t.FontSize = UiCfg.Inst.BaseFontSize*1.3;
						t.LineHeight = UiCfg.Inst.BaseFontSize*1.65;
					})
					.Text(vm, x => x.Translation)
			)
		;

		var Card = new StackPanel(){ Orientation = Orientation.Vertical };
		Card.Margin = CardMargin();
		Card.Children(
				Source
				,Translation
					.Margin(new Thickness(0, UiCfg.Inst.GapSection, 0, 0))
				,MkSegments(vm)
					.IsVisible(vm, x => x.ShowSegments)
					.Margin(new Thickness(0, UiCfg.Inst.GapSection, 0, 0))
				,MkNotes(vm)
					.Margin(new Thickness(0, UiCfg.Inst.GapSection, 0, 0))
		);
		return Card;
	}

	/// 逐句對照：每句「原文弱化 ＋ 譯文正常」，句間距大一級。
	protected partial Control MkSegments(VmSimpleWord vm){
		var List = new ItemsControl()
			.With(l => _Segments = l)
			.ItemsSource(vm, x => x.Segments)
			.ItemsPanel(new StackPanel{
				Orientation = Orientation.Vertical,
				Spacing = UiCfg.Inst.GapSection,
			})
		;
		List.ItemTemplate = new FuncDataTemplate<TranslationSegment>((S, _) => {
			if(S is null){
				return null;
			}
			return new StackPanel()
				.Orientation(Orientation.Vertical)
				.Children(
					new SelectableTextBlock()
						.TextWrapping(TextWrapping.Wrap)
						.Foreground(UiCfg.Inst.FaintTextBrush)
						.FontSize(UiCfg.Inst.BaseFontSize*0.85)
						.LineHeight(UiCfg.Inst.BaseFontSize*1.45)
						.Text(S.Src)
					,new SelectableTextBlock()
						.TextWrapping(TextWrapping.Wrap)
						.FontSize(UiCfg.Inst.BaseFontSize)
						.LineHeight(UiCfg.Inst.BaseFontSize*1.55)
						.Text(S.Dst)
						.Margin(new Thickness(0, UiCfg.Inst.GapItem*0.4, 0, 0))
				)
			;
		});
		return List;
	}

	/// 術語備註：術語用次級色、說明用最弱一級，一行一條。
	protected partial Control MkNotes(VmSimpleWord vm){
		var List = new ItemsControl()
			.With(l => _Notes = l)
			.ItemsSource(vm, x => x.Notes)
			.ItemsPanel(new StackPanel{
				Orientation = Orientation.Vertical,
				Spacing = UiCfg.Inst.GapItem*0.5,
			})
		;
		List.ItemTemplate = new FuncDataTemplate<TranslationNote>((N, _) => {
			if(N is null){
				return null;
			}
			return new SelectableTextBlock()
				.TextWrapping(TextWrapping.Wrap)
				.FontSize(UiCfg.Inst.BaseFontSize*0.85)
				.LineHeight(UiCfg.Inst.BaseFontSize*1.45)
				.Foreground(UiCfg.Inst.FaintTextBrush)
				.Text($"{N.Term}：{N.Note}")
			;
		});
		return List;
	}

	/// 結果卡的統一外邊距：左右用頁面內距，底部留一級頁面內距。
	protected static Thickness CardMargin(){
		return new Thickness(
			UiCfg.Inst.PagePaddingH
			,0
			,UiCfg.Inst.PagePaddingH
			,UiCfg.Inst.PagePaddingV
		);
	}
}
