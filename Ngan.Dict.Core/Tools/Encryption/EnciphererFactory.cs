namespace Ngaq.Core.Tools.Encryption;

public class EnciphererFactory: IEnciphererFactory{
	public IEncipherer GetEncipherer(EEncryptionType Type){
		switch(Type){
			case EEncryptionType.Aes256Gcm:
				return Aes256Gcm.Inst;
			default:
				throw new NotSupportedException();
		}
	}
}

