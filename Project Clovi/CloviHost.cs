namespace Project_Clovi;

using Discord; //TODO: I don't want to import the whole Discord.Net API. Find an alternative.
using Discord.WebSocket;

public class CloviHost
{
	#region Init
	/// <summary>
	/// The core socket client responsible for handling all interactions between the front-end and back-end.
	/// </summary>
	public readonly DiscordSocketClient CloviCore = new();

	public RequestDirector Director = new(new List<Request>());

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
		//TODO: Direct to ConsoleDirector
		Console.WriteLine(msg.ToString());
		return Task.CompletedTask;
	}

	public async Task ClientReady()
	{
		ulong GuildId = 262784778690887680; //!!! TEMPORARY; must be saved on a case-by-case basis.
		SocketGuild Guild = CloviCore.GetGuild(GuildId);
		LinkedList<Request> RequestList = new();

		#region Standard Request Library
		//Retrieves all standard Request library Requests. (i.e. The premade requests.)

		RequestList.AddLast(new Requests._RequestTemplate());
		#endregion

		#region Addon Requests
		//TODO: Retrieve the array from a JSON file. This contains any custom commands.
		//NOTE: Do not store Arrays directly in JSONs. A good option is to store an Array as a value in a Dictionary.
		#endregion

		//Adds custom commands to RequestDirector.
		foreach (Request r in RequestList) Director.AddRequestItem(r);

		//Adds all commands to Discord's listener.
		foreach (Request r in Director.RequestList) await Guild.CreateApplicationCommandAsync(r.DiscordCommand);

		//Activated when a command is received.
		CloviCore.SlashCommandExecuted += SlashCommandHandler;
	}

	#region Fluff
	//How can it tell which command is being executed?
	//It can't. Use cmd.CommandName to identify.
	private async Task SlashCommandHandler(SocketSlashCommand cmd)
	{
		Director.GetRequest(cmd.CommandName).Execute(cmd);
		
		
	}
	#endregion
}