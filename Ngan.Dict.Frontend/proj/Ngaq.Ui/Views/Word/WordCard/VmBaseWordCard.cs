namespace Ngaq.Ui.Views.Word.WordCard;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Ui.Infra;
using Tsinswreng.CsErr;
using Ctx = VmBaseWordListCard;
public partial class VmBaseWordListCard
	:ViewModelBase
{
	public static ObservableCollection<Ctx> Samples = [];
	static VmBaseWordListCard(){
		#if DEBUG
		// {
		// 	var o = new Ctx();
		// 	Samples.Add(o);
		// 	o.FromJnWord(SampleWord.Inst.Samples[0]);
		// }
		// {
		// 	var o = new Ctx();
		// 	Samples.Add(o);
		// 	o.FromJnWord(SampleWord.Inst.Samples[1]);
		// }
		#endif
	}

	public Ctx InitFromIWordForLearn(IWordForLearn Word){
		this.WordForLearn = Word;
		Init();
		return this;
	}

	protected virtual nil Init(){
		try{
			if(Bo == null){
				return NIL;
			}
			FontColor = Brushes.White;
			Head = Bo.Head;
			Lang = Bo.Lang;
			Index = Bo.Index;
			Weight = Bo.Weight;
			Learn_Records = Bo.Learn_Records;
			SavedLearnRecords = Bo.LearnRecords;
			LastLearnedTime = Bo.LastLearnedTime_();
			if(Learn_Records.TryGetValue(ELearn.Add, out var AddRecords)){
				FontColor = AddCntToFontColor(
					(u64)AddRecords.Count
				);
			}

			return NIL;
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	protected void OnBoChanged(object? sender, PropertyChangedEventArgs e){
		Init();
	}

	public IWordForLearn? WordForLearn{
		get;
		set{
			ForceSetProp(ref field, value);
			if(value == null){return;}
			field!.PropertyChanged += OnBoChanged;
		}
	}

	protected IWordForLearn? Bo{
		get{return WordForLearn;}
		set{WordForLearn = value;}
	}

	public str Head{
		get;
		set{
			if(Bo!=null){Bo.Head = value;}
			SetProperty(ref field, value);
		}
	} = "";

	public str Lang{
		get;
		set{
			if(Bo!=null){Bo.Lang = value;}
			SetProperty(ref field, value);
		}
	} = "";

	public u64? Index{
		get;
		set{
			if(Bo!=null){Bo.Index = value;}
			SetProperty(ref field, value);
		}
	}


	public f64? Weight{
		get;
		set{
			if(Bo!=null){Bo.Weight = value;}
			SetProperty(ref field, value);
		}
	}

	public IDictionary<ELearn, IList<ILearnRecord>> Learn_Records{
		get;
		set{
			if(Bo!=null){Bo.Learn_Records = value;}
			ForceSetProp(ref field, value);
			// SetProperty(ref _Learn_Records, value);
			// OnPropertyChanged(nameof(Learn_Records));
		}
	} = new Dictionary<ELearn, IList<ILearnRecord>>();

	public IList<ILearnRecord> SavedLearnRecords{
		get;
		set{
			if(Bo!=null){Bo.LearnRecords = value;}
			//OnPropertyChanged(nameof(field));
			ForceSetProp(ref field, value);
		}
	}= new List<ILearnRecord>();

	public i64 LastLearnedTime{
		get;
		set{SetProperty(ref field, value);}
	} = 0;


	public IBrush LearnedColor{
		get;
		set{SetProperty(ref field, value);}
	}=Brushes.Transparent;

	public IBrush FontColor{
		get;
		set{SetProperty(ref field, value);}
	} = Brushes.White;



	public static str LearnToSymbol(ELearn Learn){
		if(Learn == ELearn.Add){
			return "🤔";
		}else if(Learn == ELearn.Rmb){
			return "✅";
		}else if(Learn == ELearn.Fgt){
			return "❌";
		}
		throw new FatalLogicErr("Unknown Learn: " + Learn);
	}

	public static IBrush AddCntToFontColor(u64 Cnt){
		var rgb = (u8 r,u8 g,u8 b)=>{
			return new SolidColorBrush(Color.FromRgb(r,g,b));
		};
		if(Cnt <= 1){
			return Brushes.White;
		}else if(Cnt == 2){
			return rgb(0,255,128);
		}else if(Cnt == 3){
			return rgb(0,255,255);
		}else if(Cnt == 4){
			return rgb(255,255,0);
		}else if(Cnt == 5){
			return rgb(200, 100, 255);
		}else{
			return rgb(255,50,50);
		}
	}

	public static str FormatUnixMsDiff(
		i64 MsDiff
	){
		var ToI64 = (f64 N)=>{
			return (i64)N;
		};
		var S = MsDiff/1000.0;
		var M = S/60.0;
		if(M < 1){
			return ToI64(S)+"s";
		}
		var H = M/60.0;
		if(H < 1){
			return ToI64(M)+"m";
		}
		var D = H/24.0;
		if(H < 100){
			return ToI64(H)+"h";
		}
		return ToI64(D)+"d";
	}


	public static string FmtNum(double num, int fix){
		if(f64.IsInfinity(num)){
			return "∞";
		}
		string exp = num.ToString("e");  // 例如 "1.030000e+003"
		var parts = exp.Split('e');      // ["1.030000", "+003"]

		double baseValue = double.Parse(parts[0]);
		baseValue = Math.Round(baseValue, fix);
		string baseStr = baseValue.ToString($"F{fix}");

		string expPart = parts[1];       // "+003" or "-004"

		// 提取符号和数字部分
		char sign = expPart[0];
		string digits = expPart.Substring(1);

		// 转成整数去掉前导0
		int expNum = int.Parse(digits);

		// 拼接指数，去掉多余的零，比如 "e+003" -> "e3"
		string result = baseStr + "e" + (sign == '+' ? "" : "-") + expNum.ToString();

		return result;
	}

	public str ToLearnHistoryRepr(){
		var z = this;
		if(z.Bo is null){
			return "";
		}
		var R = new List<str>();
		if(z.SavedLearnRecords?.Count > 0){
			var LearnSymbol = LearnToSymbol(z.SavedLearnRecords[^1].Learn);
			R.Add(LearnSymbol);
		}

		R.Add(  (GetValueOrDefault(z.Learn_Records!, ELearn.Add, null)?.Count??0) + ""  );
		R.Add(":");
		R.Add(  (GetValueOrDefault(z.Learn_Records!, ELearn.Rmb, null)?.Count??0) + ""  );
		R.Add(":");
		R.Add(  (GetValueOrDefault(z.Learn_Records!, ELearn.Fgt, null)?.Count??0) + ""  );
		return string.Join("",R);
	}

	public static TValue? GetValueOrDefault<TKey, TValue>(
		IDictionary<TKey, TValue?> dict,
		TKey key,
		TValue defaultValue = default!
	){
		return dict.TryGetValue(key, out var v) ? v : defaultValue;
	}


}
