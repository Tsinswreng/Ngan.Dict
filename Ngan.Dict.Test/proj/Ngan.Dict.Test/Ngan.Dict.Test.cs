//先dotent build編譯、後在vscode 調試界面 選TestWin㕥行、如是則既有斷點又可連數據庫
//直ᵈ dotnet run 則cwd不對、連不到數據庫
using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core;
using Ngan.Dict.Backend;
using Ngan.Dict.Backend.Di;
using Ngan.Dict.Test;
using Tsinswreng.CsTreeTest;
//dotnet publish -c Release -r win-x64
// ./bin/Release/net10.0/win-x64/publish/Ngan.Dict.Test.exe
#pragma warning disable CS8321
#region Main

//new TestToolYaml().Run();
NgaqTest.Inst.Init();


// 或运行指定节点: var node = Tsinswreng.CsTreeTest.TestNodeRunner.FindNodeByName(root, "Repo Tests"); if (node is not null) await Tsinswreng.CsTreeTest.TestNodeRunner.RunNode(node, default);

#endregion Main


public partial class NgaqTest{
	public static NgaqTest Inst = new();
	public static str GetFullTypeName<T>(){
		return typeof(T).FullName!;
	}
	// static async Task Main(string[] args){

	// }
	public void Init(){
		Di();
		InitApp();
		//AppDiMgr.Inst.FnGetRSvc(Program.GetRSvc);
		
	}
	
	public IServiceProvider SvcProvider => Ngan.Dict.Core.Di.SvcProvider;
	public nil Di(){
		var svc = new ServiceCollection();
		svc
			.SetupCore()
			.SetupLocal()//TODO 改成按需API調用
			.SetupLocalFrontend()
			.SetupTest()
		;
		Ngan.Dict.Core.Di.SvcProvider = svc.BuildServiceProvider();
		return NIL;
	}
	public nil InitApp(){
		AppIniter.Inst.Sp = SvcProvider;
		_ = AppIniter.Inst.Init(default).Result;
		return NIL;
	}
	public static T GetRSvc<T>()
		where T : class
	{
		return Ngan.Dict.Core.Di.GetRSvc<T>();
	}

}


