using System.Data.Common;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Auth.app.Data.Enums;
using Microsoft.OpenApi.Extensions;
using NATS.Client;

namespace Auth.app.Services;

public class NatsPubSubService : IPubSubService
{
    private readonly IConfiguration _config;
    private readonly ConnectionFactory _cf;
    private IConnection _connection;
    private readonly Dictionary<string, EventHandler<MsgHandlerEventArgs>> _handlers;
    private readonly Dictionary<string, IAsyncSubscription> _subs;
    private readonly object _lock = new();
    private Options _options;

    public NatsPubSubService(IConfiguration config)
    {
        _config = config;

        _options = ConnectionFactory.GetDefaultOptions();
        _options.Name = _config["NATS:Name"];
        _options.User = _config["NATS:User"];
        _options.Password = _config["NATS:Password"];
        // options.Servers = new[] { "tls://localhost:4222" };
        _options.Url = _config["NATS:Server"];
        _options.AllowReconnect = true;
        _options.AddCertificate(_config["NATS:cacrt"]);
        _options.Secure = true;
        _options.MaxReconnect = 4;
        // options.TLSRemoteCertificationValidationCallback = verifyServerCert;
        _options.TLSRemoteCertificationValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
        {
            if (certificate == null) return false;

            if (_config["NATS:cacrt"] == null) return false;
            var trustedCACert = new X509Certificate2(_config["NATS:cacrt"]!);

            var serverCert = new X509Certificate2(certificate);
            var serverIssuer = serverCert.Issuer;

            return sslPolicyErrors == SslPolicyErrors.None
            || (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors
                && chain.ChainElements
                .Cast<X509ChainElement>()
                .Any(e => e.Certificate.Thumbprint == trustedCACert.Thumbprint));
        };
        _cf = new ConnectionFactory();
        _connection = _cf.CreateConnection(_options, true);
        _subs = [];
        _handlers = [];
    }
    // private bool verifyServerCert(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    // {
    //     Console.WriteLine("Test");
    //     if (sslPolicyErrors == SslPolicyErrors.None)
    //     {
    //         return true;
    //     }
    //     Console.WriteLine(sslPolicyErrors);
    //     Console.WriteLine("asd");
    //     return false;
    // }

    public void Disconnect()
    {
        _connection.Drain();
        _connection.Close();
        _connection.Dispose();
    }

    private void EnsureConnection()
    {
        if (_connection != null && _connection.State == ConnState.CONNECTED)
            return;
        lock (_lock)
        {

            _connection?.Dispose();
            _connection = _cf.CreateConnection(_options, true);
        }

        foreach (var key in _handlers.Keys)
        {
            _connection.SubscribeAsync(key, _handlers[key]);
        }

    }

    public void SendMessage(string topic, byte[] data)
    {
        EnsureConnection();
        _connection.Publish(topic, data);
    }

    public void SubscribeToTopic(string topic, PubSubMessageHandler h)
    {
        EventHandler<MsgHandlerEventArgs> adaptedHandler = (sender, args) =>
        {
            h(sender, args.Message.Data);
        };
        IAsyncSubscription subscription = _connection.SubscribeAsync(topic, adaptedHandler);
        _subs[topic] = subscription;
        _handlers[topic] = adaptedHandler;
    }

    public void UnsubscribeFromTopic(string topic)
    {
        if (_subs.TryGetValue(topic, out IAsyncSubscription? value))
        {
            value.Unsubscribe();
        }
        _subs.Remove(topic);
        _handlers.Remove(topic);
    }
}
