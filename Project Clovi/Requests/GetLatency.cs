namespace Project_Clovi.Requests;

using Discord;
using Discord.WebSocket;

public class GetLatency : Request
{
	public GetLatency
		(
		String IdArg = "ping",
		String DescriptionArg = "Gets the delay between receiving commands and responding to commands.",
		List<Dictionary<string, object?>>? OptionDictionaryList = null,
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = true,
		GuildPermission? Perms = null
		)

		: base(IdArg, DescriptionArg, OptionDictionaryList, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient Core)
	{	
		int Ping = Core.Latency;
		Command.RespondAsync($"Latency: {Ping}ms");

		CD.W($"SUCCESS: \"{this.Name}\" request by {Command.User.Username}: {Ping}ms");
		return this;
	}
}
