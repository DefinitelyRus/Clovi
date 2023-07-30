namespace MisakaFramework;

using Discord;
using Discord.WebSocket;

/// <summary>
/// An inheritable abstract Request used as a template to create new Requests.
/// </summary>
public abstract class Request
{
	#region Constructors
	/// <summary>
	/// Constructs a Request object along with a SlashCommandProperties object.
	/// </summary>
	/// <param name="IdArg">The unique identifier for this command.</param>
	/// <param name="DescriptionArg">A description of what this command does.</param>
	/// <param name="OptionDictionaryList">Any parameters needed to run this command.</param>
	/// <param name="HasDefaultPermission">Sets the default permission of this command.</param>
	/// <param name="HasDMPermission">Whether this command can be used in DMs.</param>
	/// <param name="Perms">Sets the default member permissions to allow use of this command.</param>
	public Request
	(
		String IdArg,
		String DescriptionArg = "Placeholder description; please replace me.",
		List<Dictionary<string, object?>>? OptionDictionaryList = null,
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

		if (OptionDictionaryList is not null)
		{
			String name, description;
			bool isRequired, isDefault, isAutoComplete;
			double? minValue, maxValue;
			List<SlashCommandOptionBuilder> options;
			List<ChannelType> channelTypes;
			ApplicationCommandOptionChoiceProperties[] choices;

			foreach (Dictionary<string, object?> dict in OptionDictionaryList)
			{
				#pragma warning disable CS8600
				#pragma warning disable CS8605
				name = (string) dict["name"];
				ApplicationCommandOptionType optionType = (ApplicationCommandOptionType) dict["optionType"];
				description = (string) dict["description"];
				isRequired = (bool) dict["isRequired"];
				isDefault = (bool) dict["isDefault"];
				isAutoComplete = (bool) dict["isAutoComplete"];
				minValue = (double?) dict["minValue"];
				maxValue = (double?) dict["maxValue"];
				options = (List<SlashCommandOptionBuilder>) dict["options"];
				channelTypes = (List<ChannelType>) dict["channelTypes"];
				choices = (ApplicationCommandOptionChoiceProperties[]) dict["choices"];
				#pragma warning restore CS8600
				#pragma warning restore CS8605

				CommandBuilder.AddOption
				(
					name,
					optionType,
					description,
					isRequired,
					isDefault,
					isAutoComplete,
					minValue,
					maxValue,
					options,
					channelTypes,
					choices
				);
			}
		}

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
	#endregion


	#region Attributes
	/// <summary>
	/// This is used to chain back to the parent RequestManager. It is set automatically when added to a RequestList.
	/// </summary>
	public RequestManager? Manager { get; internal set; }

	/// <summary>
	/// Shortcut to MisakaCore.ConManager;
	/// </summary>
	protected static readonly ConsoleManager CD = MisakaCore.ConManager;

	/// <summary>
	/// The unique identifier for this command.
	/// </summary>
	public String Name { get; set; }

	/// <summary>
	/// Returns a dictionary for use in constructing a new Request.
	/// </summary>
	/// <param name="name">The name of this parameter.</param>
	/// <param name="optionType">The type of input this parameter will allow.</param>
	/// <param name="description">The description of this parameter.</param>
	/// <param name="isRequired">Whether this parameter is required.</param>
	/// <param name="isDefault">Whether this option is the default option.</param>
	/// <param name="isAutoComplete">Whether this option supports auto-complete.</param>
	/// <param name="minValue">The minimum value the user is allowed to input.</param>
	/// <param name="maxValue">The maximum value the user is allowed to input.</param>
	/// <param name="options">The options for this option; a 2nd layer of options.</param>
	/// <param name="channelTypes">The types of channels allowed for this option.</param>
	/// <param name="choices">The choices this option allows.</param>
	/// <returns></returns>
	public static Dictionary<string, object?> GetNewOptionProperties
	(
		string? name = null,
		ApplicationCommandOptionType? optionType = null,
		string? description = null,
		bool? isRequired = null,
		bool? isDefault = null,
		bool? isAutoComplete = null,
		double? minValue = null,
		double? maxValue = null,
		List<SlashCommandOptionBuilder>? options = null,
		List<ChannelType>? channelTypes = null,
		ApplicationCommandOptionChoiceProperties[]? choices = null
	)
	{
		Dictionary<string, object?> dict = new()
		{
			{ "name", (name is null) ? "default-name" : name },
			{ "optionType", (optionType is null) ? ApplicationCommandOptionType.String : optionType},
			{ "description", (description is null) ? "No description set." : description},
			{ "isRequired", (isRequired is null) ? false : isRequired},
			{ "isDefault", (isDefault is null) ? false : isDefault },
			{ "isAutoComplete", (isAutoComplete is null) ? false : isAutoComplete },
			{ "minValue", minValue },
			{ "maxValue", maxValue },
			{ "options" , options },
			{ "channelTypes", channelTypes },
			{ "choices", choices }
		};

		return dict;
	}

	public SlashCommandProperties DiscordCommand { get; internal set; }
	#endregion


	#region Methods
	/// <summary>
	/// Executes the custom request command.
	/// The Dictionary argument should be retrieved from this Request.Params, then modify its values.
	/// </summary>
	/// <param name="Args">Any arguments required to execute the request. Typically this Request.Param with modified values.</param>
	/// <returns>This Request for method chaining.</returns>
	public abstract Request Execute(SocketSlashCommand Cmd, DiscordSocketClient Core);
	#endregion
}
