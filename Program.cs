namespace App_00;

class HelloWorld
{
    public float _userInput;

    public void Main()
    {
        Console.WriteLine("Hello, World!");
    }

    public void ParseAndSquare()
    {
        // Reading the user's input and try and parse the value
        float.TryParse(Console.ReadLine(), out _userInput);

        // Out the Square Value
        Console.WriteLine($"User's Value Squared: {MathF.Pow(_userInput, 2)}");
    }
}

class Program
{
    private static Discord.WebSocket.DiscordSocketClient? _client;

    public static async Task Main()
    {
        // Create our client object
        _client = new Discord.WebSocket.DiscordSocketClient();

        // Hook the Log Handler into the Client's Log Event
        _client.Log += Log;

        // Read in the config file
        string token = File.ReadAllText("token.cfg");
        token = token.TrimEnd('\r', '\n');

        // Log the bot in via the token
        await _client.LoginAsync(Discord.TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is killed
        await Task.Delay(-1);
    }

    private static Task Log(Discord.LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
