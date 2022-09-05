﻿namespace Project_Clovi.Requests;

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

		String SchedName = "Untitled Schedule", SchedDesc = "",
			StartDate = "01/01/01", StartTime = "00:00", ParsedStartDateTime = "BAD TIME";
		String? EndDate = null, EndTime = null, ParsedEndDateTime = null;

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
					StartDate = (String)option.Value;
					break;
				case "start-time":
					StartTime = (String)option.Value;
					break;
				case "end-date":
					EndDate = (String)option.Value;
					break;
				case "end-time":
					EndTime = (String)option.Value;
					break;
			}

		}
		bool AllGood = true;
		byte Division = 0;
		short[] StartDateHolder = new short[3];
		short[]? EndDateHolder = new short[3];
		StringBuilder[] StartTimeHolder = new StringBuilder[2] { new(), new() };
		StringBuilder[] EndTimeHolder = new StringBuilder[2] { new(), new() };
		#endregion

		StartDateHolder = DateFormatter(StartDate);
		EndDateHolder = (EndDate == null) ? null : DateFormatter(EndDate);

		#region Filtering Time Input
		if (StartTime == null || StartTime.Length == 0)
		{
			StartTimeHolder[0].Append("00");
			StartTimeHolder[1].Append("00");
		}
		else
		{
			CD.W($"Reading StartTime. Value: \"{StartTime}\"");
			int index = 0;
			foreach (char c in StartTime)
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
		ParsedStartDateTime = $"{StartDateHolder[0]}-{StartDateHolder[1]}-{StartDateHolder[2]} {StartTimeHolder[0]}:{StartTimeHolder[1]}";
		if (EndDate == null) ParsedEndDateTime = null;
		else
		{
			if (EndTime == null) { EndTimeHolder[0].Append("00"); EndTimeHolder[1].Append("00"); }
			ParsedEndDateTime = $"{EndDateHolder[0]}-{EndDateHolder[1]}-{EndDateHolder[2]} {EndTimeHolder[0]}:{EndTimeHolder[1]}";
		}
		#region Condensing to DateTime

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

		int MM = int.Parse(DateSliced[0].ToString());
		int DD = int.Parse(DateSliced[1].ToString());
		int YY = int.Parse(DateSliced[2].ToString());

		//If MM is more than 12, set it to 12.
		if (MM > 12)
		{
			DateSliced[0].Remove(0, 2);
			DateSliced[0].Append("12");
		}

		//If DD exceeds the number of days in its respective month, set it to the maximum.
		if (DD > DateTime.DaysInMonth(YY, MM))
		{
			DateSliced[2].Remove(0, 2);
			DateSliced[2].Append(DateTime.DaysInMonth(YY, MM));
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
