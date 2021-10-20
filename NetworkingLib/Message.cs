using System;
using System.Text;
using System.Text.Json;

namespace NetworkingLib
{
	public class Message
	{
		public MessageType MessageType { get; set; }
		public ContentType ContentType { get; set; }
		public byte[] Content { get; set; }

		public ChatPartner Partner { get; set; }

		public void SetContent(string text, ContentType encoding)
		{
			ContentType = encoding;
			switch (encoding)
			{
				case ContentType.ASCII:
					Content = Encoding.ASCII.GetBytes(text);
					break;
				case ContentType.UTF8:
					Content = Encoding.UTF8.GetBytes(text);
					break;
				default:
					Content = new byte[1024];
					break;
			}
		}

		public string GetTextMessage()
		{

			return GetPrefix() + GetContentAsString();

		}

		public string GetContentAsString()
		{
			return JsonEncoding.DecodeString(Content, ContentType);
		}

		private string GetPrefix()
		{
			return "[" + Partner.Username + "]: ";
		}

		public byte[] GetSerializedBytes()
		{
			return JsonEncoding.EncodeMessage(this);
		}
	}

	public enum MessageType
	{
		JOIN,
		LEAVE,
		KEEPALIVE,
		TEXT
	}

	public enum ContentType
	{
		ASCII,
		UTF8
	}

}
