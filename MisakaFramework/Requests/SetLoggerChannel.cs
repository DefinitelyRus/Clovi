namespace MisakaFramework.Requests;

using Discord;
using Discord.WebSocket;

public class SetLoggerChannel : Request
{
	readonly string Password = "Bugzapper"; //Changes when I feel like changing it. -Rus
	public SetLoggerChannel
		(
		String IdArg = "setloggerchannel",
		String DescriptionArg = "Assigns a text channel as a destination for all console log messages. For developers only.",
		List<Dictionary<string, object?>>? OptionDictionaryList = null, //Initialized in CloviHost.cs.
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = false,
		GuildPermission? Perms = null
		)

		: base(IdArg, DescriptionArg, OptionDictionaryList, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient Core)
	{
		ConsoleManager CD = MisakaCore.ConDirector;
		CD.W($"User {Command.User.Username} used command {this.Name}...");
		SocketTextChannel? Channel = null;
		string? InputPassword = null;
		foreach (SocketSlashCommandDataOption option in Command.Data.Options)
		{
			if (option.Name.Equals("channel")) Channel = (SocketTextChannel) option.Value;
			else if (option.Name.Equals("dev-password")) InputPassword = (string) option.Value;
		}

		#region Command cancellations.
		if (Channel is null || InputPassword is null)
		{
			CD.W("Invalid/missing input(s). Cancelling...");
			Command.RespondAsync("Invalid/missing input(s).");
			return this;
		}
		else if (!InputPassword.Equals(Password))
		{
			CD.W("Invalid password. Cancelling...");
			Command.RespondAsync("Incorrect password. The password can be found in the source code of this command.");
			return this;
		}
		#endregion

		if (MisakaCore.SQLDirector.GetType() == typeof(DatabaseManager))
		{
			DatabaseManager Director = (DatabaseManager) MisakaCore.SQLDirector;

			SQLiteDatabase Database = Director.GetDatabase("GuildsData");
			Database.Connection.Open();
			Database.Execute($"UPDATE guilds_settings SET setting_value = \"{Channel.Id}\" WHERE guild_id = \"{Command.GuildId}\" AND setting_name = \"LoggerChannelId\"");
			Database.Connection.Close();

			Command.RespondAsync($"Set log destination to {Channel.Mention}. The changes will be applied on next restart.");
		}

		CD.W($"SUCCESS: \"{this.Name}\" request by {Command.User.Username}");
		return this;
	}
}
