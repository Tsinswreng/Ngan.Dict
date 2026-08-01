namespace Ngaq.Ui.Infra;

[Obsolete]
public interface IMsgViewModel{

	public ICollection<object?> Msgs{get;set;}

	public bool IsShowMsg{get;set;}

	public ViewModelBase AddMsg(object? Msg);

	public nil ShowMsg();

	public nil ClearMsg();
}
