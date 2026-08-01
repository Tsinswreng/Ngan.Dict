namespace Ngaq.Core.Shared.Word.Models.Dto;

using Ngaq.Core.Shared.Base.Models.Req;
using Tsinswreng.CsPage;
using Tsinswreng.CsTempus;

public class ReqScltWordsOfLearnResultByTimeInterval:IReq{
	public UnixMs TimeStart{get;set;}
	public UnixMs TimeEnd{get;set;}
	public UnixMs TimeInterval{get;set;}
	public str LearnResult{get;set;}
	public IPageQry PageQry{get;set;}
}

