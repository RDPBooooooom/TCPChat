using NetworkingLib;
using System;

namespace TCPChatClient
{
	class App
	{
		static void Main(string[] args)
		{
			Client client = new Client("127.0.0.1", 9999);
			client.Run();
		}
	}

}

