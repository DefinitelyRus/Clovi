namespace Project_Clovi;

using Discord.WebSocket;

/// <summary>
/// Manages the input and output of commands and logs/responses respectively.
/// This class in itself cannot do anything and must be implemented.
/// </summary>
public abstract class IODirector
{
	/// <summary>
	/// Constructor for any IODirector instance.
	/// </summary>
	/// <param name="CoreArg">The host's  DiscordSocketClient Core.</param>
	/// <param name="ReqDirectorArg">The host's RequestDirector.</param>
	/// <param name="IdArg">A unique identifier for this IODirector instance.</param>
	public IODirector(DiscordSocketClient CoreArg, RequestDirector ReqDirectorArg, String IdArg = "ConsoleApp")
	{
		Core = CoreArg;
		ReqDirector = ReqDirectorArg;
		Id = IdArg;
	}

	/// <summary>
	/// The host's  DiscordSocketClient Core.
	/// </summary>
	internal DiscordSocketClient Core { get; set; }

	/// <summary>
	/// The host's RequestDirector.
	/// </summary>
	internal RequestDirector ReqDirector { get; set; }

	/// <summary>
	/// A unique identifier for this IODirector instance.
	/// </summary>
	public String Id { get; set; }

	/// <summary>
	/// Prints the input string to the target destination.
	/// </summary>
	/// <param name="Text">The text to be printed.</param>
	public abstract void Print(String Text);

	/// <summary>
	/// Receives any user inputs from the I/O destination.
	/// </summary>
	/// <param name="Text">The text inputted by the user.</param>
	public abstract void Input(String Text);
}
