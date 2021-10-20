using NetworkingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPChatClient
{
	public class Client
	{
		private Socket socket;

		private Thread receive;

		public string Hostname { get; private set; }
		public int Port { get; private set; }

		public IPAddress Ip4Adress { get; private set; }

		private ChatPartner chatPartner;

		public Client(string hostname, int port)
		{

			Hostname = hostname;
			Port = port;

			IPHostEntry adressList = Dns.GetHostEntry(Hostname);

			foreach (IPAddress ip in adressList.AddressList)
			{
				if (AddressFamily.InterNetwork == ip.AddressFamily)
				{
					Ip4Adress = ip;
				}
			}

			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			receive = new Thread(new ThreadStart(Receive));
		}

		private void Connect()
		{
			Console.WriteLine("[Client]: Set username:");

			chatPartner = new ChatPartner
			{
				Username = Console.ReadLine()
			};

			bool isConnected = false;
			while (!isConnected)
			{
				try
				{
					socket.Connect(Ip4Adress, Port);
					isConnected = true;
				}
				catch (Exception e)
				{
					Console.WriteLine("[Client] Couldn't establish connection");
				}
			}

			Console.WriteLine("Client connected to {0}", socket.RemoteEndPoint.ToString());

			WriteTitle();

			Join();
		}

		private void SendTextMessage(string content)
		{
			Message message = new Message();
			message.MessageType = MessageType.TEXT;
			message.SetContent(content, ContentType.ASCII);

			SendMessage(message);
		}

		private void SendMessage(Message message)
		{
			message.Partner = chatPartner;
			socket.Send(message.GetSerializedBytes());
		}

		private void Join()
		{
			Message message = new Message();
			message.MessageType = MessageType.JOIN;

			SendMessage(message);
		}

		private void Leave()
		{
			Message message = new Message();
			message.MessageType = MessageType.LEAVE;

			SendMessage(message);
		}

		private Message ReceiveMessage()
		{
			byte[] byteRec = new byte[1024];
			int bytes = socket.Receive(byteRec);

			return JsonEncoding.DecodeMessage(byteRec);
		}

		private void Receive()
		{
			while (true)
			{
				Message msgReceived = ReceiveMessage();

				DisplayMessage(msgReceived);
			}
		}

		private void DisplayMessage(Message toDisplay)
		{
			Console.WriteLine(toDisplay.GetTextMessage());
		}

		public void Run()
		{
			Connect();
			receive.Start();

			while (true)
			{
				SendTextMessage(Console.ReadLine());
			}
		}


		private void WriteTitle()
		{
			Console.WriteLine("===========================================");
			Console.WriteLine("                Started Chat               ");
			Console.WriteLine("===========================================");
		}
	}
}
