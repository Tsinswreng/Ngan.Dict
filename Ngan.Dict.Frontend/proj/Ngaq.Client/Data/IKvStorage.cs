namespace Ngaq.Client.Data;

public  partial interface IKvStorage{
	public Task<str?> Get(str key);
	public Task<nil> Set(str key, str value);
	public Task<nil> Delete(str key);
}
