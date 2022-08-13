namespace Project_Clovi;

using Discord.WebSocket;
using System.Text;

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
		TimeNow = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
		String Output = $"{TimeNow} >> {Text}";
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
		if (!IsOnline) { Print(Text); return; }


			if (PendingLog.Length == 0) Channel.SendMessageAsync($"```{Output}```");
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
		else
		WaitingForQueue = true;
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
	public void Input(string Text)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// An alias for ConsoleDirector.Print(). It simply logs the input string to the console, along with the timestamp.
	/// </summary>
	/// <param name="Text"></param>
	public void W(String Text) { Print(Text); }
}
