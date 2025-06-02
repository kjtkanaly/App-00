using System.Text.Json;

namespace App_00;

public struct GuildInfo
{
    public string _name {get; set;}
    public ulong _id {get; set;}
    public List<ChannelInfo> _channels {get; set;}
}

public struct ChannelInfo
{
    public string _name {get; set;}
    public ulong _id {get; set;}
}

public struct DataFiles
{
    public string _botConfigFile {get;} = "token.cfg";
    public string[] _jsonPaths {get;} = ["loser-bar.json"]; 

    public DataFiles()
    {
    }

    public string GetBotToken()
    {
        // Read in the config file
        string token = File.ReadAllText(_botConfigFile);
        token = token.TrimEnd('\r', '\n');
        return token;
    }

    public List<GuildInfo> GetAllGuildInfo(bool debug = false)
    {
        List<GuildInfo> allGuildInfo = new List<GuildInfo>(_jsonPaths.Length);
        
        for (int i = 0; i < _jsonPaths.Length; i++)
        {
            string? jsonString = File.ReadAllText(_jsonPaths[0]);
            
            if (debug)
            {
                Console.WriteLine(jsonString);
            }

            GuildInfo guildInfo = JsonSerializer.Deserialize<GuildInfo>(
                    string.Join(Environment.NewLine, jsonString));

            allGuildInfo.Add(guildInfo);
        }
        
        return allGuildInfo;
    }
}

class Program
{
    private static Discord.WebSocket.DiscordSocketClient? _client;
    private static string? _token;
    private static List<GuildInfo>? _guilds;

    public static async Task Main()
    {
        // Create the data files instance
        DataFiles dataFiles = new DataFiles();

        // Get the List of Guild Info
        _guilds = dataFiles.GetAllGuildInfo();

        // Create our client object
        _client = new Discord.WebSocket.DiscordSocketClient();

        // Hook the Log Handler into the Client's Log Event
        _client.Log += Log;

        // Get the Bot's Token
        _token = dataFiles.GetBotToken();

        // Log the bot in via the token
        await _client.LoginAsync(Discord.TokenType.Bot, _token);
        await _client.StartAsync();
        
        // Load the jsons
        
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
                else if (input.Equals("scrape-channel"))
                {
                    await ScrapeTextChannel();
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
    
    private static string GetUserInput()
    {
        // Listen for the user input
        while(true)
        {
            string? input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                return input;
            }
        }
    }

    private static async Task ScrapeTextChannel()
    {
        if (_guilds == null)
        {
            return;
        }

        // Print the loaded guild options
        Console.WriteLine($"Pick a Guild:");
        for(int i = 0; i < _guilds.Count; i++)
        {
            Console.WriteLine($"[{i}]: {_guilds[i]._name}");
        }
        int guildIndex = int.Parse(GetUserInput());
        GuildInfo guild = _guilds[guildIndex];

        // Await the Channel ID
        Console.WriteLine($"Pick a Channel:");
        for(int i = 0; i < guild._channels.Count; i++)
        {
            Console.WriteLine($"[{i}]: {guild._channels[i]._name}");
        }
        int channelIndex = int.Parse(GetUserInput());
        ChannelInfo channel = guild._channels[channelIndex];
        
        // Get the content 
        await ScrapeMessagesAsync(guild._id, channel._id);
    }
    
    private static async Task ScrapeMessagesAsync(
        ulong guildID, 
        ulong channelID)
    {
        // Make sure the client object is set
        if (_client == null)
        {
            return;
        }
        // Get the channel using the provided guild and channel IDs
        Discord.WebSocket.SocketGuild guild = _client.GetGuild(guildID);
        Console.WriteLine($"Guild Name: {guild.Name}");
        Discord.WebSocket.SocketTextChannel channel = guild.GetTextChannel(channelID);

        // Incase the channel wasn't found
        if (channel == null)
        {
            Console.WriteLine($"Channel not found...");
            return;
        }
        
        // Get 5 messages from the text channel
        List<Discord.IMessage> messages = new List<Discord.IMessage>();
        await foreach(var page in channel.GetMessagesAsync(limit: 100))
        {
            messages.AddRange(page);
        }
        
        // Debug
        for(int i = 0; i < messages.Count; i++)
        {
            Console.WriteLine($"{messages[i].Timestamp}: {messages[i].Author} - {messages[i].Content}");
        }
    }
}
