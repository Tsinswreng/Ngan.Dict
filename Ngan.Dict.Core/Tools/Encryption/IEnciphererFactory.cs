namespace Ngaq.Core.Tools.Encryption;

public interface IEnciphererFactory{
	public IEncipherer GetEncipherer(EEncryptionType Type);
}

