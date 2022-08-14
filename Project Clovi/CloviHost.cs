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
	/// A Dictionary of guilds containing all guild-specific data about the bot.
	/// </summary>
	internal static Dictionary<ulong, Object> GuildsData = new();

	public static Task Main() => new CloviHost().MainAsync();
	#endregion

	#region Main Async Task
	/// <summary>
	/// An async main method. Lets the main method run asynchronously.
	/// </summary>
	public async Task MainAsync()
	{
		CD.W("Checking for existing instance data...");
		if (!FIODirector.CheckRequiredFiles()) return;

		CD.W("Adding all core databases...");
		SQLDirector.DatabaseList.Add(new SQLiteDatabase("GuildsData"));

		CD.W("Initalizing logger...");
		CloviCore.Log += LoggerTask;

		CD.W("Initializing client...");
		CloviCore.Ready += ClientReady;

		CD.W("Attempting to start...");
		await CloviCore.LoginAsync(TokenType.Bot, Token);
		await CloviCore.StartAsync(); //Returns immediately after finishing.

		//Temporarily edits the bot's token which will remain as is if the bot fails to login.
		FIODirector.UpdateInstanceData("BotToken", "secret");

		while (true);
	}
	#endregion

	#region Client is Ready Task
	/// <summary>
	/// Runs when the Core is ready for use.
	/// </summary>
	public async Task ClientReady()
	{
		try
		{
			#region Initialization
			CD.W("Client started. Preparing...");
			FIODirector.UpdateInstanceData("BotToken", Token);
			Token = "secret";
			File.Delete(FIODirector.Directory[0] + @"\BotToken.txt");
			LinkedList<Request> RequestList = new();
			#endregion

			#region Standard Request Library
			//Retrieves all standard Request library Requests. (i.e. The premade requests.)
			CD.W("Initializing requests...");

			RequestList.AddLast(new Requests.GetLatency());
			RequestList.AddLast(new Requests.Ctest());
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
				CD.W($"Setting up guild \"{Guild.Name} ({Guild.Id})\"...");

				//Removes all commands made by this bot in the past.
				List<SocketApplicationCommand> CommandList = Guild.GetApplicationCommandsAsync().Result.ToList();
				foreach (SocketApplicationCommand cmd in CommandList)
				{
					await cmd.DeleteAsync();
					CD.W($"Removed Command \"{cmd.Name}\" from the list.");
				}

				//Adds custom commands to RequestDirector.
				foreach (Request r in RequestList)
				{
					ReqDirector.AddRequestItem(r);
					CD.W($"Added Request \"{r.Id}\" to the RequestList.");
				}

				//Adds all commands to Discord's listener.
				foreach (Request r in ReqDirector.RequestList)
				{
					await Guild.CreateApplicationCommandAsync(r.DiscordCommand);
					CD.W($"Added Request \"{r.Id}\" to the Listener.");
				}
			}
			#endregion

			#region Logging on guilds.
			SQLiteDatabase GuildsData = SQLDirector.GetDatabase("GuildsData");
			GuildsData.Connection.Open();
			SqliteDataReader Reader = GuildsData.Query("SELECT setting_value FROM guilds_settings WHERE setting_name = \"LoggerChannelId\"");
			ulong ChannelId;

			while (Reader.Read())
			{
				ChannelId = Reader.GetFieldValue<ulong>(0);
				CD.W($"ID: {ChannelId}");
				CD.ChannelIdList.Add(ChannelId);
				CD.W($"Logs now directs to {CloviCore.GetChannel(ChannelId)} ({ChannelId}).");
			}

			GuildsData.Connection.Close();
			#endregion

			CD.W("Enabling commands handler...");
			CloviCore.SlashCommandExecuted += SlashCommandHandler;

			CD.IsOnline = true;
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
		CD.W(msg.Message);
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

		return Task.CompletedTask;
	}
	#endregion
}