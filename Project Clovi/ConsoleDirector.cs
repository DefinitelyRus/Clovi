﻿namespace Project_Clovi;

using Discord.WebSocket;
using System.Text;
using System.Diagnostics;

/// <summary>
/// Handles the input and output of information to and from the console.
/// </summary>
public class ConsoleDirector
{
	#region Constructor
	/// <summary>
	/// Creates a new instance of ConsoleDirector.
	/// </summary>
	public ConsoleDirector()
	{
		IsOnline = false;
		PendingLog = new();
	}
	#endregion

	#region Attributes
	/// <summary>
	/// A static variable used to hold the time string.
	/// Its default value is always replaced with the current time under normal circumstances.
	/// </summary>
	private static String TimeNow = "CriticalTimeError";

	/// <summary>
	/// A list of channel IDs belonging to SocketTextChannels assigned to receive log messages.
	/// </summary>
	internal List<ulong> LogChannelIdList = new();

	/// <summary>
	/// Whether CloviCore is online and can send messages on Discord.
	/// </summary>
	internal bool IsOnline { get; set; }

	//NOTE: Might not be necessary at all. Consider for removal.
	/// <summary>
	/// Decides if the PendingLog should be sent (false) or not (true).
	/// </summary>
	private bool WaitingForQueue { get; set; }

	/// <summary>
	/// A collection of log messages combined into one string.
	/// </summary>
	private StringBuilder PendingLog { get; }
	#endregion

	#region Methods
	/// <summary>
	/// Prints the input string to the console along with a timestamp, then optionally adds the message to the PendingLog.
	/// </summary>
	/// <param name="Text"></param>
	[Obsolete("This method is for clarity only. Use ConsoleDirector.P() instead.", false)]
	public void Print(String Text, bool AddToPendingLog = false)
	{
		StackTrace Source = new();
		TimeNow = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
		#pragma warning disable CS8602
		String Output = $"{TimeNow} {Source.GetFrame(3).GetMethod().Name}() >> {Text}";
		#pragma warning restore CS8602

		Console.WriteLine($"{Output}\n");
		if (AddToPendingLog) PendingLog.AppendLine(Output);
	}

	/// <summary>
	/// Logs the inserted text to the console, target Discord channel(s), and console log files.
	/// If the bot is not online, the message will be forwarded to ConsoleDirector.Print().
	/// </summary>
	/// <param name="Text">The text to be logged.</param>
	/// <param name="IsFinal">Whether to also send the message to the destination(s).</param>
	[Obsolete("This method is for clarity only. Use ConsoleDirector.W() instead.", false)]
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
				foreach (ulong id in LogChannelIdList)
				{
					if (id == 0) continue;

					SocketTextChannel Channel = (SocketTextChannel) CloviHost.CloviCore.GetChannel(id);

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

	/// <summary>
	/// Sends the PendingLog to their destination(s).
	/// </summary>
	public void SendLog()
	{
		if (PendingLog.Length == 0) return;

		WaitingForQueue = true;
		TimeNow = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";

		SocketTextChannel Channel;
		foreach (ulong id in LogChannelIdList)
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

	#region Aliases
	/// <summary>
	/// An alias for ConsoleDirector.Log() and functions identically to Q().
	/// It simply logs the input string to the console, along with the timestamp and parent method.
	/// </summary>
	/// <param name="Text">The text you wish to log.</param>
	#pragma warning disable CS0618
	public void W(String Text, bool IsFinal = false) => Log(Text, IsFinal);


	/// <summary>
	/// An alias for ConsoleDirector.Log() and functions identically to W().
	/// It simply logs the input string to the console, along with the timestamp and parent method.
	/// </summary>
	/// <param name="Text">The text you wish to log.</param>
	public void Q(String Text, bool IsFinal = false) => Log(Text, IsFinal);

	/// <summary>
	/// An alias for ConsoleDirector.Print().
	/// It simply prints the text to console and optionally adds the text to the pending log.
	/// </summary>
	/// <param name="Text"></param>
	/// <param name="AddToPendingLog"></param>
	public void P(String Text, bool AddToPendingLog = false) => Print(Text, AddToPendingLog);
	#pragma warning restore CS0618
	#endregion
	#endregion
}
