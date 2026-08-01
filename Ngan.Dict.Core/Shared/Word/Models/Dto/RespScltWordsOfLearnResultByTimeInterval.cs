namespace Ngaq.Core.Shared.Word.Models.Dto;

using Ngaq.Core.Shared.Base.Models.Resp;
using Tsinswreng.CsPage;
using Tsinswreng.CsTempus;

public class TimeIntervalCnt{
	public UnixMs TimeStart{get;set;}
	public UnixMs TimeEnd{get;set;}
	public i64 Cnt{get;set;}
}

public class RespScltWordsOfLearnResultByTimeInterval:IResp{
	public IPageAsyE<TimeIntervalCnt> IntervalPage{get;set;}
}

