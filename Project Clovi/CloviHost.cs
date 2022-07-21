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

	internal static FileIODirector FIODirector = new();

	internal static String Token = "secret";

	internal static Dictionary<ulong, Object> GuildsData = new();

	public static Task Main() => new CloviHost().MainAsync();
	#endregion

	#region Main Async Task
	/// <summary>
	/// An async main method. Lets the main method run asynchronously.
	/// </summary>
	public async Task MainAsync()
	{
		if (!FIODirector.CheckRequiredFiles()) return;

		CloviCore.Log += Log;
		CloviCore.Ready += ClientReady;

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
		FIODirector.UpdateInstanceData("BotToken", Token);
		Token = "secret";
		File.Delete(FIODirector.Directories[0] + @"\BotToken.txt");
		ulong GuildId = 262784778690887680; //!!! TEMPORARY; must be saved on a case-by-case basis.
		SocketGuild Guild = CloviCore.GetGuild(GuildId);
		LinkedList<Request> RequestList = new();
		#endregion

		#region Standard Request Library
		//Retrieves all standard Request library Requests. (i.e. The premade requests.)

		RequestList.AddLast(new Requests.GetLatency());
		#endregion

		#region Addon Requests
		//TODO: Retrieve the array from a JSON file. This contains any custom commands.
		//NOTE: Do not store Arrays directly in JSONs. A good option is to store an Array as a value in a Dictionary.
		#endregion

		#region Removal & addition of requests.
		//Do this for every guild listed in GuildData.
		//Removes all commands made by this bot in the past.
		//Not the most efficient way to do this, but it'll do for now.
		List<SocketApplicationCommand> c = Guild.GetApplicationCommandsAsync().Result.ToList();
		foreach (SocketApplicationCommand cmd in c) await cmd.DeleteAsync();

		//Adds custom commands to RequestDirector.
		foreach (Request r in RequestList) ReqDirector.AddRequestItem(r);

		//Adds all commands to Discord's listener.
		foreach (Request r in ReqDirector.RequestList) await Guild.CreateApplicationCommandAsync(r.DiscordCommand);
		#endregion

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
		String Message = msg.Message;
		CD.W(Message);
		return Task.CompletedTask;
	}
	#endregion

	#region Command Handler Task
	/// <summary>
	/// Called when any command is executed.
	/// </summary>
	private Task SlashCommandHandler(SocketSlashCommand cmd)
	{
		ReqDirector.ExecuteRequest(cmd, CloviCore);

		return Task.CompletedTask;
	}
	#endregion
}