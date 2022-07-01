using Xabbo.GEarth;
using Xabbo.Messages;
using Xabbo.Interceptor;

var extension = new Extension(GEarthOptions.Default
    .WithName("FFBot")
    .WithDescription("Falling Furni BOT")
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
        string message = e.Packet.ReadString();
        if (message.StartsWith('/'))
        {
            e.Block();
            string[] args = message[1..].Split();

            isEnabled = args[0] == "start";
            await SendAsync(In.Chat, 0, $"BOT {(object)(isEnabled ? "Enabled" : "Disabled")}", 0, 0, 0, 0);
        }
    }

    [InterceptIn(nameof(Incoming.ActiveObjectAdd), nameof(Incoming.ActiveObjectUpdate))]
    public async void OnObjectAdd(InterceptArgs e)
    {
        if (isEnabled)
        {
            IPacket packet = e.Packet;
            packet.ReadInt();
            packet.ReadInt();
            int x = e.Packet.ReadInt(),
                y = e.Packet.ReadInt();

            await SendAsync(Out.Move, x, y).ConfigureAwait(false);
        }
    }
}