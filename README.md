一个学习项目，用于学习socket封装tcp、udp。使用C#实现了一个实时聊天室，包含服务器和客户端。

IPAddress 包装IP地址
IPEndPoint 包装IP地址+端口
IPEndPoint对应一条连接

Server：
+ Socket.Bind绑定到IP（localhost）和端口
+ Socket.Listen监听端口
+ Socket.Blocking设置是否阻塞模式（默认阻塞）
+ Socket.Accept等待连接
+ Socket.Recive接受数据

Client:



## 🛠️开发历程
1. 先了解Socket基本概念
    - 基本API使用
    - 阻塞和非阻塞、异步
    - 线程使用
2. 单线程阻塞式
3. 单线程非阻塞式
4. 多线程
5. 异步
