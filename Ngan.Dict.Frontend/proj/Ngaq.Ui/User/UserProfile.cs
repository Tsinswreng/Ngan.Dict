using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Shared.User.Models.Po.User;

namespace Ngaq.Ui.User {

public interface IUserProfile{
	public IdUser UserId{get;set;}
	public str? UniqName{get;set;}
	public str? NickName{get;set;}
	public str? Email{get;set;}
	public str? AvatarUrl{get;set;}
	public IImage? AvatarImg{get;set;}
	public void Clear(){
		UserId = default;
		UniqName = null;
		NickName = null;
		Email = null;
		AvatarUrl = null;
		AvatarImg = null;
	}
}

public class UserProfile:ObservableObject, IUserProfile {
	public IdUser UserId{
		get;
		set{SetProperty(ref field, value);}
	}
	public str? UniqName{
		get;
		set{SetProperty(ref field, value);}
	}
	public str? NickName{
		get;
		set{SetProperty(ref field, value);}
	}
	public str? Email{
		get;
		set{SetProperty(ref field, value);}
	}
	public str? AvatarUrl{
		get;
		set{SetProperty(ref field, value);}
	}
	public IImage? AvatarImg{
		get;
		set{SetProperty(ref field, value);}
	}
}

public static class ExtnFrontendUserCtx{
	extension<T>(T z)
		where T:IFrontendUserCtx
	{
		//TODO 直把UserProfile 加進 IFrontendUserCtx 作其成員。少用 Props
		public IUserProfile? UserProfile{
			get{
				return z?.Props?["UserProfile"] as IUserProfile;
			}set{
				z?.Props?["UserProfile"] = value;
			}
		}
	}
}


}//~Ns
