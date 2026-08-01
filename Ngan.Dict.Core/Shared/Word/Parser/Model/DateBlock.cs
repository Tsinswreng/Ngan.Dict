namespace Ngaq.Core.Service.Parser.Model;
public  partial interface I_DateBlock{
	public I_StrSegment Date{get;set;}
	public IList<I_WordBlock> Words{get;set;}

	public IList<I_Prop> Props{get;set;}
}

public  partial struct DateBlock:I_DateBlock{
	//protected DateBlock(){}
	public DateBlock(){}//不寫此則不初始化ᵣ直ᵈ賦值之字段ˇ
	public DateBlock(I_StrSegment date){
		this.Date = date;
	}
	public I_StrSegment Date{get;set;}
	public IList<I_WordBlock> Words{get;set;} = new List<I_WordBlock>();

	public IList<I_Prop> Props{get;set;} = new List<I_Prop>();
}
