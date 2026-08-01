namespace Ngaq.Client;

public interface IHttpClientGetter{
	public HttpClient GetHttpClient();
}

public class HttpClientGetter:IHttpClientGetter{
protected static HttpClientGetter? _Inst = null;
public static HttpClientGetter Inst => _Inst??= new HttpClientGetter();

	public HttpClient HttpClient{get;set;} = new();
	public HttpClient GetHttpClient(){
		return HttpClient;
	}
}
