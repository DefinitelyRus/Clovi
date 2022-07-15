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

		//Code goes here.
		Command.RespondAsync($"Latency: {Core.Latency}ms");

		//Do not modify unless necessary.
		//TODO: Add logger call here and have it send a success message.

		return this;
	}
}
