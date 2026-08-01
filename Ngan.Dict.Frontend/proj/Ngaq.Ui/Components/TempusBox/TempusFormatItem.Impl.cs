using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Ngaq.Core.Infra;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsCore;
using Tsinswreng.CsTempus;

namespace Ngaq.Ui.Components.TempusBox;

public partial class TempusFormatItem{
	static TempusFormatItem(){
		Iso8601Full = new TempusFormatItem{
			FmtDisplayName = "ISO 8601",
			Converter = MkIsoLocalConverter(),
		};
		UnixMs = new TempusFormatItem{
			FmtDisplayName = "Unix ms",
			Converter = MkUnixMsConverter(),
		};
		yy_MM_DD = new TempusFormatItem{
			FmtDisplayName = "yy-MM-dd",
			Converter = MkDateTimePatternConverter("yy-MM-dd"),
		};
		yy_MM_DD__HH_mm = new TempusFormatItem{
			FmtDisplayName = "yy-MM-dd HH:mm",
			Converter = MkDateTimePatternConverter("yy-MM-dd HH:mm"),
		};
	}

	static IValueConverter MkIsoLocalConverter(){
		return new FnConvtr<obj?, obj?>(
			(v, p)=>{
				if(v is UnixMs t){
					return DateTimeOffset.FromUnixTimeMilliseconds(t.Value)
					.ToLocalTime()
					.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
				}
				return BindingNotification.UnsetValue;
			},
			(v, p)=>{
				if(v is str s && DateTimeOffset.TryParseExact(
					s,
					"yyyy-MM-ddTHH:mm:ss.fffzzz",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out var dto
				)){
					return Tsinswreng.CsTempus.UnixMs.FromUnixMs(dto.ToUnixTimeMilliseconds());
				}
				return BindingNotification.UnsetValue;
			}
		);
	}

	static IValueConverter MkUnixMsConverter(){
		return new FnConvtr<obj?, obj?>(
			(v, p)=>{
				if(v is UnixMs t){
					return t.Value.ToString(CultureInfo.InvariantCulture);
				}
				return BindingNotification.UnsetValue;
			},
			(v, p)=>{
				if(v is str s && i64.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms)){
					return Tsinswreng.CsTempus.UnixMs.FromUnixMs(ms);
				}
				return BindingNotification.UnsetValue;
			}
		);
	}

	static IValueConverter MkDateTimePatternConverter(str Pattern){
		var fmt = NormalizePattern(Pattern);
		return new FnConvtr<obj?, obj?>(
			(v, p)=>{
				if(v is UnixMs t){
					return DateTimeOffset.FromUnixTimeMilliseconds(t.Value)
					.ToLocalTime()
					.DateTime
					.ToString(fmt, CultureInfo.InvariantCulture);
				}
				return BindingNotification.UnsetValue;
			},
			(v, p)=>{
				if(v is str s && DateTime.TryParseExact(
					s,
					fmt,
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out var dt
				)){
					return Tsinswreng.CsTempus.UnixMs.FromDateTime(new DateTime(
						dt.Year, dt.Month, dt.Day,
						dt.Hour, dt.Minute, dt.Second,
						dt.Millisecond,
						DateTimeKind.Local
					));
				}
				return BindingNotification.UnsetValue;
			}
		);
	}

	static str NormalizePattern(str Pattern){
		return Pattern
		.Replace("YYYY", "yyyy")
		.Replace("YY", "yy")
		.Replace("DD", "dd");
	}
}

