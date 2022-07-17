using Discord;
using Discord.WebSocket;

namespace Project_Clovi.Requests;

public class GetLatency : Request
{
	public GetLatency
		(
		String IdArg = "get-latency",
		String DescriptionArg = "Gets the delay between receiving commands and responding to commands.",
		SlashCommandOptionBuilder[]? ParamsArg = null,
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = true,
		GuildPermission? Perms = null
		)

		: base(IdArg, DescriptionArg, ParamsArg, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient? Core = null)
	{
		//Optional prerequisites. Delete or modify only if necessary.

		//Code goes here.
		Command.RespondAsync($"Latency: {Core.Latency}ms");


		//---------------

		//Optional post-fixed instructions. Delete or modify only if necessary.
		return this;
	}
}
