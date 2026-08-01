/*
c# 異步流式讀文件、每次讀一部分。
讀內容的時候異步。
不用指定字符編碼、直接返回數字。
 */

using W = byte;
using System.Text;
using Chunk = System.ArraySegment<byte>;
using Ngaq.Core.Iter;

namespace Ngaq.Core.Tools.Io;

public  partial class ByteReader: IIterable<u8>, IDisposable{

	public str Path{get; set;}

	[Obsolete]
	public Encoding? Encoding{get; set;} = null;

	public FileStream Fs{get; set;}

	protected byte[] _Buffer = new byte[1];

	//protected i64 _ByteRead = -1;

	public u64 BufferSize{get; set;} = 0x100000;

	public u64 Pos{get; set;} = 0;
	//public Chunk nextChunk{get; set;} = default;
	public u64 ByteSize{get; set;}
	public Chunk CurChunk{get; set;} = default;
	public u64 ChunkPos{get; set;} = 0;//下次將讀取的位置

	public bool IsReadingChunk{get; set;} = false;

	//public word[] buffer{get; set;}


	public ByteReader(str path){
		Path = path;
		//buffer = new word[bufferSize];
		Fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		ByteSize = (u64)Fs.Length;
	}

	/** @pure */
	public async Task< Chunk > ReadNextChunk(){
		byte[] buffer = new byte[BufferSize];//宜複用、勿新開
		i32 bytesRead = await Fs.ReadAsync(buffer, 0, (i32)BufferSize);
		//i32 bytesRead = fs.Read(buffer, 0, bufferSize);
		if(bytesRead <= 0){
			return default; //Array=null, Count=0, Offset=0
		}
		//  return buffer.Take(bytesRead).ToArray(); // 只返回已读取的数据
		return new Chunk(buffer, 0, bytesRead);
	}

	public async Task<nil> AssignNextChunk(){
		IsReadingChunk = true;
		CurChunk = await ReadNextChunk();
		IsReadingChunk = false;
		return NIL;
	}



	public bool IsEnd{get; set;} = false;
	public bool HasNext(){
		return Pos < ByteSize;
	}

	public W Next(){
		if(ChunkPos >= (u64)CurChunk.Count){
			CurChunk = ReadNextChunk().Result;
			ChunkPos = 0;
			if(ChunkPos >= (u64)CurChunk.Count){
				//return -1; // EOF
				IsEnd = true;
				return 0;
			}
		}
		var ans = CurChunk[(i32)ChunkPos];
		ChunkPos++;
		Pos++;
		return ans;
	}

	public void Dispose(){
		Fs.Dispose();
	}

	~ByteReader(){
		Dispose();
	}

		//example
	// public static async IAsyncEnumerable<byte[]> ReadFileAsync
	// (
	// 	string filePath
	// 	, int bufferSize
	// 	, CancellationToken cancellationToken = default
	// ){
	// 	using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);

	// 	byte[] buffer = new byte[bufferSize];
	// 	int bytesRead;

	// 	while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0){
	// 		// 创建一个新数组，并将读取到的数据复制到其中
	// 		var data = new byte[bytesRead];
	// 		Array.Copy(buffer, data, bytesRead);
	// 		yield return data;
	// 	}
	// }
}


/*

c# 如何異步以utf8逐碼點讀文件?
比如我的文件是:   一1a𠂇😍
讀取時則依次返回 [一, 1, a, 𠂇, 😍]
注意: 以下字符的unicode編碼爲:
𠂇:0x20087
😍:0x1f60d
超過了0xffff。要把他們當成整個字符讀取、不要拆開。
 */

/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public static class Utf8Reader
{
    public static async Task<IEnumerable<string>> ReadUtf8CodePointsAsync(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true); // useAsync: true for asynchronous I/O
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 4096); // detectEncodingFromByteOrderMarks: true

        var codePoints = new List<string>();
        char[] buffer = new char[4096]; // Adjust buffer size as needed
        int charsRead;

        while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < charsRead; i++)
            {
                // Handle surrogate pairs for code points > U+FFFF
                if (char.IsHighSurrogate(buffer[i]) && i + 1 < charsRead && char.IsLowSurrogate(buffer[i + 1]))
                {
                    string codePoint = new string(buffer, i, 2);
                    codePoints.Add(codePoint);
                    i++; // Skip the low surrogate
                }
                else
                {
                    codePoints.Add(buffer[i].ToString());
                }
            }
        }

        return codePoints;
    }
}


// Example usage:
public async Task ExampleAsync()
{
    string filePath = "your_file.txt"; // Replace with your file path
    try
    {
        IEnumerable<string> codePoints = await Utf8Reader.ReadUtf8CodePointsAsync(filePath);
        foreach (string codePoint in codePoints)
        {
            Console.WriteLine(codePoint); // Output each code point
            Console.WriteLine($"Code point in hex: 0x{Convert.ToInt32(codePoint[0]).ToString("X4")}"); //Output code point in hex
        }
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"File not found: {filePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}
 */


/*

Result* getResult(args){
	//do sth.
	Result* result = new Result();
	result->value = xxx;
	return result;
}

int getResult(Result* result, args){
	//do sth.
	result->value = xxx;
	return 0;
}

 */
