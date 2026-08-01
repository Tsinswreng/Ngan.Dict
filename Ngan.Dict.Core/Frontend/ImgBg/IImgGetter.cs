namespace Ngaq.Core.Frontend.ImgBg;

using Tsinswreng.CsTools;

[Obsolete]
public partial interface IImgGetter{
public enum EType{
	Stream = 1
}
	IEnumerable<ITypedObj> GetN(u64 n);
}


