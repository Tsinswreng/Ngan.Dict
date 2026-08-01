namespace Ngaq.Core.Shared.Base.Models.Req;

using Ngaq.Core.Infra.IF;

//參數對象、多用于服務類之參數DTO 未必潙httpʹ請求對象
public interface IReq:IAppSerializable{
	public nil Validate(){
		return NIL;
	}

}

public static class ExtnIReq{
	public static nil Validate(this IReq req){
		return req.Validate();
	}
}
