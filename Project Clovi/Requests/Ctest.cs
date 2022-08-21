namespace Project_Clovi.Requests;

using Discord;
using Discord.WebSocket;

public class Ctest : Request
{
	public Ctest
		(
		String IdArg = "ctest",
		String DescriptionArg = "A debugging tool used for testing new commands.",
		List<Dictionary<string, object?>>? OptionDictionaryList = null,
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = true,
		GuildPermission? Perms = null
		)

		: base(IdArg, DescriptionArg, OptionDictionaryList, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient Core)
	{
		//Optional prerequisites. Delete or modify only if necessary.
		ConsoleDirector CD = CloviHost.ConDirector; //Use CD.W(_) for logging.

		//Code goes here.

		//--------------

		//Optional post-fixed instructions. Delete or modify only if necessary.
		CD.W($"SUCCESS: \"{this.Name}\" request by {Command.User.Username}", true);
		return this;
	}
}
