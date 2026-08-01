using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Ngaq.Core.Infra;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsCore;

namespace Ngaq.Ui.Components.TempusBox;

public partial interface ITempusFormatItem{
	[Doc(@$"該種格式的名稱 在下拉框中顯示")]
	public str FmtDisplayName{get;set;}
	public IValueConverter Converter{get;set;}
}

public partial class TempusFormatItem:ITempusFormatItem{
	public str FmtDisplayName{get;set;} = "";
	public IValueConverter Converter{get;set;} = null!;

	[Doc(@$"顯示本地時區。
	#Example([2026-04-07T23:16:07.344+08:00])
	")]
	public static TempusFormatItem Iso8601Full{get;set;}
	[Doc(@$"
	#Examples([1775575237143])
	")]
	public static TempusFormatItem UnixMs{get;set;}
	[Doc(@$"
	#Examples([26-04-07])
	")]
	public static TempusFormatItem yy_MM_DD{get;set;}
	[Doc(@$"
	#Examples([26-04-07 12:34])
	")]
	public static TempusFormatItem yy_MM_DD__HH_mm{get;set;}



}
