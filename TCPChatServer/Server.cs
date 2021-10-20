using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetworkingLib;

namespace TCPChatServer
{
	public class Server
	{

		public Dictionary<ChatPartner, Socket> RegisteredClients { get; private set; }

		public List<Socket> UnregisteredClients { get; private set; }

		public Queue<Message> MessageQueue { get; private set; }

		private Socket socket;

		Thread sender;
		Thread listener;

		public static Mutex registeredClientsMutex = new Mutex();
		public static Mutex unregisteredClientsMutex = new Mutex();

		public Server()
		{
			RegisteredClients = new Dictionary<ChatPartner, Socket>();
			UnregisteredClients = new List<Socket>();
			MessageQueue = new Queue<Message>();


			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			sender = new Thread(new ThreadStart(Send));
			listener = new Thread(new ThreadStart(Receive));
		}

		public void Run()
		{
			if (Setup())
			{
				sender.Start();
				listener.Start();
				Accept();
			}
		}

		private bool Setup()
		{
			try
			{
				IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 9999);
				socket.Bind(localEndPoint);
				socket.Listen();
			}
			catch (SocketException e)
			{
				Console.WriteLine("[Server]: Wasn't able to bind: {0} ", e.ToString());
				return false;
			}
			return true;
		}

		private void Send()
		{
			while (true)
			{
				if (MessageQueue.Count > 0)
				{
					registeredClientsMutex.WaitOne();

					Message msg = MessageQueue.Dequeue();
					foreach (KeyValuePair<ChatPartner, Socket> kvp in RegisteredClients)
					{
						byte[] test = JsonEncoding.EncodeMessage(msg);
						kvp.Value.Send(JsonEncoding.EncodeMessage(msg));
					}

					registeredClientsMutex.ReleaseMutex();
				}
			}
		}

		private void Accept()
		{
			while (true)
			{
				Socket clientSocket = socket.Accept();
				clientSocket.Blocking = false;

				unregisteredClientsMutex.WaitOne();

				UnregisteredClients.Add(clientSocket);
				Console.WriteLine("[Server] Accepted Connection");

				unregisteredClientsMutex.ReleaseMutex();
			}
		}

		private void Receive()
		{
			List<Socket> connections = new List<Socket>();
			while (true)
			{
				connections.Clear();

				unregisteredClientsMutex.WaitOne();
				connections.AddRange(UnregisteredClients);
				unregisteredClientsMutex.ReleaseMutex();

				registeredClientsMutex.WaitOne();
				connections.AddRange(RegisteredClients.Values);
				registeredClientsMutex.ReleaseMutex();

				ReceiveMessage(connections);
			}
		}

		private void ReceiveMessage(List<Socket> connections)
		{
			if (connections.Count == 0) { return; }

			Socket.Select(connections, null, null, 1000);

			foreach (Socket s in connections)
			{
				if (s != null)
				{
					byte[] msg = new byte[1024];
					int bytesReceived = s.Receive(msg);

					if (bytesReceived > 0)
					{
						HandleMessage(msg, s);
					}
				}
			}
		}

		private void HandleMessage(byte[] msg, Socket socket)
		{
			try
			{
				Message message = JsonEncoding.DecodeMessage(msg);

				switch (message.MessageType)
				{
					case MessageType.JOIN:
						Join(message, socket);
						break;
					case MessageType.LEAVE:
						Leave(message);
						break;
					case MessageType.TEXT:
						AddToMsgQueue(message);
						break;
					case MessageType.KEEPALIVE:
						break;
				}
			}
			catch (JsonException je)
			{
				Console.WriteLine("[Server]: Can't decode message");
				return;
			}
		}

		private void Join(Message msg, Socket socket)
		{
			registeredClientsMutex.WaitOne();
			if (!RegisteredClients.ContainsKey(msg.Partner))
			{
				RegisteredClients.Add(msg.Partner, socket);

				Console.WriteLine("[Server] Added user {0} to chat", msg.Partner.Username);

			}
			registeredClientsMutex.ReleaseMutex();

			unregisteredClientsMutex.WaitOne();
			if (UnregisteredClients.Contains(socket))
			{
				UnregisteredClients.Remove(socket);
			}
			unregisteredClientsMutex.ReleaseMutex();
		}

		private void Leave(Message msg)
		{
			registeredClientsMutex.WaitOne();

			if (RegisteredClients.ContainsKey(msg.Partner))
			{
				Socket toClose;
				RegisteredClients.TryGetValue(msg.Partner, out toClose);

				toClose.Shutdown(SocketShutdown.Both);
				toClose.Close();

				RegisteredClients.Remove(msg.Partner);

				Console.WriteLine("[Server] Removed user {0} from chat", msg.Partner.Username);
			}

			registeredClientsMutex.ReleaseMutex();
		}

		private void AddToMsgQueue(Message msg)
		{
			MessageQueue.Enqueue(msg);

			Console.WriteLine("[Server] Added Message from {0} to Queue", msg.Partner.Username);
		}
	}
}
