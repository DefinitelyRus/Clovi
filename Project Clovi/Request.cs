namespace Project_Clovi;

using Discord;
using Discord.WebSocket;

/// <summary>
/// An inheritable abstract Request used as a template to create new Requests.
/// </summary>
public abstract class Request
{
	/// <summary>
	/// This is used to chain back to the parent RequestDirector. It is set automatically when added to a RequestList.
	/// </summary>
	public RequestDirector? Parent { get; internal set; }

	public SlashCommandProperties DiscordCommand { get; internal set; }


	/// <summary>
	/// Constructs a Request object along with a SlashCommandProperties object.
	/// </summary>
	/// <param name="IdArg">The unique identifier for this command.</param>
	/// <param name="DescriptionArg">A description of what this command does.</param>
	/// <param name="OptionsArray">Any parameters needed to run this command.</param>
	/// <param name="HasDefaultPermission">Sets the default permission of this command.</param>
	/// <param name="HasDMPermission">Whether this command can be used in DMs.</param>
	/// <param name="Perms">Sets the default member permissions to allow use of this command.</param>
	public Request
		(
			String IdArg,
			String DescriptionArg = "Placeholder description; please replace me.",
			SlashCommandOptionBuilder[]? OptionsArray = null,
			Boolean HasDefaultPermission = true,
			Boolean HasDMPermission = true,
			GuildPermission? Perms = null
		)
	{
		Name = IdArg;

		//Builds the slash command with the given attributes and assigns it to DiscordCommand.
		SlashCommandBuilder CommandBuilder = new();
		CommandBuilder
			.WithName(IdArg)
			.WithDescription(DescriptionArg)
			.WithDefaultPermission(HasDefaultPermission)
			.WithDMPermission(HasDMPermission)
			.WithDefaultMemberPermissions(Perms);

		if (OptionsArray is not null) CommandBuilder.AddOptions(OptionsArray);

		DiscordCommand = CommandBuilder.Build();
	}

	/// <summary>
	/// Constructs a Request object using a premade SlashCommandProperties object.
	/// </summary>
	/// <param name="CommandArg">A premade Discord command.</param>
	public Request(SlashCommandProperties CommandArg)
	{
		Name = (String) CommandArg.Name;
		DiscordCommand = CommandArg;
	}
	/// <summary>
	/// The unique identifier for this command.
	/// </summary>
	public String Name { get; set; }

	/// <summary>
	/// Executes the custom request command.
	/// The Dictionary argument should be retrieved from this Request.Params, then modify its values.
	/// </summary>
	/// <param name="Args">Any arguments required to execute the request. Typically this Request.Param with modified values.</param>
	/// <returns>This Request for method chaining.</returns>
	public abstract Request Execute(SocketSlashCommand Cmd, DiscordSocketClient Core);
}
