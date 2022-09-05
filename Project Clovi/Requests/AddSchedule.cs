namespace Project_Clovi.Requests;

using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Text;

public class AddSchedule : Request
{
	public AddSchedule
		(
		String IdArg = "remind",
		String DescriptionArg = "Adds a new schedule to the reminder calendar.",
		List<Dictionary<string, object?>>? OptionDictionaryList = null,
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = false,
		GuildPermission? Perms = null
		)

		: base(IdArg, DescriptionArg, OptionDictionaryList, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient Core)
	{
		/*
		 * Parameters:
		 * - Title (Max 160 chars)
		 * - Desc (Optional; Max 1000 chars)
		 * - Start On Date (Optional; String; Parse to TimeUnit):
		 *		Format: 12/31/99 or 12/31 
		 * - End On Date (Optional; String; Parse to TimeUnit):
		 *		Format: MM/DD/YY or MM/DD
		 * - Start On Time (Optional; String; Parse to TimeUnit):
		 *		Format: 23:59 or 11:59PM
		 *		Default: 00:00
		 * - End On Time (Optional; String; Parse to TimeUnit):
		 *		Format: 23:59 or 11:59PM
		 *		Default: 00:00
		 */
		ConsoleDirector CD = CloviHost.ConDirector;
		CD.W($"User {Command.User.Username} used command {this.Name}...");
		SQLiteDirector DBDir = CloviHost.SQLDirector;
		SQLiteDatabase DB = DBDir.GetDatabase("GuildsData");
		SqliteDataReader Reader;

		#region Database Check
		CD.W("Attempting to add schedule to the database...");
		DB.Connection.Open();
		try
		{
			CD.W("Checking database validity...");
			Reader = DB.Query("SELECT sched_name FROM schedule_reminder WHERE sched_name = \"test_sched\"");
			if (Reader.Read()) CD.W($"Found \"{Reader.GetFieldValue<String>(1)}\".");
		}
		catch (Exception e1)
		{
			CD.W(e1.Message);
			CD.W("Attempting to create a new table...");
			try
			{
				//Creating the schedule_reminder table.
				DB.Execute(
					"CREATE TABLE schedule_reminder" +
					"(" +
						"\"onlykeys\" INTEGER NOT NULL," +
						"\"sched_name\" TEXT NOT NULL," +
						"\"sched_desc\" TEXT," +
						"\"sched_start\" TEXT NOT NULL," +
						"\"sched_end\" TEXT," +
						"\"sched_creation\"TEXT NOT NULL," +
						"PRIMARY KEY(\"onlykeys\" AUTOINCREMENT)" +
					")");

				//Creating the tester entry.
				DB.Execute(
					"INSERT INTO schedule_reminder" +
					"( sched_name, sched_desc, sched_start, sched_end, sched_creation )" +
					"VALUES (" +
						"\"test_sched\"," +
						"\"test_desc\"," +
						"datetime(2001-01-01 00:00)," +
						"datetime(2999-12-31 23:59)," +
						"datetime(\"now\"))");
			}
			catch (Exception e2)
			{
				CD.W(e2.ToString());
				Command.RespondAsync("An error was encountered while accessing the database. Please tell the developer about this.");
				return this;
			}

			CD.W("New table created. Creating new entry...");
		}
		#endregion

		#region Assigning values to variables.
		String SchedName = "Untitled Schedule", SchedDesc = "",
			StartDateString = "01/01/01", StartTimeString = "00:00", ParsedStartDateTime;
		String? EndDateString = null, EndTimeString = "00:00", ParsedEndDateTime;

		foreach (SocketSlashCommandDataOption option in Command.Data.Options)
		{
			switch (option.Name)
			{
				case "name":
					SchedName = (String)option.Value;
					break;
				case "description":
					SchedDesc = (String)option.Value;
					break;
				case "start-date":
					StartDateString = (String)option.Value;
					break;
				case "start-time":
					StartTimeString = (String)option.Value;
					break;
				case "end-date":
					EndDateString = (String)option.Value;
					break;
				case "end-time":
					EndTimeString = (String)option.Value;
					break;
			}

		}
		CD.W(	
			$"SchedName: {SchedName}\n" +
			$"SchedDesc: {SchedDesc}\n" +
			$"StartDate: {StartDateString}\n" +
			$"StartTime: {StartTimeString}\n" +
			$"EndDate: {EndDateString}\n" +
			$"EndTime: {EndTimeString}"
		);

		bool AllGood = true;
		byte Division = 0;
		short[] StartDate;
		short[]? EndDate;
		StringBuilder[] StartTimeHolder = new StringBuilder[2] { new(), new() };
		StringBuilder[] EndTimeHolder = new StringBuilder[2] { new(), new() };
		DateTime RightNow = DateTime.Now;
		#endregion

		StartDate = DateFormatter(StartDateString);
		EndDate = (EndDateString == null) ? null : DateFormatter(EndDateString);

		//Cancels if DateFormatter() returns a failed product.
		if (StartDate[0] == 999 || (EndDate != null && EndDate[0] == 999))
		{
			CD.W("An error has caused DateFormatter() to return a fail.");
			Command.RespondAsync("Invalid input. Please use format `MM/DD/YYYY` (year is optional).");
			return this;
		}

		//Cancel if the start month or year has already passed, or if the exact date has already passed.
		if (StartDate[0] < RightNow.Month ||
			StartDate[2] < RightNow.Year ||
			(StartDate[0] <= RightNow.Month && StartDate[1] < RightNow.Day && StartDate[2] <= RightNow.Year))
		{
			string output = $"The start date {StartDate[0]}-{StartDate[1]}-{StartDate[2]} has already passed.";
			CD.W(output);
			Command.RespondAsync(output);
			return this;
		}

		#region Filtering Time Input
		if (StartTimeString.Length == 0)
		{
			StartTimeHolder[0].Append("00");
			StartTimeHolder[1].Append("00");
		}
		else
		{
			CD.W($"Reading StartTime. Value: \"{StartTimeString}\"");
			int index = 0;
			foreach (char c in StartTimeString)
			{
				if (Division > 2) { AllGood = false; break; }
				index++;

				if (StartTimeHolder[Division] == null) StartTimeHolder[Division] = new StringBuilder();

				if (c.Equals(':') || c.Equals('-') || index == 3) Division++;
				else if (char.IsNumber(c))
				{
					if (StartTimeHolder[Division].Length >= 4) { AllGood = false; break; }
					else if (StartTimeHolder[Division].Length < 2)
					{
						StartTimeHolder[Division].Append(c);
					}
				}
				else if (char.ToLower(c).Equals('p'))
				{
					byte Time;
					Time = byte.Parse(StartTimeHolder[0].ToString());

					//12:00PM is simply 12:00, not 24:00.
					if (Time != 12) Time += 12;

					//Cancel if Time adds up to 24+. 13PM is not a thing.
					if (Time > 23) { AllGood = false; break; }

					StartTimeHolder[0].Clear();
					StartTimeHolder[0].Append(Time);
				}
			}
			//Cancel if MM is over 59. There can't be 69 minutes in an hour, unfortunately.
			if (Byte.Parse(StartTimeHolder[1].ToString()) > 59) { AllGood = false; }
		}
		#endregion

		#region Condensing to DateTime
		ParsedStartDateTime = $"{StartDate[0]}-{StartDate[1]}-{StartDate[2]} {StartTimeHolder[0]}:{StartTimeHolder[1]}";
		if (EndDate == null) ParsedEndDateTime = null;
		else
		{
			if (EndTimeString.Length == 0)
			{
				EndTimeHolder[0].Append("00");
				EndTimeHolder[1].Append("00");
			}
			ParsedEndDateTime = $"{EndDate[0]}-{EndDate[1]}-{EndDate[2]} {EndTimeHolder[0]}:{EndTimeHolder[1]}";
		}
		#endregion

		if (AllGood)
		{
			DB.Execute("INSERT INTO schedule_reminder" +
				"( sched_name, sched_desc, sched_start, sched_end, sched_creation )" +
				"VALUES (" +
					$"\"{SchedName}\"," +
					$"\"{SchedDesc}\"," +
					$"\"{ParsedStartDateTime}\"," +
					$"\"{ParsedEndDateTime}\"," +
					$"datetime(\"now\"))");
			CD.W($"SUCCESS: \"{this.Name}\" request by {Command.User.Username}");
		}
		else
		{
			CD.W($"FAIL: \"{this.Name}\" request by {Command.User.Username}");
			Command.RespondAsync("An error has been encounted. Please inform the developer.");
		}

		DB.Connection.Close();
		return this;
	}

