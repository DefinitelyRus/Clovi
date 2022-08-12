namespace Project_Clovi;

using Discord.WebSocket;
using System.Text;

/// <summary>
/// Handles the input and output of information to and from the console.
/// </summary>
public class ConsoleDirector : IODirector
{
	/// <summary>
	/// Creates a new instance of ConsoleDirector.
	/// </summary>
	/// <param name="CoreArg"></param>
	/// <param name="ReqDirArg"></param>
	/// <param name="Id"></param>
	public ConsoleDirector(DiscordSocketClient CoreArg, RequestDirector ReqDirArg, String Id) : base(CoreArg, ReqDirArg, Id)
	{
		IsOnline = false;
		PendingLog = new();
	}

	/// <summary>
	/// An overload of the default ConsoleDirector constructor which takes an existing IODirector implementation and uses it as reference.
	/// </summary>
	/// <param name="CDA">An abstracted ConsoleDirector object.</param>
	public ConsoleDirector(IODirector CDA) : base(CDA.Core, CDA.ReqDirector, CDA.Id)
	{
		IsOnline = false;
		PendingLog = new();
	}

	/// <summary>
	/// A static variable used to hold the time string.
	/// Its default value is always replaced with the current time under normal circumstances.
	/// </summary>
	private static String TimeNow = "CriticalTimeError";

	internal bool IsOnline { get; set; }

	private StringBuilder PendingLog { get; }

	/// <summary>
	/// Prints the input string to the console along with a timestamp.
	/// </summary>
	/// <param name="Text"></param>
	public override void Print(String Text)
	{
		TimeNow = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
		String Output = $"{TimeNow} >> {Text}";
		Console.WriteLine($"{Output}\n");

		//Prints the log to the target channel.
		//TODO: Accumulate all failed prints, then send in one go.
		//Identify where to finally send the logs.
		if (IsOnline)
		{
			//TODO: Guild and Channel IDs are to be stored in a DB.
			SocketTextChannel Channel = CloviHost.CloviCore.GetGuild(262784778690887680).GetTextChannel(857171496254308372);


			if (PendingLog.Length == 0) Channel.SendMessageAsync($"```{Output}```");
			else
			{
				Channel.SendMessageAsync($"```{PendingLog}{Output}```");
				PendingLog.Clear();
			}
		}
		else
		{
			PendingLog.AppendLine(Output);
		}
	}

	/// <summary>
	/// NOT IMPLEMENTED.
	/// The standard console cannot receive inputs.
	/// This method will remain unimplemented for this version.
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public override void Input(string Text)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// An alias for ConsoleDirector.Print(). It simply logs the input string to the console, along with the timestamp.
	/// </summary>
	/// <param name="Text"></param>
	public void W(String Text) { Print(Text); }
}
