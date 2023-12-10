using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UnityEngine;

public static class FileLoader
{
	public static void CompressFile(string path, string outputPath)
	{
		WriteAndCompress(outputPath, ReadFile(path));
	}

	public static T ReadCompressedJson<T>(string path)
	{
		return JsonUtility.FromJson<T>(ReadAndDecompress(path));
	}

	public static void ReadOverwriteJson(string path, object obj)
	{
		JsonUtility.FromJsonOverwrite(ReadAndDecompress(path), obj);
	}

	public static string ReadAndDecompress(string path)
	{
		using (StreamReader streamReader = new StreamReader(new GZipStream(File.Open(path, FileMode.Open), CompressionMode.Decompress)))
		{
			return streamReader.ReadToEnd();
		}
	}

	public static string ReadFile(string path)
	{
		using (StreamReader streamReader = new StreamReader(File.Open(path, FileMode.Open)))
		{
			return streamReader.ReadToEnd();
		}
	}

	public static void WriteAndCompressJson(string path, object data)
	{
		WriteAndCompress(path, JsonUtility.ToJson(data));
	}

	public static void WriteAndCompress(string path, string data)
	{
		using (StreamWriter streamWriter = new StreamWriter(new GZipStream(File.Create(path), CompressionMode.Compress)))
		{
			streamWriter.WriteLine(data);
		}
	}

	public static void WriteFile(string path, string data)
	{
		using (StreamWriter streamWriter = new StreamWriter(File.Create(path)))
		{
			streamWriter.WriteLine(data);
		}
	}
}
