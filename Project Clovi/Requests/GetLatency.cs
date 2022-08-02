namespace Project_Clovi.Requests;

using Discord;
using Discord.WebSocket;

public class GetLatency : Request
{
	public GetLatency
		(
		String IdArg = "ping",
		String DescriptionArg = "Gets the delay between receiving commands and responding to commands.",
		SlashCommandOptionBuilder[]? ParamsArg = null,
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = true,
		GuildPermission? Perms = null
		)

		: base(IdArg, DescriptionArg, ParamsArg, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient Core)
	{
		//Optional prerequisites. Delete or modify only if necessary.
		ConsoleDirector CD = CloviHost.ConDirector;

		//Code goes here.

		int Ping = Core.Latency;
		Command.RespondAsync($"Latency: {Ping}ms");

		//---------------

		//Optional post-fixed instructions. Delete or modify only if necessary.
		CD.W($"SUCCESS: \"{this.Id}\" request by {Command.User.Username}: {Ping}ms");

		//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		CloviHost.SQLDirector.Execute("GuildsData", "INSERT INTO guilds_settings (guild_id, setting_name, setting_value) values (999, \"testname\", \"testvalue\")");
		return this;
	}
}
