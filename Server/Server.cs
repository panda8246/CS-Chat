using System.Net.Sockets;
using System.Net;

public class Server
{
    public int Port { get; private set; }
    public String Host { get { return "127.0.0.1"; } }

    private Socket _serverSocket = null;

    public Server(int port)
    {
        Port = port;
    }

    public void Start()
    {
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(IPEndPoint.Parse(this.Host +  ":" + this.Port));
        _serverSocket.Listen(16);
        _serverSocket.Send()
        Loop();
    }

    public void Loop()
    {
        while (true)
        {
            Socket clientSocket = _serverSocket.Accept();
        }
    }
}
