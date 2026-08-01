using System.Buffers;
using System.Text;
using Ngan.Dict.Core.Tools;

namespace Ngan.Dict.Server.Http;

public static class ExtnHttpCtx{
	public static HttpContext Body<T>(this HttpContext z, T Body){
		var json = JSON.Stringify(Body);
		z.Response.ContentType = "application/json;charset=utf-8";
		z.Response.BodyWriter.Write(Encoding.UTF8.GetBytes(json));
		return z;
	}
}
