namespace Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Word.Models.Dto;



public partial class DtoSyncWords{
	/// 庫中未有之待添之諸新詞 若從文本詞表則按description數量決定'add'ˉLeanrnRecord之數
	public IList<IJnWord> NeoWords{get;set;} = new List<IJnWord>();
	/// 待更新之諸詞
	public IList<DtoUdpWord> UpdatedWords{get;set;} = new List<DtoUdpWord>();
}

public class DtoUdpWord{
	public IJnWord? WordInDb{get;set;}
	public DtoSyncTwoWords DtoSyncTwoWords{get;set;} = new();
}


#region Old


[Obsolete]
public partial class DtoUpdWordOld2{
	public DtoUpdWordOld2(
		IJnWord WordInDb
		,IJnWord NeoPart
		,IJnWord ChangedPart
	){
		this.WordInDb = WordInDb;
		this.NeoPart = NeoPart;
		this.ChangedPart = ChangedPart;
	}
	/// 庫中既有之詞
	public IJnWord WordInDb{get;set;}
	/// 待添之詞(NeoPart)
	public IJnWord? NeoPart{get;set;}
	/// WordToAdd有洏WordInDb無之屬性 按description數量決定'add'ˉLeanrnRecord之數

	/// WordToAdd 與 WordInDb 之差集 (ChangedPart)
	//public DtoWordDiff DtoWordDiff{get;set;}
	public IJnWord? ChangedPart{get;set;}

}

[Obsolete]
public partial class DtoSyncWordsOld2{
	/// 庫中未有之待添之諸新詞 按description數量決定'add'ˉLeanrnRecord之數
	public IList<IJnWord> NeoWords{get;set;} = new List<IJnWord>();
	/// 待更新之諸詞
	public IList<DtoUpdWordOld2> UpdatedWords{get;set;} = new List<DtoUpdWordOld2>();
}





public partial class DtoUpdWordOld{
	public DtoUpdWordOld(
		IJnWord WordInDb
		,IJnWord WordToAdd
		,IJnWord DiffedWord
	){
		this.WordInDb = WordInDb;
		this.WordToAdd = WordToAdd;
		this.DiffedWord = DiffedWord;
	}
	/// 庫中既有之詞
	public IJnWord WordInDb{get;set;}
	/// 待添之詞
	public IJnWord WordToAdd{get;set;}
	/// WordToAdd有洏WordInDb無之屬性 按description數量決定'add'ˉLeanrnRecord之數

	/// WordToAdd 與 WordInDb 之差集
	//public DtoWordDiff DtoWordDiff{get;set;}
	public IJnWord DiffedWord{get;set;}

}

public partial class DtoAddWordsOld{
	/// 庫中未有之待添之諸新詞 按description數量決定'add'ˉLeanrnRecord之數
	public IList<IJnWord> NeoWords{get;set;} = new List<IJnWord>();
	/// 待更新之諸詞
	public IList<DtoUpdWordOld> UpdatedWords{get;set;} = new List<DtoUpdWordOld>();
}

#endregion
