namespace Auth.app.Services;

public interface IPubSubService
{
    public void SubscribeToTopic(string topic, PubSubMessageHandler h);

    public void UnsubscribeFromTopic(string topic);

    public void SendMessage(string topic, byte[] data);

    public void Disconnect();
}

public delegate void PubSubMessageHandler(object? sender, byte[] message);