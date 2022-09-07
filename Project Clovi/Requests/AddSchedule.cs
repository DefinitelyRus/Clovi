namespace Project_Clovi.Requests;

using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Text;

public class AddSchedule : Request
{
	public AddSchedule
		(
		string IdArg = "remind",
		string DescriptionArg = "Adds a new schedule to the reminder calendar.",
		List<Dictionary<string, object?>>? OptionDictionaryList = null,
		Boolean HasDefaultPermission = true,
		Boolean HasDMPermission = false,
		GuildPermission? Perms = null
		)

		: base(IdArg, DescriptionArg, OptionDictionaryList, HasDefaultPermission, HasDMPermission, Perms) { }

	public override Request Execute(SocketSlashCommand Command, DiscordSocketClient Core)
	{
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
		string?
			SchedName = "Untitled Schedule",
			SchedDesc = "",
			StartDateString,
			StartTimeString = "00:00",
			EndDateString,
			EndTimeString,
			ParsedStartDateTime,
			ParsedEndDateTime;

		foreach (SocketSlashCommandDataOption option in Command.Data.Options)
		{
			switch (option.Name)
			{
				case "name":
					SchedName = (string)option.Value;
					break;
				case "description":
					SchedDesc = (string)option.Value;
					break;
				case "start-date":
					StartDateString = (string)option.Value;
					break;
				case "start-time":
					StartTimeString = (string)option.Value;
					break;
				case "end-date":
					EndDateString = (string)option.Value;
					break;
				case "end-time":
					EndTimeString = (string)option.Value;
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
		short[] StartDate;
		short[]? EndDate;
		short[] StartTime;
		short[] EndTime;
		DateTime RightNow = DateTime.Now;
		#endregion

		#region Date Formatting
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
		#endregion

		#region Filtering Time Input
		StartTime = TimeFormatter(StartTimeString);
		EndDate = TimeFormatter(EndDateString);
		#endregion

		#region Condensing to DateTime
		ParsedStartDateTime = $"{StartDate[0]}-{StartDate[1]}-{StartDate[2]} {StartTime[0]}:{StartTime[1]}";
		if (EndDate == null) ParsedEndDateTime = null;
		else
		{
			if (EndTimeString.Length == 0)
			{
				StartTime[0].Append("00");
				StartTime[1].Append("00");
			}
			ParsedEndDateTime = $"{EndDate[0]}-{EndDate[1]}-{EndDate[2]} {StartTime[0]}:{StartTime[1]}";
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

	static private short[] TimeFormatter(string? TimeInput)
	{
		CD.W($"Reading StartTime... Value: \"{TimeInput}\"");

		//To be returned if an error was encountered.
		short[] FailReturn = new short[] { 999, 999 };

		//A temporary container where to put the formatted time units.
		StringBuilder[] TimeSliced = new StringBuilder[2];

		//The index that divides the HH and MM units. Cancel if there are more than 2 divisions.
		byte Division = 0;

		if (TimeInput == null || TimeInput.Length == 0)
		{
			TimeSliced[0] = new StringBuilder("00");
			TimeSliced[1] = new StringBuilder("00");
			TimeInput = "";
		}

		foreach (char c in TimeInput)
		{
			if (Division > 2) { return FailReturn; }

			if (c.Equals(':') || c.Equals('-')) Division++;

			else if (char.IsNumber(c))
			{
				//Create a new instance if not already created.
				if (TimeSliced[Division] == null) TimeSliced[Division] = new StringBuilder();

				//Cancels if the length is already 2+ chars long.
				if (TimeSliced[Division].Length >= 2)
				{
					CD.W("TimeFormatter does not support seconds and cannot format more than 2 divisions.");
					return FailReturn;
				}

				//Else (if no issues are detected), add character to current division.
				else TimeSliced[Division].Append(c);
			}

			//"09:00PM" -> "21:00".
			else if (char.ToLower(c).Equals('p'))
			{
				byte Time;
				try { Time = byte.Parse(TimeSliced[0].ToString()); }
				catch { Time = 0; }

				//12:00PM is simply 12:00, not 24:00.
				if (Time != 12) Time += 12;

				if (Time > 23)
				{
					CD.W("Cannot set to more than 23 hours.");
					return FailReturn;
				}

				TimeSliced[0].Clear();
				TimeSliced[0].Append(Time);
			}
		}

		//In case these somehow end up remaining null...
		if (TimeSliced[0] == null) { TimeSliced[0] = new StringBuilder("00"); }
		if (TimeSliced[1] == null) { TimeSliced[1] = new StringBuilder("00"); }

		//Cancel if HH is over 23.
		if (Byte.Parse(TimeSliced[0].ToString()) > 23)
		{
			CD.W("Hour unit cannot be over 23.");
			return FailReturn;
		}

		//Cancel if MM is over 59.
		if (Byte.Parse(TimeSliced[1].ToString()) > 59)
		{
			CD.W("Minute unit cannot be over 59.");
			return FailReturn;
		}

		short[] Result = new short[]
		{
			short.Parse(TimeSliced[0].ToString()),
			short.Parse(TimeSliced[1].ToString())
		};
		return Result;
	}
}
