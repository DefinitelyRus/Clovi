namespace Project_Clovi;

using Discord.WebSocket;
using System.Text;
using System.Diagnostics;

/// <summary>
/// Handles the input and output of information to and from the console.
/// </summary>
public class ConsoleDirector
{
	/// <summary>
	/// Creates a new instance of ConsoleDirector.
	/// </summary>
	public ConsoleDirector()
	{
		IsOnline = false;
		PendingLog = new();
	}

	/// <summary>
	/// A static variable used to hold the time string.
	/// Its default value is always replaced with the current time under normal circumstances.
	/// </summary>
	private static String TimeNow = "CriticalTimeError";

	internal List<ulong> ChannelIdList = new();

	internal bool IsOnline { get; set; }

	private bool WaitingForQueue { get; set; }

	private StringBuilder PendingLog { get; }

	/// <summary>
	/// Prints the input string to the console along with a timestamp.
	/// </summary>
	/// <param name="Text"></param>
	[Obsolete("This method is for redundancy only. Use ConsoleDirector.W() instead.", false)]
	public void Print(String Text)
	{
		StackTrace Source = new();
		TimeNow = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
		#pragma warning disable CS8602
		String Output = $"{TimeNow} {Source.GetFrame(3).GetMethod().Name}() >> {Text}";
		#pragma warning restore CS8602
		Console.WriteLine($"{Output}\n");

		//Prints the log to the target channel.
		//TODO: Accumulate all failed prints, then send in one go.
		//Identify where to finally send the logs.
		if (!IsOnline)
		{
			PendingLog.AppendLine(Output);
		}
		else
		{
			foreach (ulong id in ChannelIdList)
			{
				SocketTextChannel Channel = (SocketTextChannel)CloviHost.CloviCore.GetChannel(id);

				//If PendingLog is empty, send Output only.
				if (PendingLog.Length == 0) Channel.SendMessageAsync($"```{Output}```");
				else //else, append Output to PendingLog, then clear PendingLog.
				{
					Channel.SendMessageAsync($"```{PendingLog}{Output}```");
					PendingLog.Clear();
				}
			}
		}
	}

	/// <summary>
	/// Logs the inserted text to the console, target Discord channel(s), and console log files.
	/// </summary>
	/// <param name="Text"></param>
	/// <param name="IsFinal"></param>
	[Obsolete("This method is for redundancy only. Use ConsoleDirector.W() instead.", false)]
	public void Log(String Text, bool IsFinal = false)
	{
		if (!IsOnline) { Print(Text); return; }

		if (IsFinal) WaitingForQueue = false;

		StackTrace Src = new();
		TimeNow = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
		#pragma warning disable CS8602
		String Output = $"{TimeNow} {Src.GetFrame(2).GetMethod().Name}() >> {Text}";
		#pragma warning restore CS8602

		//Prints to console.
		Console.WriteLine($"{Output}\n");

		try
		{
			//If WaitingForQueue, add to PendingLog.
			//Else, send the whole PendingLog and the current Output to the target channel(s).
			if (WaitingForQueue) PendingLog.AppendLine(Output);
			else
			{
				Console.WriteLine($"Count {ChannelIdList.Count.ToString()}"); //TEMP

				foreach (ulong id in ChannelIdList)
				{
					if (id == 0) continue;

					Console.WriteLine($"ID: {id.ToString()}"); //TEMP

					SocketTextChannel Channel = (SocketTextChannel) CloviHost.CloviCore.GetChannel(id);

					//If PendingLog is empty...
					if (PendingLog.Length == 0) Channel.SendMessageAsync($"```{Output}```");
					else
					{
						Channel.SendMessageAsync($"```{PendingLog}{Output}```");
						PendingLog.Clear();
					}
				}
				WaitingForQueue = true;
			}
		}
		catch (Exception e) { Console.WriteLine(e.ToString()); }
	}

	public void SendLog()
	{
		WaitingForQueue = true;
		TimeNow = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";

		SocketTextChannel Channel;
		foreach (ulong id in ChannelIdList)
		{
			Channel = (SocketTextChannel) CloviHost.CloviCore.GetChannel(id);
			Channel.SendMessageAsync($"```{PendingLog}```");
		}

		PendingLog.Clear();
	}

	/// <summary>
	/// NOT IMPLEMENTED.
	/// The standard console cannot receive inputs.
	/// This method will remain unimplemented for this version.
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public void Input(string Text)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// An alias for ConsoleDirector.Log(). It simply logs the input string to the console, along with the timestamp and parent method.
	/// </summary>
	/// <param name="Text"></param>
	#pragma warning disable CS0618
	public void W(String Text, bool IsFinal = false) => Log(Text, IsFinal);
	#pragma warning restore CS0618
}
