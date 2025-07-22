using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Common;
using ProtoBuf;

/// <summary>
/// Client暂时不用异步Socket，ReadLine是阻塞实现的
/// </summary>
public class Client
{
    private Socket _socket = null;
    private ChatCache chatCache = new ChatCache(20);

    public Client()
    {

    }

    public void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(IPEndPoint.Parse("127.0.0.1:8080"));
        Thread wirteThread = new Thread(Write);
        Thread readThread = new Thread(ReadFromServer);
        wirteThread.Start();
        readThread.Start();
        wirteThread.Join();
        readThread.Join();
    }

    public void Write()
    {
        while (_socket.Connected)
        {
            string str = Console.ReadLine();
            if (str == null)
                continue;
            _socket.Send(Encoding.UTF8.GetBytes(str));
        }
    }

    public void ReadFromServer()
    {
        byte[] bytes = new byte[1024];
        while (_socket.Connected)
        {
            int byteLength = _socket.Receive(bytes);
            if (byteLength > 0)
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(bytes, 0, byteLength);
                    ms.Position = 0;
                    ChatItem item = Serializer.Deserialize<ChatItem>(ms);
                    chatCache.AddChatItem(item);
                }
                UpdateChat();
            }
        }
    }

    public void UpdateChat()
    {
        Console.Clear();
        foreach (var item in chatCache.GetChatItem(20))
        {
            Console.WriteLine(item.ToString());
        }
    }
}
