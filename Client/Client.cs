using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client
{
    private Socket _socket = null;
    private StringBuilder _buffer = new StringBuilder();

    public Client()
    {

    }

    public void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(IPEndPoint.Parse("127.0.0.1:8080"));
        WaitOne();
    }

    public void WaitOne()
    {
        byte[] bytes = new byte[1024];
        while (_socket.Connected)
        {
            string str = Console.ReadLine();
            if (str == null)
                continue;
            _socket.Send(Encoding.UTF8.GetBytes(str));
            int byteLength = _socket.Receive(bytes);
            if (byteLength > 0)
            {
                _buffer.Append(Encoding.UTF8.GetString(bytes, 0, byteLength));
            }
            Console.WriteLine("from server： " + _buffer.ToString());
            _buffer.Clear();
        }
    }
}
