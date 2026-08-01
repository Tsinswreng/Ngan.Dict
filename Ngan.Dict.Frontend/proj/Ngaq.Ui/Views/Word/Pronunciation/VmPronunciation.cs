namespace Ngaq.Ui.Views.Word.Pronunciation;

using System.Collections.ObjectModel;
using Ngaq.Core.Shared.Audio;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Ui.Infra;
using Pronunciation = Ngaq.Core.Shared.Dictionary.Models.Pronunciation;
using Ctx = VmPronunciation;
using Ngaq.Core.Shared.Dictionary.Models;

public partial class VmPronunciation: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmPronunciation(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmPronunciation(){
		#if DEBUG
		{
			var o = App.DiOrMk<Ctx>();
			o.FromPronunciation(Pronunciation.Sample.Samples[0]);
			Samples.Add(o);
		}
		#endif
	}

	IAudioPlayer? AudioPlayer;
	public VmPronunciation(IAudioPlayer? AudioPlayer){
		this.AudioPlayer = AudioPlayer;
	}

	public Ctx FromPronunciation(Pronunciation P){
		Raw = P;
		Text = P.Text;
		return this;
	}

	public Pronunciation? Raw{get;set;}

	public str Text{
		get;
		set{SetProperty(ref field, value);}
	}="";

	public async Task<nil> Play(CT Ct){
		if(AnyNull(Raw, AudioPlayer)){
			return NIL;
		}
		await Task.Run(async()=>{
			if(Raw.Audio is null){
				await Raw.AssignAudioFromUrl(Ct);
			}
			if(Raw.Audio is null){
				return;
			}
			await AudioPlayer.Play(Raw.Audio, Ct);
		}, Ct);
		return NIL;
	}

	public async Task<nil> TryDelay(CT Ct){
		await Task.Delay(50000, Ct);
		return NIL;
	}


}
