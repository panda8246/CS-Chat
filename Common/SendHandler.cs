using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Common;

/// <summary>
/// 用于处理socket广播，和实现一个应用层发送缓冲区以测试黏包等情况
/// </summary>
public class SendHandler
{
    private HashSet<Socket> sendSockets;

    public SendHandler()
    {

    }

    public void SetSockets(HashSet<Socket> sockets)
    {
        sendSockets = sockets;
    }

    public void SetSockets(Socket socket)
    {
        if (sendSockets == null)
        {
            sendSockets = new HashSet<Socket>();
        }
        sendSockets.Add(socket);
    }

    public void Send(byte[] bytes)
    {
        // TODO sendSockets列表线程不安全
        foreach (Socket socket in sendSockets)
        {
            socket.Send(bytes);
        }
    }

    public async Task SendAsync(byte[] bytes)
    {
        await Task.WhenAll(sendSockets.Select(socket => socket.SendAsync(bytes)));
    }

}
