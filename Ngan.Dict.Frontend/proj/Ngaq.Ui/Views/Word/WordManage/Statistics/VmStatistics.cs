namespace Ngaq.Ui.Views.Word.WordManage.Statistics;

using System.Collections.ObjectModel;
using Avalonia.Threading;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Infra;
using ScottPlot;
using Tsinswreng.CsCore;
using Tsinswreng.CsPage;
using Tsinswreng.CsTempus;
using Tsinswreng.CsI18n;
using Ctx = VmStatistics;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class VmStatistics: ViewModelBase{
	void Init(){
		PageBar = VmPageBar.Mk();
		PageBar.PageSize = 10;
		PageBar.FnPrevPage = OnPrevPage;
		PageBar.FnNextPage = OnNextPage;
	}

	public enum ETimeUnit{
		Second,
		Minute,
		Hour,
		Day,
		Week,
		Month,
		Year,
	}
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmStatistics(){
		Init();
	}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmStatistics(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}


	ISvcWord? SvcWord;
	public VmStatistics(
		ISvcWord? SvcWord
	):this(){
		this.SvcWord = SvcWord;
	}


	public VmPageBar PageBar{get;set;} = null!;

	public u64 PageIdx{
		get{
			if(PageBar.PageNum == 0){
				return 0;
			}
			return PageBar.PageNum-1;
		}
		set{PageBar.PageNum = value+1;}
	}

	public u64 PageSize{
		get{return PageBar.PageSize;}
		set{PageBar.PageSize = value;}
	}

	public UnixMs TimeStart{
		get;
		set{SetProperty(ref field, value);}
	}=UnixMs.FromDateTime(DateTime.Now.AddMonths(-2));

	public UnixMs TimeEnd{
		get;
		set{SetProperty(ref field, value);}
	}=new UnixMs();

	public i64 IntervalNoUnit{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(TimeInterval));
			}
		}
	}=1;

	public ETimeUnit IntervalUnit{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(TimeInterval));
			}
		}
	}=ETimeUnit.Week;

	public UnixMs TimeInterval{
		get{return ValueUnitToTempus(IntervalNoUnit, IntervalUnit);}
		//set{SetProperty(ref field, value);}
	}

	static readonly IReadOnlyList<ELearn> LearnResultValues = [
		ELearn.Add,
		ELearn.Rmb,
		ELearn.Fgt,
	];

	public IReadOnlyList<str> LearnResultOptions => [
		I18n[K.Learn_Add],
		I18n[K.Learn_Rmb],
		I18n[K.Learn_Fgt],
	];

	public i32 LearnResultIndex{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(LearnResult));
			}
		}
	}=0;

	public str LearnResult{
		get{return GetLearnResultByIndex(LearnResultIndex).ToString();}
	}


	// public IList<f64> Times{
	// 	get;
	// 	set{SetProperty(ref field, value);}
	// }=[];

	// public IList<f64> Cnts{
	// 	get;
	// 	set{SetProperty(ref field, value);}
	// }=[];

	public static UnixMs ValueUnitToTempus(i64 Value, ETimeUnit Unit){
		return Unit switch{
			ETimeUnit.Second => new UnixMs(Value*InMillisecond.Second),
			ETimeUnit.Minute => new UnixMs(Value*InMillisecond.Minute),
			ETimeUnit.Hour => new UnixMs(Value*InMillisecond.Hour),
			ETimeUnit.Day => new UnixMs(Value*InMillisecond.Day),
			ETimeUnit.Week => new UnixMs(Value*InMillisecond.Week),
			ETimeUnit.Month => new UnixMs(Value*InMillisecond.Month),
			ETimeUnit.Year => new UnixMs(Value*InMillisecond.Year),
			_ => throw new Exception("Invalid TimeUnit"),
		};
	}

	static ELearn GetLearnResultByIndex(i32 Index){
		if(Index < 0 || Index >= LearnResultValues.Count){
			return ELearn.Add;
		}
		return LearnResultValues[Index];
	}

	public List<Coordinates> Points{
		get;
		set{SetProperty(ref field, value);}
	}=[];

	public event EventHandler? GraphChanged;

	public async Task<nil> GetDataAsy(CT Ct){
		if(AnyNull(SvcWord)){
			return NIL;
		}
		var Req = new ReqScltWordsOfLearnResultByTimeInterval{
			TimeStart = TimeStart,
			TimeEnd = TimeEnd,
			LearnResult = LearnResult,
			TimeInterval = TimeInterval,
			PageQry = PageBar.ToPageQry(),
		};
		Req.PageQry.WantTotCnt = true;
		await Task.Run(async()=>{
			var Resp = await SvcWord.ScltAddedWordsByTimeInterval(Req, Ct);
			var Intervals = await Resp.IntervalPage.DataAsyE.OrEmpty().ToListAsync(Ct);
			Dispatcher.UIThread.Post(()=>{
				PageBar.FromPageResultInfo(Resp.IntervalPage);
				Points.Clear();
				foreach(var Interval in Intervals){
					Points.Add(new Coordinates(Interval.TimeStart, Interval.Cnt));
				}
				GraphChanged?.Invoke(this, EventArgs.Empty);
			});

		});
		return NIL;
	}

	protected async Task<nil> OnPrevPage(VmPageBar PageBar, CT Ct){
		if(PageBar.PageNum <= 1){
			PageBar.PageNum = 1;
			return NIL;
		}
		PageBar.PageNum--;
		return await GetDataAsy(Ct);
	}

	protected async Task<nil> OnNextPage(VmPageBar PageBar, CT Ct){
		if(PageBar.TotPageCnt is u64 TotPageCnt && TotPageCnt > 0 && PageBar.PageNum >= TotPageCnt){
			return NIL;
		}
		PageBar.PageNum++;
		return await GetDataAsy(Ct);
	}

}
