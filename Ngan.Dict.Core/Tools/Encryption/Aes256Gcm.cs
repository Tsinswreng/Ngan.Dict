namespace Ngaq.Core.Tools.Encryption;

using System;
using System.Security.Cryptography;

public class Aes256Gcm : IEncipherer {
	protected static Aes256Gcm? _Inst = null;
	public static Aes256Gcm Inst => _Inst??= new Aes256Gcm();

	private const int NonceSize = 12; // 96-bit
	private const int TagSize = 16; // 128-bit
	private const int KeySize = 32; // 256-bit
	private const int TagBits = TagSize * 8; // 128

	public byte[] Encrypt(Span<byte> data, Span<byte> key) {
		if (key.Length != KeySize)
			throw new ArgumentException($"Key must be {KeySize} bytes.", nameof(key));

		byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
		byte[] tag = new byte[TagSize];
		byte[] cipher = new byte[data.Length];

		// ★ 使用显式 tag 长度
		using var aes = new AesGcm(key, TagBits);
		aes.Encrypt(nonce, data, cipher, tag);

		byte[] bundle = new byte[NonceSize + TagSize + cipher.Length];
		nonce.AsSpan().CopyTo(bundle);
		tag.AsSpan().CopyTo(bundle.AsSpan(NonceSize));
		cipher.AsSpan().CopyTo(bundle.AsSpan(NonceSize + TagSize));

		CryptographicOperations.ZeroMemory(cipher);
		return bundle;
	}

	public byte[] Decrypt(Span<byte> data, Span<byte> key) {
		if (key.Length != KeySize)
			throw new ArgumentException($"Key must be {KeySize} bytes.", nameof(key));
		if (data.Length < NonceSize + TagSize)
			throw new ArgumentException("Data too short.", nameof(data));

		Span<byte> nonce = data.Slice(0, NonceSize);
		Span<byte> tag = data.Slice(NonceSize, TagSize);
		Span<byte> cipher = data.Slice(NonceSize + TagSize);

		byte[] plain = new byte[cipher.Length];

		// ★ 同样指定 tag 长度
		using var aes = new AesGcm(key, TagBits);
		aes.Decrypt(nonce, cipher, tag, plain);

		return plain;
	}
}
