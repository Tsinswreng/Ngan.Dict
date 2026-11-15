namespace Ngan.Dict.Core.Infra;

using Tsinswreng.CsCore;
using Tsinswreng.CsErr.Results;

[Doc($$"""
{{nameof(IAnswer<T>)}} 的引用型實現。

*為何不用庫裏的 {{nameof(Answer<T>)}}*：那是 `struct`，而錯誤收集用的是
`AddErr<TSelf>(this TSelf, obj) where TSelf : class`——值型別過不了引用約束。
本類是同一個契約的 class 版，於是 `new AnswerBox<T>().AddErr(e)` 這種慣用寫法照舊成立。

*與庫的關係*：本類不新增語義，成員與 {{nameof(Answer<T>)}} 一一對應；
`OkWith`／`DataOrThrow` 這些庫擴展掛在 {{nameof(IAnswer<T>)}} 上，對本類同樣有效。
""")]
public sealed class AnswerBox<T> : IAnswer<T> {
	public T? Data { get; set; }
	public bool Ok { get; set; }
	public IList<obj?> Errors { get; set; } = new List<obj?>();
}
