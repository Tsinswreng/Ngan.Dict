using Ngaq.Core.Iter;

namespace Ngaq.Core.Tools.Io;

public  partial class StrByteReader: IIterable<u8>{
	public str Str{get;set;}
	public u8[] Bytes{get;set;}
	public u64 Pos{get;set;}=0;
	public StrByteReader(str s){
		this.Str = s;
		this.Bytes = System.Text.Encoding.UTF8.GetBytes(s);
	}

	public bool HasNext(){
		return Pos < (u64)Bytes.Length;
	}

	public u8 Next(){
		return Bytes[Pos++];
	}
}


// using System.Collections;
// using Ngaq.Core.Stream;

// namespace Ngaq.Core.Util.Io;

// public  partial class StrByteReader
// 	:I_Iter<u8>
// 	,IEnumerable<u8>
// 	,IEnumerator<u8>
// {
// 	public str Str{get;set;}
// 	public u8[] Bytes{get;set;}
// 	public u64 Pos{get;set;}=0;

// 	public u8 Current{
// 		get;protected set;
// 	}

// 	object IEnumerator.Current => Current;

// 	public StrByteReader(str s){
// 		this.Str = s;
// 		this.Bytes = System.Text.Encoding.UTF8.GetBytes(s);
// 	}

// 	public bool HasNext(){
// 		return Pos < (u64)Bytes.Length;
// 	}

// 	public u8 Next(){
// 		MoveNext();
// 		return Current;
// 	}

// 	public bool MoveNext() {
// 		var hasNext = HasNext();
// 		if (hasNext) {
// 			Current = Bytes[Pos++];
// 		}
// 		return hasNext;
// 	}

// 	public void Reset() {
// 		Pos = 0;
// 	}

// 	public void Dispose() {

// 	}

// 	public IEnumerator<byte> GetEnumerator() {
// 		return new StrByteReader(Str);
// 	}

// 	IEnumerator IEnumerable.GetEnumerator() {
// 		return GetEnumerator();
// 	}
// }