	static private short[] DateFormatter(string? DateInput)
	{
		short[] FailReturn = new short[] { 999, 999, 999 };
		StringBuilder[] DateSliced = new StringBuilder[3];
		byte Division = 0;

		CD.W($"Reading DateInput. Value: \"{DateInput}\"");

		if (DateInput == null)
		{
			CD.W("DateInput is null. Returning a bad array...");
			return FailReturn;
		}
		#region Division of date units
		/*
		 * The purpose of this loop is to separate the month, day, and year
		 * units into their own individual StringBuilders.
		 * This makes cleaning the inputs much simpler.
		 */
		foreach (char c in DateInput)
		{
			//Cancels if it detects a 4th division.
			// "MM-DD-YY-??" <-- 4th division doesn't exist and causes OutOfBounds Index
			if (Division > 3)
			{
				CD.W("There are more than 3 divisions in the input. Cancelling...");
				return FailReturn;
			}

			// Moves to the next division when these chars are detected.
			// "MM" -> "DD" -> "YY"
			if (c.Equals('/') || c.Equals('-')) Division++;

			else if (Char.IsNumber(c))
			{
				//Instantiates a new instance of StringBuilder if it's currently null.
				//This is for when the division doesn't change but
				//there are more characters to be added to the same division.
				if (DateSliced[Division] == null) DateSliced[Division] = new StringBuilder();

				//Cancels the operation if MM or DD is already 2+ chars long OR if YY is already 4+ characters long.
				if (DateSliced[Division].Length >= 2 || DateSliced.Length >= 4)
				{
					CD.W("The month and/or date value(s) exceed 2 digits. Cancelling...");
					return FailReturn;
				}

				//Else (if no issues are detected), add character to current division.
				DateSliced[Division].Append(c);
			}
		}
		#endregion

		DateTime DateToday = DateTime.Today;

		//If YY is null, set it to the current year.
		if (DateSliced[2] == null)
		{
			DateSliced[2] = new();
			DateSliced[2].Append(DateToday.Year);
		}

		int MM, DD, YY;

		//Tries to parse the date strings into integers.
		try
		{
			MM = int.Parse(DateSliced[0].ToString());
			DD = int.Parse(DateSliced[1].ToString());
			YY = int.Parse(DateSliced[2].ToString());
		}
		catch (NullReferenceException)
		{
			//Triggers when inputting non-numerical characters.
			CD.W("Invalid input.");
			return FailReturn;
		} 

		//If MM is more than 12, set it to 12.
		if (MM > 12)
		{
			DateSliced[0].Remove(0, 2);
			DateSliced[0].Append("12");
			MM = 12; //Necessary for the next IF block.
		}

		//If DD exceeds the number of days in its respective month, set it to the maximum.
		if (DD > DateTime.DaysInMonth(YY, MM))
		{
			DateSliced[1].Remove(0, 2);
			DateSliced[1].Append(DateTime.DaysInMonth(YY, MM));
		}

		//If the set year is less than 4 chars, make it 4 chars.
		else if (DateSliced[2].Length < 4)
		{
			string yearHolder = DateSliced[2].ToString()[^2..]; //Last 2 chars of this string.
			DateSliced[2] = new();
			DateSliced[2].Append($"20{yearHolder}");
		}

		short[] Result = new short[]
		{
			short.Parse(DateSliced[0].ToString()),
			short.Parse(DateSliced[1].ToString()),
			short.Parse(DateSliced[2].ToString()),
		};
		return Result;
	}
}
