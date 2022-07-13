using Discord;
using Discord.WebSocket;

namespace Project_Clovi
{
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
		/// The unique identifier for this command.
		/// </summary>
		public String Id { get; set; }

		/// <summary>
		/// Constructs a Request object along with a SlashCommandProperties object.
		/// </summary>
		/// <param name="IdArg">The unique identifier for this command.</param>
		/// <param name="DescriptionArg">A description of what this command does.</param>
		/// <param name="ParamsArg">Any parameters needed to run this command.</param>
		/// <param name="HasDefaultPermission">Sets the default permission of this command.</param>
		/// <param name="HasDMPermission">Whether this command can be used in DMs.</param>
		/// <param name="Perms">Sets the default member permissions to allow use of this command.</param>
		public Request
			(
				String IdArg,
				String? DescriptionArg = null,
				SlashCommandOptionBuilder[]? ParamsArg = null,
				Boolean HasDefaultPermission = true,
				Boolean HasDMPermission = true,
				GuildPermission? Perms = null
			)
		{
			Id = IdArg;

			//Builds the slash command with the given attributes and assigns it to DiscordCommand.
			DiscordCommand = new SlashCommandBuilder()
				.WithName(IdArg)
				.WithDescription(DescriptionArg)
				.AddOptions(ParamsArg)
				.WithDefaultPermission(true)
				.WithDMPermission(true)
				.WithDefaultMemberPermissions(null)
				.Build();
		}

		/// <summary>
		/// Constructs a Request object using a premade SlashCommandProperties object.
		/// </summary>
		/// <param name="CommandArg">A premade Discord command.</param>
		public Request(SlashCommandProperties CommandArg)
		{
			Id = (String) CommandArg.Name;
			DiscordCommand = CommandArg;
		}

		/// <summary>
		/// Executes the custom request command.
		/// The Dictionary argument should be retrieved from this Request.Params, then modify its values.
		/// </summary>
		/// <param name="Args">Any arguments required to execute the request. Typically this Request.Param with modified values.</param>
		/// <returns>This Request for method chaining.</returns>
		public abstract Request Execute(SocketSlashCommand Cmd);
	}
}
