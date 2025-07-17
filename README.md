# CS-Chat
一个学习项目，用于学习socket封装tcp、udp。使用C#实现了一个实时聊天室，包含服务器和客户端。

IPAddress 包装IP地址
IPEndPoint 包装IP地址+端口
IPEndPoint对应一条连接

Socket for Server：
+ Socket.Bind绑定到IP（localhost）和端口
+ Socket.Listen监听端口
+ Socket.Blocking设置是否阻塞模式（默认阻塞）
+ Socket.Accept等待连接
+ Socket.Recive接受数据
+ Socket.Send发送数据


Socket for Client:
+ Socket.Connect发起连接
+ Socket.Recive接受数据
+ Socket.Send发送数据


接收和发送方都有各自的缓冲区，由操作系统对缓冲区的数据包做处理。
TCP是字节流协议，没有消息边界概念，需要应用层自己定义消息边界。


## 🛠️开发历程
1. 先了解Socket基本概念
    - 基本API使用
    - 阻塞和非阻塞、异步
    - 线程使用
2. 单线程阻塞式
    - 简单的接收、发送
    - 阻塞式的，一端发送时，另一端必须阻塞在Receive，否则会互相死锁
    - 服务端只支持一个客户端
3. 单线程非阻塞式
    - 轮询等待实现监听
4. 多线程
    - 服务端对每个连接分配一个线程，线程内阻塞监听连接、消息
    - 服务端单独一个线程对所有连接广播消息
    - 客户端收、发两个线程
    - 服务端支持多个客户端
5. 异步


编码问题
封装数据帧
