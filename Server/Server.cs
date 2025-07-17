using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server
{
    public int Port { get; private set; }
    public String Host { get { return "127.0.0.1"; } }

    private Socket _serverSocket = null;

    private StringBuilder _buffer = new StringBuilder();

    public Server(int port)
    {
        Port = port;
    }

    public void Start()
    {
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(IPEndPoint.Parse(this.Host +  ":" + this.Port));
        _serverSocket.Listen(16);
        WaitOne();
    }

    public void WaitOne()
    {
        Socket clientSocket = _serverSocket.Accept();
        // clientSocket.ReceiveTimeout = 500;
        // clientSocket.SendTimeout = 100;
        Console.WriteLine($"Client {clientSocket.RemoteEndPoint} connected");
        byte[] bytes = new byte[1024];
        while (clientSocket.Connected)
        {
            int byteLength = clientSocket.Receive(bytes);
            if (byteLength > 0)
            {
                _buffer.Append(Encoding.UTF8.GetString(bytes, 0, byteLength));
            }
            Console.WriteLine(_buffer);
            clientSocket.Send(Encoding.UTF8.GetBytes(_buffer.ToString()));
            _buffer.Clear();
        }
    }
}
