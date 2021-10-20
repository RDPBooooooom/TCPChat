using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetworkingLib
{
	public static class JsonEncoding
	{

		public static byte[] EncodeMessage(Message message)
		{
			return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
		}

		public static Message DecodeMessage(byte[] message)
		{

			// Remove bytes with no data
			int i = message.Length - 1;
			while (message[i] == 0) { 
				--i;
			}
			byte[] perfectArray = new byte[i + 1];
			Array.Copy(message, perfectArray, i + 1);


			return JsonSerializer.Deserialize<Message>(Encoding.UTF8.GetString(perfectArray));
		}

		public static byte[] EncodeString(string text, ContentType type)
		{
			switch (type)
			{
				case ContentType.ASCII:
					return Encoding.ASCII.GetBytes(text);
				case ContentType.UTF8:
					return Encoding.UTF8.GetBytes(text);
				default:
					return new byte[1024];
			}
		}

		public static String DecodeString(byte[] toDecode, ContentType type)
		{
			if (toDecode == null) return "";

			switch (type)
			{
				case ContentType.ASCII:
					return Encoding.ASCII.GetString(toDecode);
				case ContentType.UTF8:
					return Encoding.UTF8.GetString(toDecode);
				default:
					return "";
			}
		}
	}
}
