namespace Project_Clovi;

using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;

public class CloviHost
{
	#region Initialization
	/// <summary>
	/// This current build's version. Change after each version release.
	/// </summary>
	public const String BOT_VERSION = $"ALPHA v1.0.0";

	/// <summary>
	/// The core socket client responsible for handling all interactions between the front-end and back-end.
	/// </summary>
	public static readonly DiscordSocketClient CloviCore = new();

	/// <summary>
	/// Handles the input and output of information to and from the console within this host.
	/// </summary>
	internal static ConsoleDirector ConDirector = new();
	private static readonly ConsoleDirector CD = ConDirector;

	/// <summary>
	/// Handles all Database reads and writes.
	/// </summary>
	internal static SQLiteDirector SQLDirector = new();

	/// <summary>
	/// Handles all file reads and writes stored locally on the host device.
	/// </summary>
	internal static FileIODirector FIODirector = new();

	/// <summary>
	/// Directs all Request reads and changes within this host.
	/// </summary>
	internal static RequestDirector ReqDirector = new(new List<Request>());

	/// <summary>
	/// The bot's secret token. Its default value is "secret".
	/// </summary>
	internal static String Token = "secret";

	/// <summary>
	/// Determines whether the bot is currently activated and (but not always) online.
	/// </summary>
	internal static bool IsBotActive = true;

	public static Task Main() => new CloviHost().MainAsync();
	#endregion

	#region Main Async Task
	/// <summary>
	/// An async main method. Lets the main method run asynchronously.
	/// </summary>
	public async Task MainAsync()
	{
		CD.W($"Starting Clovi {BOT_VERSION}...");
		CD.W("Checking for existing instance data...");
		if (!FIODirector.CheckRequiredFiles()) return;

		CD.W("Adding all core databases...");
		SQLDirector.DatabaseList.Add(new SQLiteDatabase("GuildsData"));

		CD.W("Checking for databases...");
		SQLDirector.DatabaseCheckup(0);

		CD.W("Initalizing logger...");
		CloviCore.Log += LoggerTask;

		CD.W("Initializing client...");
		CloviCore.Ready += ClientReady;

		CD.W("Attempting to start...");
		await CloviCore.LoginAsync(TokenType.Bot, Token);
		await CloviCore.StartAsync(); //Returns immediately after called.
		await CloviCore.SetActivityAsync(new Game("you wank.", ActivityType.Watching, ActivityProperties.Join));

		//Creates a new thread to run an interruptible infinite loop.
		new Thread(() =>
		{
			while (IsBotActive)
			{
				Thread.Sleep(20000);
				if (CD.PendingLog.Length > 0) CD.SendLog();
			}

			CD.W("Bot marked for shutdown. Logging out & shutting down...", true);
			CloviCore.LogoutAsync();
			Environment.Exit(0);
		}).Start();
	}
	#endregion

