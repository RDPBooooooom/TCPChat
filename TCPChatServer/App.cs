using System;

namespace TCPChatServer
{
	class App
	{
		static void Main(string[] args)
		{
			Server server = new Server();
			server.Run();

		}
	}
}
