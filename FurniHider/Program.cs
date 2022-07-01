using Xabbo.GEarth;
using Xabbo.Messages;
using Xabbo.Interceptor;

var extension = new Extension(GEarthOptions.Default
    .WithName("FurniHider")
    .WithDescription("Hide furnis (client-side)")
    .WithAuthor("Moch")
);

Console.WriteLine("Connecting...");
await extension.RunAsync();

class Extension : GEarthExtension
{
    private bool isEnabled;

    public Extension(GEarthOptions options) : base(options)
    { }

    protected override void OnConnected(GameConnectedEventArgs e) =>
        Console.WriteLine("Connected to G-Earth");
    protected override void OnDisconnected() =>
        Console.WriteLine("Disconnected to G-Earth");

    [InterceptOut(nameof(Outgoing.Chat))]
    public async void OnChat(InterceptArgs e)
    {
        string message = e.Packet.ReadString().ToLower();
        if (message.StartsWith('/'))
        {
            e.Block();
            string[] args = message[1..].Split();

            isEnabled = args[0] == "start";
            await SendAsync(In.Chat, 0, (isEnabled ? "Enabled" : "Disabled"), 0, 0, 0, 0);
        }
    }

    [InterceptOut(nameof(Outgoing.UseStuff), nameof(Outgoing.UseWallItem))]
    public async void OnUse(InterceptArgs e)
    {
        if (isEnabled)
        {
            IPacket packet = e.Packet;
            int id = packet.ReadInt();

            await SendAsync(In.ActiveObjectRemove, id.ToString(), false, 0, 0).ConfigureAwait(false);
            await SendAsync(In.RemoveItem, id.ToString(), 0).ConfigureAwait(false);
        }
    }
}