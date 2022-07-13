using Discord.WebSocket;

namespace Project_Clovi;

public class RequestHandler
{
	async Task SlashCommandHandler(SocketSlashCommand Command)
	{
		Console.WriteLine($"Running request \"{Command.CommandName}\"...");
		await Command.RespondAsync($"Running request \"{Command.CommandName}\"...");
		await Command.RespondAsync($"Latency:"); //CloviCore is unaccessible from outside of CloviHost. How do I fix that? Getter method?

	}
}
