namespace Ngaq.Core.Shared.Audio;

public static class ExtnAudioPlayer{
	extension(IAudioPlayer z){
		public async Task<IPlayState?> Play(Audio Audio, CT Ct){
			var stream = await Audio.MkrData(Ct);
			return await z.Play(stream, Audio.Type, Ct);
		}
	}
}
