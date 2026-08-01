namespace Ngaq.Windows.Domains.Clipboard;

using System;
using System.Runtime.InteropServices;
using System.Text;

unsafe public partial class WinClipBoard{
	[DllImport("user32.dll")]
	private static extern bool OpenClipboard(IntPtr hWndNewOwner);

	[DllImport("user32.dll")]
	private static extern bool CloseClipboard();

	[DllImport("user32.dll")]
	private static extern bool EmptyClipboard();

	[DllImport("user32.dll")]
	private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

	[DllImport("user32.dll")]
	private static extern IntPtr GetClipboardData(uint uFormat);

	[DllImport("user32.dll")]
	private static extern bool IsClipboardFormatAvailable(uint format);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("kernel32.dll")]
	private static extern bool GlobalUnlock(IntPtr hMem);

	private const uint CF_UNICODETEXT = 13; // Unicode 文本格式
	private const uint GMEM_MOVEABLE = 0x0002;

	public static void SetText(string text)
	{
		if (text == null)
			return;

		// 打開剪貼板
		if (!OpenClipboard(IntPtr.Zero))
			return;

		// 清空剪貼板
		EmptyClipboard();

		// 將字符串轉換為 UTF-16（Unicode）
		int size = (text.Length + 1) * sizeof(char); // +1 是為了結尾的空字符
		IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)size);
		if (hGlobal == IntPtr.Zero)
		{
			CloseClipboard();
			return;
		}

		IntPtr pGlobal = GlobalLock(hGlobal);
		if (pGlobal == IntPtr.Zero)
		{
			GlobalUnlock(hGlobal);
			CloseClipboard();
			return;
		}

		// 複製字符串到內存
		Marshal.Copy(text.ToCharArray(), 0, pGlobal, text.Length);
		// 添加結尾空字符
		Marshal.WriteInt16(pGlobal, text.Length * sizeof(char), 0);

		// 解鎖內存
		GlobalUnlock(hGlobal);

		// 將內存句柄設置到剪貼板
		SetClipboardData(CF_UNICODETEXT, hGlobal);

		// 關閉剪貼板
		CloseClipboard();
	}

	public static string? GetText()
	{
		// 打開剪貼板
		if (!OpenClipboard(IntPtr.Zero))
			return null;

		// 檢查剪貼板是否包含 Unicode 格式
		if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
		{
			CloseClipboard();
			return null;
		}

		// 獲取剪貼板數據
		IntPtr hGlobal = GetClipboardData(CF_UNICODETEXT);
		if (hGlobal == IntPtr.Zero)
		{
			CloseClipboard();
			return null;
		}

		IntPtr pGlobal = GlobalLock(hGlobal);
		if (pGlobal == IntPtr.Zero)
		{
			CloseClipboard();
			return null;
		}

		// 讀取字符數量
		var result = Marshal.PtrToStringUni(pGlobal);

		// 解鎖內存
		GlobalUnlock(hGlobal);

		// 關閉剪貼板
		CloseClipboard();

		return result;
	}
}
