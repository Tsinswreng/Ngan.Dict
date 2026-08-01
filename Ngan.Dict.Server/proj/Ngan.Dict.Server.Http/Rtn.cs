namespace Ngan.Dict.Server.Http;

public class Rtn :Attribute{
	public Type? Type { get; set; }
	public Rtn(Type? Type){
		this.Type = Type;
	}
}
