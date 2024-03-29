﻿namespace MisakaFramework.Requests;

using Discord;
using Discord.WebSocket;

public class _RequestTemplate : Request
{
	public _RequestTemplate
		(
		String IdArg = "req-template",
		String DescriptionArg = "temp description",
		List<Dictionary<string, object?>>? OptionDictionaryList = null,
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = true,
		GuildPermission? Perms = null
		)

		: base(IdArg, DescriptionArg, OptionDictionaryList, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient Core)
	{
		//Optional prerequisites. Delete or modify only if necessary.
		ConsoleManager CD = MisakaCore.ConManager; //Use CD.W(...) for logging.
		CD.W($"User {Command.User.Username} used command {this.Name}...");
		//IMPORTANT: Do not use CD.W("...", true) in this method unless absolutely necessary.

		//Code goes here.

		//---------------

		//Optional post-fixed instructions. Delete or modify only if necessary.
		CD.W($"SUCCESS: \"{this.Name}\" request by {Command.User.Username}");
		return this;
	}
}
