using System;
using System.IO;
using System.Security.Cryptography;

namespace SandboxNetwork
{
	public class FileHashManager
	{
		public static string GenerateFileHash(string path, out int fileLength)
		{
			using (var md5 = MD5.Create())
			{
				fileLength = 0;
				var fileBytes = File.ReadAllBytes(path);
				if (fileBytes == null) return string.Empty;
				var fileHash = BitConverter.ToString(md5.ComputeHash(fileBytes)).Replace("-", "").ToLower();
				fileLength = fileBytes.Length;
				return fileHash;
			}
		}

		public static bool IsHashMatch(string path, string hash)
		{
			return GenerateFileHash(path, out _).ToLower() == hash.ToLower();
		}
	}
}