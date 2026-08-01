
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Ngaq.Core.Shared.Audio;

namespace Ngaq.Core.Android.Audio;

public class AndroidAudioPlayer : IAudioPlayer {
	public async Task<IPlayState?> Play(System.IO.Stream stream, EAudioType audioType, CT Ct) {
		try{
			// 确定文件扩展名
			string extension = audioType switch {
				EAudioType.Mp3 => ".mp3",
				EAudioType.Wav => ".wav",
				_ => throw ToolAudioErr.MkAudioPlayFailedErr(
					null,
					$"UnsupportedAudioType={audioType}",
					"Platform=Android"
				)
			};

			// 保存 Stream 到临时文件
			string tempFilePath = Path.Combine(Application.Context.CacheDir.AbsolutePath, Guid.NewGuid().ToString() + extension);
			await using (var fileStream = File.Create(tempFilePath)) {
				await stream.CopyToAsync(fileStream, Ct);
			}

			var mediaPlayer = new MediaPlayer();
			var tcs = new TaskCompletionSource<bool>();

			// 注册准备完成事件
			EventHandler? preparedHandler = null;
			EventHandler<MediaPlayer.ErrorEventArgs>? errorHandler = null;

			preparedHandler = (sender, args) => {
				mediaPlayer.Prepared -= preparedHandler;
				mediaPlayer.Error -= errorHandler;
				mediaPlayer.Start();
				tcs.TrySetResult(true);
			};

			errorHandler = (sender, args) => {
				mediaPlayer.Prepared -= preparedHandler;
				mediaPlayer.Error -= errorHandler;
				mediaPlayer.Release();
				tcs.TrySetException(new InvalidOperationException($"MediaPlayer error: {args.What}, {args.Extra}"));
			};

			mediaPlayer.Prepared += preparedHandler;
			mediaPlayer.Error += errorHandler;

			try {
				// 设置数据源并异步准备
				mediaPlayer.SetDataSource(tempFilePath);
				mediaPlayer.PrepareAsync();

				// 等待准备完成或取消
				using (Ct.Register(() => {
					tcs.TrySetCanceled(Ct);
				})) {
					await tcs.Task;
				}
			}catch{
				mediaPlayer.Release();
				try { File.Delete(tempFilePath); } catch { }
				throw;
			}

			// 注册播放完成后的清理
			mediaPlayer.Completion += (sender, args) => {
				mediaPlayer.Release();
				try { File.Delete(tempFilePath); } catch { /* 忽略删除失败 */ }
			};

			return null; // 按需求返回 null
		}catch(OperationCanceledException) when(Ct.IsCancellationRequested){
			throw;
		}catch(Exception ex) when(ex is not Tsinswreng.CsErr.AppErr){
			throw ToolAudioErr.MkAudioPlayFailedErr(ex, $"Type={audioType}", "Platform=Android");
		}
	}
}
