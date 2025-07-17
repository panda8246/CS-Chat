using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common;


public class Server
{
    public int Port { get; private set; }
    public String Host { get { return "127.0.0.1"; } }

    private Socket _serverSocket = null;

    private HashSet<Thread> _threads = new HashSet<Thread>();
    private HashSet<Socket> _clientSockets = new HashSet<Socket>();
    private ChatCache _chatCache = new ChatCache(20);
    private SendHandler _sendHandler = new SendHandler();
    private List<byte[]> _msgList = new List<byte[]>();


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
        CreateOneWait();
        MainLoop();
    }

    protected void MainLoop()
    {
        while (true)
        {
            lock (_msgList)
            {
                foreach (var item in _msgList)
                    _sendHandler.Send(item);
                _msgList.Clear();
            }

            Thread.Sleep(100);  // 100ms
        }
    }

    protected void CreateOneWait()
    {
        Thread thread = new Thread(WaitOne);
        thread.Start();
        _threads.Add(thread);
    }

    public void WaitOne()
    {
        StringBuilder buffer = new StringBuilder();
        Socket clientSocket = _serverSocket.Accept();
        _clientSockets.Add(clientSocket);
        Console.WriteLine($"Client {clientSocket.RemoteEndPoint} connected");
        // 开启一个新线程去监听连接
        CreateOneWait();
        const int ReadLength = 1024;
        byte[] bytes = new byte[ReadLength];
        try
        {
            while (clientSocket.Connected)
            {
                // 流式传输，可能出现多条消息合并在一起
                int byteLength = clientSocket.Receive(bytes);
                if (byteLength > 0)
                {
                    buffer.Append(Encoding.UTF8.GetString(bytes, 0, byteLength));
                }

                var str = $"{clientSocket.RemoteEndPoint} says: {buffer}";
                Console.WriteLine(str);
                lock (_msgList)
                {
                    _msgList.Add(Encoding.UTF8.GetBytes(str));
                }
                buffer.Clear();
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
            clientSocket.Close();
        }
        _clientSockets.Remove(clientSocket);
        _threads.Remove(Thread.CurrentThread);
    }
}
