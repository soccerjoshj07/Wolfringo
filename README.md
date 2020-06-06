
# Wolfringo
[![GitHub](https://img.shields.io/github/license/TehGM/Wolfringo)](LICENSE) [![Releases](https://img.shields.io/github/v/release/TehGM/Wolfringo?sort=semver)]

This is a .NET library for WOLF (previously Palringo).

This library is designed with extensibility through Dependancy Injection in mind, and is compatible with ASP.NET Core and other .NET Core Hosting scenarios through [Wolfringo.Hosting](https://github.com/TehGM/Wolfringo/packages/257845) package.

Library works with strongly-typed messages and responses, that are serialized when sending and deserialized when receiving. Message listeners can be invoked by message type, giving full benefit of strong typing. Additionally, [Wolfringo.Utilities](https://github.com/TehGM/Wolfringo/packages/257846) package provides a Sender extensions class, which abstracts common sending tasks. Utilities package is included by default with Wolfringo package.

### Download

This library is currently in preview and hasn't yet been battle-tested, and therefore there might be bugs and updates might introduce breaking changes, some of which might not be clearly documented. Once preview ends, I'll do my best to make the library as backwards-compatible as possible, but until 1.0.0 release, be aware of pre-release stage of this library.

Preview version of this library is available as a [GitHub Package](https://github.com/TehGM/Wolfringo/packages/257862). Later versions will be available on nuget.org.

See [GitHub Packages](https://help.github.com/en/packages/using-github-packages-with-your-projects-ecosystem/configuring-dotnet-cli-for-use-with-github-packages#installing-a-package) installation instructions to learn how to install a package hosted on GitHub.

## Usage example

```csharp
using TehGM.Wolfringo;

// create client
IWolfClient client = new WolfClient();
client.AddMessageListener<WelcomeEvent>(OnWelcome);
client.AddMessageListener<ChatMessage>(OnChatMessage);

async void OnWelcome(WelcomeEvent welcome)
{
    if (welcome.LoggedInUser == null)
    {
        // login with Sender Utility
        await client.LoginAsync("MyBotEmail", "MyBotPassword");
    }
}

async void OnChatMessage(ChatMessage message)
{
    if (message.IsText && message.StartsWith("!hello"))
    {
        // get user profile
        WolfUser user = await client.GetUserAsync(message.SenderID.Value);
        // respond to the user (in group if it's a group message, in PM if it's a private message)
        await client.RespondWithTextAsync(message, $"Hello, {user.Nickname}!");
        // message someone bragging about being hello'ed!
        if (message.IsGroupMessage)
        {
            WolfGroup group = await client.GetGroupAsync(message.RecipientID);
            await client.SendPrivateTextMessageAsync(ownerUserID, $"I was greeted by {user.Nickname} in [{group.Name}] group!");
        }
        else
            await client.SendPrivateTextMessageAsync(ownerUserID, $"I was greeted by {user.Nickname} in PM!");
    }
}
```

See [Example project](Examples/SimplePingBot) for a full example.

### Logging
[WolfClient](Wolfringo.Core/WolfClient.cs) constructor takes an ILogger as one of the optional parameters, making it compatible with any of the major .NET logging frameworks. To enable logging, create a new instance of any ILogger, and simply pass it to the client constructor.

In .NET Core Host, simply configure logging using services as you normally would in ASP.NET Core/other Hosted scenario. Default [HostedWolfClient](Wolfringo.Hosting/HostedWolfClient.cs) will use dependency injection mechanisms to get the logger and pass it to the underlying [WolfClient](Wolfringo.Core/WolfClient.cs).

### Auto-Reconnecting

You can use [WolfClientReconnector](Wolfringo.Utilities/WolfClientReconnector.cs) to enable auto-reconnecting behaviour.
```csharp
IWolfClient client = new WolfClient();
WolfClientReconnector reconnector = new WolfClientReconnector(client);
```
The reconnector class will automatically reconnect the client until reconnection fails or `reconnector.Dispose()` method is called. To re-enable reconnection, manually connect the client and create a new [WolfClientReconnector](Wolfringo.Utilities/WolfClientReconnector.cs) again, providing the same client via the constructor.

If the reconnector behaviour is not sufficent for your use-case, listen to client's Disconnected event to implement own behaviour.

> Note: do not use [WolfClientReconnector](Wolfringo.Utilities/WolfClientReconnector.cs) if using hosted client wrapper from [Wolfringo.Hosting](Wolfringo.Hosting) package. This wrapper has reconnection logic built-in.

### .NET Core Host

For use with .NET Core Host, install [Wolfringo.Hosting](https://github.com/TehGM/Wolfringo/packages/257845) package in addition to the main Wolfringo package. This package contains a client wrapper suitable for use with ASP.NET Core and Generic Host.

```csharp
using TehGM.Wolfringo.Hosting;

// Configure the client options to include login credentials
services.Configure<HostedWolfClientOptions>(context.Configuration.GetSection("WolfClient"));
// add hosted wolf client with it's default services
services.AddWolfClient();
// add message handler that implements IHostedService and takes IHostedWolfClient as one of constructor parameters
services.AddHostedService<HostedMessageHandler>();
```

See [Example project](Examples/HostedPingBot) for a full example.

### Errors when sending
If server responds with an error, [MessageSendingException](Wolfringo.Core/MessageSendingException.cs) will be thrown and provide a error details. To handle errors, use try-catch block when sending any message.

If logging is configured in the client, the client will log the error regardless of how the exception gets handled by the caller.

## Extending the client
#### Serializer maps
Client uses power of Dependency Injection to allow customizability. The client accepts optional Message and Response Serializer maps which are used for serializing and deserializing the message and response objects. You can inject own instance of the map to change mapping, or even add new types if it's required.

You can see [DefaultMessageSerializerMap](Wolfringo.Core/Messages/Serialization/DefaultMessageSerializerMap.cs), [DefaultResponseSerializerMap](Wolfringo.Core/Messages/Serialization/DefaultResponseSerializerMap.cs),[DefaultMessageSerializer](Wolfringo.Core/Messages/Serialization/Serializers/DefaultMessageSerializer.cs) and [DefaultResponseSerializer](Wolfringo.Core/Messages/Serialization/Serializers/DefaultResponseSerializer.cs) for examples of default base implementations.
#### Overriding client methods
Client automatically caches the entities based on message/response type. If you add a new type that needs to support this, you must create a new client class inheriting from [WolfClient](Wolfringo.Core/WolfClient.cs). You can override `Task OnMessageSentInternalAsync(IWolfMessage message, IWolfResponse response, SerializedMessageData rawResponse, CancellationToken cancellationToken = default)` method to change behaviour for sent messages and received responses, and `Task OnMessageReceivedInternalAsync(IWolfMessage message, SerializedMessageData rawMessage, CancellationToken cancellationToken = default)` method to change behaviour for received events and messages.

> Note: it's recommended to still call base method after own implementation to prevent accidentally breaking default behaviour. Overriding these methods should be handled with caution.

#### Determining response type for sent message
[WolfClient](Wolfringo.Core/WolfClient.cs) needs to know how to deserialize message's response, and to determine the type, it uses an [IResponseTypeResolver](Wolfringo.Core/Messages/Responses/IResponseTypeResolver.cs) to select the type that will be used with response serializer mappings. This interface can be passed into the client constructor. If null or none is passed in, [DefaultResponseTypeResolver](Wolfringo.Core/Messages/Responses/DefaultResponseTypeResolver.cs) will be used automatically.

[IResponseTypeResolver](Wolfringo.Core/Messages/Responses/IResponseTypeResolver.cs) respects [ResponseType](Wolfringo.Core/Messages/Responses/ResponseTypeAttribute.cs) attribute on the message type, and will ignore the generic type passed in with `SendAsync` method. If the attribute is missing, default client implementation will instruct the type resolver to use provided generic type. Client will attempt to cast the response to the provided generic type regardless of the actual response type, and might return null.

## Further development
> This library is still in preview, so breaking changes might be introduced with any version until 1.0.0 release.

#### Planned features
- Some kind of commands system

#### Known bugs and missing features
- Avatar setting is not supported, due to Wolf protocol not supporting it yet.
- Spam filter settings is not supported.

### Reporting a bug or requesting a feature?
In case you want to report a bug or request a feature, open a new [Issue](https://github.com/TehGM/Wolfringo/issues).

## License
Copyright (c) 2020 TehGM 

Licensed under [MIT License](LICENSE).