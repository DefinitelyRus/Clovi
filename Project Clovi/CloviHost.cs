namespace Project_Clovi;

using Discord;
using Discord.WebSocket;

public class CloviHost
{
	#region Initialization
	/// <summary>
	/// The core socket client responsible for handling all interactions between the front-end and back-end.
	/// </summary>
	public static readonly DiscordSocketClient CloviCore = new();

	/// <summary>
	/// Directs all Request reads and changes within this host.
	/// </summary>
	internal static RequestDirector ReqDirector = new(new List<Request>());

	/// <summary>
	/// Handles the input and output of information to and from the console within this host.
	/// </summary>
	internal static ConsoleDirector ConDirector = new(CloviCore, ReqDirector, "ConsoleApp");
	private static  readonly ConsoleDirector CD = ConDirector;

	/// <summary>
	/// Handles all file reads and writes stored locally on the host device.
	/// </summary>
	internal static FileIODirector FIODirector = new();

	/// <summary>
	/// Handles all Database reads and writes.
	/// </summary>
	internal static Database SQLDirector = new();

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

		CD.W("Initalizing logger...");
		CloviCore.Log += Log;

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
		#region Initialization
		CD.W("Client started. Preparing...");
		FIODirector.UpdateInstanceData("BotToken", Token);
		Token = "secret";
		File.Delete(FIODirector.Directory[0] + @"\BotToken.txt");
		ulong GuildId = 262784778690887680; //!!! TEMPORARY; must be saved on a case-by-case basis.
		SocketGuild Guild = CloviCore.GetGuild(GuildId);
		LinkedList<Request> RequestList = new();
		#endregion

		#region Standard Request Library
		//Retrieves all standard Request library Requests. (i.e. The premade requests.)
		CD.W("Initializing requests...");

		RequestList.AddLast(new Requests.GetLatency());
		#endregion

		#region Addon Requests
		//TODO: Retrieve the array from a JSON file. This contains any custom commands.
		//NOTE: Do not store Arrays directly in JSONs. A good option is to store an Array as a value in a Dictionary.
		CD.W("Initializing additional requests...");
		#endregion

		#region Removal & addition of requests.
		CD.W("Removing any duplicate requests...");
		//Do this for every guild listed in GuildData.
		//Removes all commands made by this bot in the past.
		//Not the most efficient way to do this, but it'll do for now.
		List<SocketApplicationCommand> c = Guild.GetApplicationCommandsAsync().Result.ToList();
		foreach (SocketApplicationCommand cmd in c) await cmd.DeleteAsync();

		//Adds custom commands to RequestDirector.
		foreach (Request r in RequestList) { ReqDirector.AddRequestItem(r); CD.W($"Added Request \"{r.Id}\" to the RequestList."); }

		//Adds all commands to Discord's listener.
		foreach (Request r in ReqDirector.RequestList) { await Guild.CreateApplicationCommandAsync(r.DiscordCommand); CD.W($"Added Request\"{r.Id}\" to the Listener."); }
		#endregion

		CD.W("Enabling commands handler...");
		CloviCore.SlashCommandExecuted += SlashCommandHandler;
	}
	#endregion

	#region Logger Task
	/// <summary>
	/// A logging task that's executed by the API everytime it needs to log something to the console.
	/// It forwards any logging task to a ConsoleDirector.
	/// </summary>
	private Task Log(LogMessage msg)
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