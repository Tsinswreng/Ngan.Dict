namespace Ngaq.Core.Tools.Encryption;

public interface IEncipherer{
	public u8[] Encrypt(Span<u8> Data, Span<u8> Key);
	public u8[] Decrypt(Span<u8> Data, Span<u8> Key);
}

