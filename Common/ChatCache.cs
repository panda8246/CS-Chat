using ProtoBuf;

namespace Common;

[ProtoContract]
public struct ChatItem
{
    [ProtoMember(1)]public string UserName;
    [ProtoMember(2)]public string Message;

    public string ToString()
    {
        return $"{UserName} says: {Message}";
    }
}

public class ChatCache
{
    Queue<ChatItem> chatItems = new ();
    private readonly int queueLength;

    public ChatCache(int queueLength)
    {
        this.queueLength = queueLength;
    }


    public void AddChatItem(in ChatItem chatItem)
    {
        lock(this)
        {
            chatItems.Enqueue(chatItem);
            if (chatItems.Count > queueLength)
                chatItems.Dequeue();
        }
    }

    public ChatItem[] GetChatItem(int count)
    {
        if (chatItems.Count == 0)
            return [];
        count = int.Min(count, chatItems.Count);
        ChatItem[] items = new ChatItem[count];
        var idx = 0;
        // 只取队列末尾的count条
        int skip = chatItems.Count - count;
        foreach (var item in chatItems.Skip(skip))
        {
            items[idx++] = item;
        }
        return items;
    }
}
