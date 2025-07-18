using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using Common;
using ProtoBuf;


public class Server
{
    public int Port { get; private set; }
    public String Host { get { return "127.0.0.1"; } }

    private Socket _serverSocket = null;

    private HashSet<Task> _taskSet = new HashSet<Task>();
    private HashSet<Socket> _clientSockets = new HashSet<Socket>();
    private ChatCache _chatCache = new ChatCache(20);
    private SendHandler _sendHandler = new SendHandler();
    private List<ChatItem> _msgList = new List<ChatItem>();


    public Server(int port)
    {

        Port = port;
        _sendHandler.SetSockets(_clientSockets);
    }

    public void Start()
    {
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(IPEndPoint.Parse(this.Host +  ":" + this.Port));
        _serverSocket.Listen(16);
        AsyncStarter().Wait();
    }

    protected async Task AsyncStarter()
    {
        Accpet();
        await MainLoop();
    }

    protected async Task Accpet()
    {
        while (true)
        {
            Socket socket = await _serverSocket.AcceptAsync();
            _clientSockets.Add(socket);
            Console.WriteLine($"Client {socket.RemoteEndPoint} connected");
            Receive(socket);
        }
    }

    protected async Task Receive(Socket socket)
    {
        StringBuilder buffer = new StringBuilder();
        const int ReadLength = 1024;
        byte[] bytes = new byte[ReadLength];
        try
        {
            while (socket.Connected)
            {
                // 流式传输，可能出现多条消息合并在一起
                int byteLength = await socket.ReceiveAsync(bytes);
                if (byteLength > 0)
                {
                    buffer.Append(Encoding.UTF8.GetString(bytes, 0, byteLength));
                }

                ChatItem chatItem = new ChatItem { UserName = socket.RemoteEndPoint.ToString(), Message = buffer.ToString() };
                Console.WriteLine(chatItem.ToString());
                _msgList.Add(chatItem);
                buffer.Clear();
            }
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.ConnectionReset)
            {
                Console.WriteLine($"Client {socket.RemoteEndPoint} disconnected");
            }
            socket.Close();
        }
        _clientSockets.Remove(socket);
    }

    protected async Task MainLoop()
    {
        while (true)
        {
            foreach (var item in _msgList)
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, item);
                    await _sendHandler.SendAsync(ms.ToArray());
                }
            }
            _msgList.Clear();
            await Task.Delay(100); // 100ms
        }
    }

}
