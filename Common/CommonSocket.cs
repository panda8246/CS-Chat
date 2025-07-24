using System.Diagnostics;
using System.Net.Sockets;

namespace Common;

/// <summary>
/// 封装socekt
/// </summary>
public class CommonSocket
{
    private Socket socket;
    private byte[] receiveBuffer;
    private byte[] messageBuffer;
    private uint messageLength = 0;     // 消息头中定义的消息长度
    private uint hadReceive = 0;        // 记录该消息已接收了多少字节

    private Action<Memory<byte>> ReceiveCallback;


    public CommonSocket(Socket socket, int ReceiveBufferSize = 1024, int MaxMessageSize = 1024 * 1024)
    {
        this.socket = socket;
        this.receiveBuffer = new byte[ReceiveBufferSize];
        this.messageBuffer = new byte[MaxMessageSize];
    }


    public bool IsTCP
    {
        get { return socket?.ProtocolType == ProtocolType.Tcp; }
    }

    public bool IsUDP
    {
        get { return socket?.ProtocolType == ProtocolType.Udp; }
    }

    public void ListenReceive(Action<Memory<byte>> callback)
    {
        ReceiveCallback += callback;
    }

    public void StopListenReceive(Action<Memory<byte>> callback)
    {
        ReceiveCallback -= callback;
    }

    public void Send(byte[] buffer)
    {
        if (!socket.Connected)
            return;
        socket.Send(buffer);
    }


    public async Task SendAsync(byte[] buffer)
    {
        if (!socket.Connected)
            return;
        await socket.SendAsync(buffer);
    }

    public void ReceiveLoop()
    {
        try
        {
            while (socket.Connected)
            {
                ProcessMessage(socket.Receive(receiveBuffer));
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
    }

    public async Task ReceiveLoopAsync()
    {
        try
        {
            while (socket.Connected)
            {
                ProcessMessage(await socket.ReceiveAsync(receiveBuffer));
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
    }

    public void ProcessMessage(int length)
    {
        if (length > 0)
        {
            int receiveStart = 0;
            while (receiveStart < length)
            {
                if (messageLength == 0)
                {
                    messageLength = BitConverter.ToUInt32(receiveBuffer, 0);
                    hadReceive = 0;
                    receiveStart = 4;   // 跳过消息头
                }
                var remain = messageLength - hadReceive;
                var roundLength = int.Min((int)remain, (length - receiveStart));
                // 这里还要复制一次内存，有什么方式能消除?
                Array.Copy(receiveBuffer, receiveStart, messageBuffer, hadReceive, roundLength);
                hadReceive += (uint)roundLength;
                receiveStart += roundLength;
                if (hadReceive >= messageLength)
                {
                    // 直接引用内存，避免复制
                    Memory<byte> memory = messageBuffer.AsMemory(0, (int)messageLength);
                    ReceiveCallback.Invoke(memory);
                    messageLength = 0;
                }
            }
        }
    }
}
