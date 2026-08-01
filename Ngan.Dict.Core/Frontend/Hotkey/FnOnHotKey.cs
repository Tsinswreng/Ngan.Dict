namespace Ngaq.Core.Frontend.Hotkey;

public delegate Task<IRespHotKey?> FnOnHotKey(
	IReqHotKey? Req, CT Ct
);
