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

    private HashSet<Thread> _threads = new HashSet<Thread>();
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
        _serverSocket.Blocking = false;
        MainLoop();
    }

    protected void MainLoop()
    {
        while (true)
        {
            CheckConnect();
            CheckReceive();
            CheckSend();
            Thread.Sleep(10);  // 100ms
        }
    }

    protected void CheckConnect()
    {
        try
        {
            Socket clientSocket = _serverSocket.Accept();
            _clientSockets.Add(clientSocket);
            Console.WriteLine($"Client {clientSocket.RemoteEndPoint} connected");
        }
        catch (SocketException ex)
        {

        }
    }

    List<Socket> disconnectedSockets = new List<Socket>();
    const int ReadLength = 1024;
    byte[] bytes = new byte[ReadLength];
    StringBuilder buffer = new StringBuilder();
    protected void CheckReceive()
    {
        foreach (var scoket in _clientSockets)
        {
            buffer.Clear();
            if (!scoket.Connected)
            {
                disconnectedSockets.Add(scoket);
                continue;
            }

            try
            {
                int byteLength = scoket.Receive(bytes);
                // 流式传输，可能出现多条消息合并在一起
                if (byteLength > 0)
                {
                    buffer.Append(Encoding.UTF8.GetString(bytes, 0, byteLength));
                }
                ChatItem chatItem = new ChatItem { UserName = scoket.RemoteEndPoint.ToString(), Message = buffer.ToString() };
                Console.WriteLine(chatItem.ToString());
                _msgList.Add(chatItem);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.NotConnected)
                {
                    Console.WriteLine($"SocketException: {ex.Message}");
                    disconnectedSockets.Add(scoket);
                }
                else
                {
                    // Console.WriteLine($"SocketException: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                disconnectedSockets.Add(scoket);
            }
        }

        foreach (var socket in disconnectedSockets)
        {
            socket.Close();
            _clientSockets.Remove(socket);
        }
        disconnectedSockets.Clear();
    }

    protected void CheckSend()
    {
        foreach (var item in _msgList)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, item);
                _sendHandler.Send(ms.ToArray());
            }
        }
        _msgList.Clear();
    }
}