	#region Ready Client Task
	/// <summary>
	/// Runs when the Core is ready for use.
	/// </summary>
	public async Task ClientReady()
	{
		try
		{
			#region Initialization
			CD.W("Client started. Preparing...");
			Token = "secret";
			File.Delete(FIODirector.Directory[0] + @"\BotToken.txt");
			LinkedList<Request> RequestList = new();
			SqliteDataReader Reader;
			bool IsBotEnabled = true;
			#endregion

			CD.W("Checking guilds_settings table...");
			SQLDirector.DatabaseCheckup(1);

			#region Standard Request Library
			//Retrieves all standard Request library Requests. (i.e. The premade requests.)
			CD.W("Initializing standard requests...");

			RequestList.AddLast(new Requests.GetLatency());
			RequestList.AddLast(new Requests.Ctest());
			RequestList.AddLast(new Requests.SetLoggerChannel( //idfk why its throwing: System.ArgumentNullException: Value cannot be null. (Parameter 'name')
				ParamsArg: new SlashCommandOptionBuilder[2]
				{
					new SlashCommandOptionBuilder().AddOption(name: "channel", ApplicationCommandOptionType.Channel, "The channel to send logs to.", isRequired: true),
					new SlashCommandOptionBuilder().AddOption(name: "devpassword", ApplicationCommandOptionType.String, "The password only found in the source code.", isRequired: true)
				}
			));
			#endregion

			#region Addon Requests
			//TODO: Retrieve the array from a JSON file. This contains any custom commands.
			//NOTE: Do not store Arrays directly in JSONs. A good option is to store an Array as a value in a Dictionary.
			CD.W("Initializing additional requests...");
			#endregion

			#region Removal & addition of requests.
			//For each Guild the bot is in...
			foreach (SocketGuild Guild in CloviCore.Guilds)
			{
				CD.W($"Setting up guild \"{Guild.Name}\" ({Guild.Id})...");

				SQLDirector.GetDatabase("GuildsData").Connection.Open();
				Reader = SQLDirector.Query("GuildsData", "SELECT setting_value FROM guilds_settings WHERE setting_name = \"IsBotEnabled\"");
				if (Reader.Read()) IsBotEnabled = bool.Parse(Reader.GetFieldValue<String>(0));

				CD.W($"IsBotEnabled for \"{Guild.Name}\"? {IsBotEnabled}");

				if (IsBotEnabled)
				{
					//Removes all commands made by this bot in the past.
					List<SocketApplicationCommand> CommandList = Guild.GetApplicationCommandsAsync().Result.ToList();
					CD.W("Removing all commands...");
					foreach (SocketApplicationCommand cmd in CommandList)
					{
						await cmd.DeleteAsync();
						CD.W($"Removed Command \"{cmd.Name}\" from the list.");
					}

					//Adds custom commands to RequestDirector.
					CD.W("Adding Requests to the RequestList...");
					foreach (Request r in RequestList)
					{
						ReqDirector.AddRequestItem(r);
						CD.W($"Added Request \"{r.Name}\".");
					}

					//Adds all commands to Discord's listener.
					CD.W("Adding Requests to the Listener...");
					foreach (Request r in ReqDirector.RequestList)
					{
						await Guild.CreateApplicationCommandAsync(r.DiscordCommand);
						CD.W($"Added Request \"{r.Name}\".");
					}
				}
			}
			#endregion

			#region Logging on guilds.
			SQLiteDatabase GuildsData = SQLDirector.GetDatabase("GuildsData");
			GuildsData.Connection.Open();
			Reader = GuildsData.Query("SELECT setting_value FROM guilds_settings WHERE setting_name = \"LoggerChannelId\"");
			ulong ChannelId;

			CD.W("Adding channels for logging...");
			while (Reader.Read())
			{
				ChannelId = Reader.GetFieldValue<ulong>(0);
				if (ChannelId == 0) continue;
				CD.LogChannelIdList.Add(ChannelId);
				CD.W($"Directing logs to #{CloviCore.GetChannel(ChannelId)} ({ChannelId}).");
			}

			GuildsData.Connection.Close();
			#endregion

			CD.W("Enabling commands handler...");
			CloviCore.SlashCommandExecuted += SlashCommandHandler;

			CD.W("Enabling messages handler...");
			CloviCore.MessageReceived += MessageHandler;

			CD.W("Ready"); //MAX 2000 CHARS.

			CD.WaitingForQueue = false;
			CD.IsOnline = true;
			CD.SendLog();
		}
		catch (Exception e) { CD.W(e.ToString()); }
	}
	#endregion

	#region Logger Task
	/// <summary>
	/// A logging task that's executed by the API everytime it needs to log something to the console.
	/// It forwards any logging task to a ConsoleDirector.
	/// </summary>
	private Task LoggerTask(LogMessage msg)
	{
		CD.W(msg.Message, CD.IsOnline);

		return Task.CompletedTask;
	}
	#endregion

	#region Command Handler Task
	/// <summary>
	/// Called when any command is executed.
	/// </summary>
	private Task SlashCommandHandler(SocketSlashCommand cmd)
	{
		CD.W($"Executing request \"{cmd.CommandName}\"...");
		ReqDirector.ExecuteRequest(cmd, CloviCore);
		CD.SendLog();

		return Task.CompletedTask;
	}
	#endregion

	#region Message Handler Task
	private Task MessageHandler(SocketMessage msg)
	{
		if (msg.Author.IsBot) return Task.CompletedTask;
		CD.W($"Receiving message from \"{msg.Channel.Name} by {msg.Author.Username}\" with the following content...\n\"{msg.Content}\"", true);

		bool isUnclean = false;
		int cCount = 0;
		foreach (char c in msg.CleanContent.ToLower())
		{
			if (c == 'c') { isUnclean = true; cCount++; }
		}

		if (isUnclean) msg.Channel.SendMessageAsync($"You dirty bastard. You used {cCount} ||c||'s in that message.");

		return Task.CompletedTask;
	}
	#endregion
}