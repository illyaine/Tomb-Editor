using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TombLib.Utils
{
	public static class XmlUtils
	{
		private static readonly XmlWriterSettings ReadableOutputSettings = new()
		{
			Indent = true,
			IndentChars = "\t"
		};

		public static bool IsXmlDocument(string filePath, out XmlDocument document)
		{
			document = new XmlDocument();

			try
			{
				document.Load(filePath);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#region Normal Read

		public static object ReadXmlFile(string filePath, Type type)
			=> ReadXmlFile(filePath, type, null);

		public static object ReadXmlFile(Stream stream, Type type)
			=> ReadXmlFile(stream, type, null);

		public static object ReadXmlFile(string filePath, Type type, XmlReaderSettings settings)
		{
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				return ReadXmlFile(stream, type, settings);
		}

		public static object ReadXmlFile(Stream stream, Type type, XmlReaderSettings settings)
		{
			using (var reader = XmlReader.Create(stream, settings))
			{
				var serializer = new XmlSerializer(type);
				return serializer.Deserialize(reader);
			}
		}

		#endregion Normal Read

		#region Generic Read

		public static T ReadXmlFile<T>(string filePath)
			=> (T)ReadXmlFile(filePath, typeof(T));

		public static T ReadXmlFile<T>(Stream stream)
			=> (T)ReadXmlFile(stream, typeof(T));

		public static T ReadXmlFile<T>(string filePath, XmlReaderSettings settings)
			=> (T)ReadXmlFile(filePath, typeof(T), settings);

		public static T ReadXmlFile<T>(Stream stream, XmlReaderSettings settings)
			=> (T)ReadXmlFile(stream, typeof(T), settings);

		#endregion Generic Read

		#region Normal Write

		public static void WriteXmlFile(string filePath, Type type, object content)
			=> WriteXmlFile(filePath, type, content, ReadableOutputSettings);

		public static void WriteXmlFile(Stream stream, Type type, object content)
			=> WriteXmlFile(stream, type, content, ReadableOutputSettings);

		public static void WriteXmlFile(string filePath, Type type, object content, XmlWriterSettings settings)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("File path must not be null or empty.", nameof(filePath));

			var fullPath = Path.GetFullPath(filePath);
			var directoryPath = Path.GetDirectoryName(fullPath);

			if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);

			var tempFilePath = GetTemporaryWritePath(fullPath);
			var backupFilePath = GetBackupFilePath(fullPath);

			try
			{
				using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					WriteXmlFile(stream, type, content, settings);
					stream.Flush(true);
				}

				ValidateXmlFile(tempFilePath, type);

				if (File.Exists(fullPath))
					File.Replace(tempFilePath, fullPath, backupFilePath, true);
				else
					File.Move(tempFilePath, fullPath);
			}
			catch
			{
				TryDeleteFile(tempFilePath);
				throw;
			}
		}

		public static void WriteXmlFile(Stream stream, Type type, object content, XmlWriterSettings settings)
		{
			using (var writer = XmlWriter.Create(stream, settings))
			{
				var serializer = new XmlSerializer(type);
				serializer.Serialize(writer, content);
			}
		}

		#endregion Normal Write

		#region Generic Write

		public static void WriteXmlFile<T>(string filePath, T content)
			=> WriteXmlFile(filePath, typeof(T), content);

		public static void WriteXmlFile<T>(Stream stream, T content)
			=> WriteXmlFile(stream, typeof(T), content);

		public static void WriteXmlFile<T>(string filePath, T content, XmlWriterSettings settings)
			=> WriteXmlFile(filePath, typeof(T), content, settings);

		public static void WriteXmlFile<T>(Stream stream, T content, XmlWriterSettings settings)
			=> WriteXmlFile(stream, typeof(T), content, settings);

		#endregion Generic Write

		public static string GetBackupFilePath(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("File path must not be null or empty.", nameof(filePath));

			return Path.GetFullPath(filePath) + ".bak";
		}

		private static string GetTemporaryWritePath(string filePath)
		{
			var directoryPath = Path.GetDirectoryName(filePath);
			var fileName = Path.GetFileName(filePath);

			return Path.Combine(directoryPath ?? string.Empty, fileName + ".tmp." + Guid.NewGuid().ToString("N"));
		}

		private static void ValidateXmlFile(string filePath, Type type)
		{
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				ReadXmlFile(stream, type, null);
		}

		private static void TryDeleteFile(string filePath)
		{
			try
			{
				if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
					File.Delete(filePath);
			}
			catch
			{
			}
		}
	}
}
