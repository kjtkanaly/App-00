namespace App_00;

static class TerminalUtilites
{
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
        
        // Listen for the user input
        while(true)
        {
            string? input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                // Display received command
                Console.WriteLine($"Received Command: {input}");

                if (input.Equals("kill-bot"))
                {
                    await BotLogOut();
                    break;
                }
            }
        }

        Console.WriteLine($"Program done... Goodbye!");
    }

    private static Task Log(Discord.LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private static async Task BotLogOut()
    {
        Console.WriteLine($"Logging out...");
        if (_client != null)
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}
