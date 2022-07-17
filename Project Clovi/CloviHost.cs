namespace Project_Clovi;

using Discord;
using Discord.WebSocket;

public class CloviHost
{
	#region Init
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

	public static Task Main() => new CloviHost().MainAsync();
	#endregion

	public async Task MainAsync()
	{
		//Note: Any Exceptions thrown inside this function will cause the program to crash.

		//How on earth do these work?
		CloviCore.Log += Log; //I think it adds this method to an internal list, then executes it.
		CloviCore.Ready += ClientReady; //Same goes here.

		await CloviCore.LoginAsync(TokenType.Bot, "OTkzMzU3NTI4ODQwODAyMzU0.GFY2sX.Oa6btULKbnk9GYQgPzTtZA7T0_q7sghJxN7MSI"); //!!! Hide this before making the repo public
		await CloviCore.StartAsync(); //Returns immediately after finishing.

		while (true) { }
	}

	private Task Log(LogMessage msg)
	{
		String Message = msg.Message;
		ConDirector.Print(Message);
		return Task.CompletedTask;
	}

	public async Task ClientReady()
	{
		ulong GuildId = 262784778690887680; //!!! TEMPORARY; must be saved on a case-by-case basis.
		SocketGuild Guild = CloviCore.GetGuild(GuildId);
		LinkedList<Request> RequestList = new();

		#region Standard Request Library
		//Retrieves all standard Request library Requests. (i.e. The premade requests.)

		RequestList.AddLast(new Requests.GetLatency());
		#endregion

		#region Addon Requests
		//TODO: Retrieve the array from a JSON file. This contains any custom commands.
		//NOTE: Do not store Arrays directly in JSONs. A good option is to store an Array as a value in a Dictionary.
		#endregion

		//Removes all commands made by this bot in the past.
		//Not the most efficient way to do this, but it'll do for now.
		//Adds custom commands to RequestDirector.
		foreach (Request r in RequestList) ReqDirector.AddRequestItem(r);

		//Adds all commands to Discord's listener.
		foreach (Request r in ReqDirector.RequestList) await Guild.CreateApplicationCommandAsync(r.DiscordCommand);

		//Activated when a command is received.
		CloviCore.SlashCommandExecuted += SlashCommandHandler;
	}

	#region Fluff
	private async Task SlashCommandHandler(SocketSlashCommand cmd)
	{
		Director.ExecuteRequest(cmd, CloviCore);
	}
	#endregion
}