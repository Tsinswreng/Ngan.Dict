namespace Ngaq.Ui.Views.Word.WordManage.SearchWords.SearchedWordCard;

using Avalonia.Controls;
using Avalonia.Media;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Tools;
using Ngaq.Ui.Converters;
using Ngaq.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsTools;
using Ctx = VmSearchedWordCard;

public partial class ViewSearchedWordCard{
	public partial ViewSearchedWordCard(){Ctx = new Ctx(); InitStyle(); Render();}
	partial void InitStyle(){
		Styles.A(Sty.Is<TextBlock>().Set(x=>x.Effect, new DropShadowDirectionEffect{Color = Colors.Black, BlurRadius = 4, ShadowDepth = 4, Direction = 330, Opacity = 0.5}));
	}
	partial void Render(){
		this.SetContent(Root.Grid);
		Root.SetRowDefs([new(3, GUT.Auto), new(8, GUT.Auto)]);
		var LangGrid = new GridStack(IsRow: false);
		LangGrid.Grid.SetColDefs([new(1, GUT.Star), new(2, GUT.Star)]);
		Root.A(LangGrid.Grid);
		LangGrid.A(new TextBlock(), o=>{LangCtrl = o; o.VAlign(x=>x.Center); o.CBind<Ctx>(o.PropText, x=>x.Lang); o.Foreground = Brushes.LightGray;}).A(MkInfoGrid());
		var HeadBox = new GridStack(IsRow: false);
		HeadBox.Grid.SetColDefs([new(1, GUT.Star)]);
		Root.A(HeadBox.Grid);
		HeadBox.A(new TextBlock(), o=>{
			HeadCtrl = o; o.VAlign(x=>x.Center); o.FontSize = UiCfg.Inst.BaseFontSize + 8;
			Ctx.Bind(o, x=>x.TextDecorations, x=>x.DelAt, Converter: new FnConvtr<IdDel?, TextDecorationCollection?>(x=>x.IsNullOrDefault() ? null : TextDecorations.Strikethrough));
			Ctx.Bind(o, x=>x.Text, x=>x.Head); Ctx.Bind(o, x=>x.Foreground, x=>x.FontColor);
		});
	}
	private partial Control MkInfoGrid(){
		var Grid = new GridStack(IsRow: false);
		Grid.Grid.SetColDefs([new(1,GUT.Auto),new(1,GUT.Auto),new(1,GUT.Auto),new(1,GUT.Auto),new(1,GUT.Auto),new(1,GUT.Auto),new(1,GUT.Auto),new(1,GUT.Auto),new(1,GUT.Auto),new(1,GUT.Auto)]);
		InfoCtrl = Grid.Grid;
		var RecordType = (ELearn Learn)=>{var Text = new TextBlock(); Ctx.Bind(Text, x=>x.Text, x=>x.Learn_Records, Converter: new ConvMultiDictValueCnt<ELearn, ILearnRecord>(), ConverterParameter: Learn); return Text;};
		Grid.A(new TextBlock(), o=>Ctx.Bind(o, x=>x.Text, x=>x.SavedLearnRecords, Converter: new FnConvtr<IList<ILearnRecord>,str>((x,_)=>x.Count > 0 ? Ctx.LearnToSymbol(x[^1].Learn) : ""), ConverterParameter: Ctx))
			.A(RecordType(ELearn.Add)).A(new TextBlock{Text=":"}).A(RecordType(ELearn.Rmb)).A(new TextBlock{Text=":"}).A(RecordType(ELearn.Fgt)).A(new TextBlock{Text="\t"})
			.A(new TextBlock(), o=>Ctx.Bind(o, x=>x.Text, x=>x.LastLearnedTime, Converter: new FnConvtr<i64,str>(x=>Ctx.FormatUnixMsDiff(DateTimeOffset.Now.ToUnixTimeMilliseconds()-x))))
			.A(new TextBlock{Text="\t"}).A(new TextBlock(), o=>Ctx.Bind(o, x=>x.Text, x=>x.Weight, Converter: new FnConvtr<f64?,str>((x,_)=>Ctx.FmtNum(x??0,2))));
		return Grid.Grid;
	}
}
