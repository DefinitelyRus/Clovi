using Discord;
using Discord.WebSocket;

namespace Project_Clovi.Requests;

public class _RequestTemplate : Request
{
	public _RequestTemplate(
		String IdArg = "req-template",
		String DescriptionArg = "temp description",
		SlashCommandOptionBuilder[]? ParamsArg = null,
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = true,
		GuildPermission? Perms = null)

		: base(IdArg, DescriptionArg, ParamsArg, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient? Core = null)
	{
		//Optional prerequisites. Delete or modify only if necessary.

		//Code goes here.

		//---------------

		//Optional post-fixed instructions. Delete or modify only if necessary.
		return this;
	}
}
