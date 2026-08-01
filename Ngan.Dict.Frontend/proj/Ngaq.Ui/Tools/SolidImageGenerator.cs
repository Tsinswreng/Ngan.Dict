namespace Ngaq.Ui.Tools;
using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

public static class SolidImageGenerator {

	/// 生成一张纯色 WriteableBitmap（Rgba8888 格式）。

	public static WriteableBitmap Create(int width, int height, Color color) {
		var bmp = new WriteableBitmap(
			new PixelSize(width, height),
			new Vector(96, 96),
			PixelFormat.Rgba8888,
			AlphaFormat.Unpremul);

		using var fb = bmp.Lock();
		unsafe {
			byte* ptr = (byte*)fb.Address;
			int row = fb.RowBytes;
			int bpp = 4;               // Rgba8888

			for (int y = 0; y < height; y++) {
				byte* rowPtr = ptr + y * row;
				for (int x = 0; x < width; x++) {
					rowPtr[x * bpp + 0] = color.R;
					rowPtr[x * bpp + 1] = color.G;
					rowPtr[x * bpp + 2] = color.B;
					rowPtr[x * bpp + 3] = color.A;
				}
			}
		}
		return bmp;
	}
}
