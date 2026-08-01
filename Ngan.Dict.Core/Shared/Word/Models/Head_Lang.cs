namespace Ngaq.Core.Shared.Word.Models;

public partial record struct Head_Lang(
	str Head
	,str Lang
){

}

// public partial struct Head_Lang()
// 	:IEquatable<Head_Lang>
// {
// 	public str Head = "";
// 	public str Lang = "";

// 	public override bool Equals(object? obj){
// 		return obj is Head_Lang other && Equals(other);
// 	}

// 	public bool Equals(Head_Lang other){
// 		return string.Equals(Head, other.Head) && string.Equals(Lang, other.Lang);
// 	}

// 	public override int GetHashCode() {
// 		unchecked{
// 			int hash = 17;
// 			hash = hash * 23 + (Head != null ? Head.GetHashCode() : 0);
// 			hash = hash * 23 + (Lang != null ? Lang.GetHashCode() : 0);
// 			return hash;
// 		}
// 	}

// }
