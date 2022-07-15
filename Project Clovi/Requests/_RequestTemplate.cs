using Discord;
using Discord.WebSocket;

namespace Project_Clovi.Requests;

public class _RequestTemplate : Request
{
	public _RequestTemplate( String IdArg = "TEMPLATE; PLEASE REFACTOR.", String? DescriptionArg = null, SlashCommandOptionBuilder[]? ParamsArg = null, Boolean HasDefaultPermission = true, Boolean HasDMPermission = true, GuildPermission? Perms = null, Boolean IsContextSensitiveArg = true) : base(IdArg, DescriptionArg, ParamsArg, HasDefaultPermission, HasDMPermission, Perms)
	{
	}

	public override Request Execute(SocketSlashCommand Command)
	{

		//Code goes here.

		//Do not modify unless necessary.
		//TODO: Add logger call here and have it send a success message.

		return this;
	}
}
