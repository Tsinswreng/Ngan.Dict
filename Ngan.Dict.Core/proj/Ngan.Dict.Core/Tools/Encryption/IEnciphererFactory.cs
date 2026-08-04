namespace Ngan.Dict.Core.Tools.Encryption;

public interface IEnciphererFactory{
	public IEncipherer GetEncipherer(EEncryptionType Type);
}

