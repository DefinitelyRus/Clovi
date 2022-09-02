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
		StringBuilder[] StartDateHolder = new StringBuilder[3];
		StringBuilder[] StartTimeHolder = new StringBuilder[2];
		StringBuilder[]? EndDateHolder = new StringBuilder[3];
		StringBuilder[]? EndTimeHolder = new StringBuilder[2];

		#region Filtering Date Inputs
		CD.W($"Reading StartDate. Value: \"{StartDate}\"");
		foreach (char c in StartDate)
		{
			// "MM-DD-YY-??" <-- 4th division doesn't exist and causes OutOfBounds Index
			if (Division > 3) { AllGood = false; break; }

			// Moves to the next division when these chars are detected.
			// "MM" -> "DD" -> "YY"
			if (c.Equals('/') || c.Equals('-')) Division++;

			else if (Char.IsNumber(c))
			{
				if (StartDateHolder[Division] == null) StartDateHolder[Division] = new StringBuilder();
				//If MM or DD is already 2+ chars long OR if YY is already 4+ characters long, cancel operation.
				if (StartDateHolder[Division].Length >= 2 || (StartDateHolder[Division].Length >= 4 && Division == 3))
				{
					AllGood = false;
					break;
				}

				//Else, add character to current division.
				StartDateHolder[Division].Append(c);
			}
		}
		for (int div = 0; div < StartDateHolder.Length; div++)
		{
			//If YY is of length 0, set it to 2022 by default.
			if (div == 2 && StartDateHolder[2].Length == 0) StartDateHolder[2].Append("2022");
			//TODO: Check current date and compare to target date.
			//		If current date has already passed in the current year, move to next year.
			//		Else, set to the same year.


			//Cancel if any division's length is 0 OR if there aren't exactly 3 divisions.
			if (StartDateHolder[div].Length == 0 && StartDateHolder.Length != 3)
			{
				AllGood = false;
				break;
			}
		}
		Division = 0;

		if (EndDate != null) 
		{
			CD.W($"Reading EndDate. Value: \"{EndDate}\"");
			foreach (char c in EndDate)
			{
				// "MM-DD-YY-??" <-- 4th division doesn't exist and causes OutOfIndex
				if (Division > 3) { AllGood = false; break; }

				// Moves to the next division when these chars are detected.
				// "MM" -> "DD" -> "YY"
				if (c.Equals('/') || c.Equals('-')) Division++;
				else if (char.IsNumber(c))
				{
					//If MM or DD is already 2+ chars long OR if YY is already 4+ characters long, cancel operation.
					if (EndDateHolder[Division].Length >= 2 || (EndDateHolder[Division].Length >= 4 && Division == 3))
					{
						AllGood = false;
						break;
					}

					//Else, add character to current division.
					EndDateHolder[Division].Append(c);
				}
				else if (char.IsLetter(c)) { AllGood = false; break; }
			}
			for (int div = 0; div < EndDateHolder.Length; div++) { if (EndDateHolder.Length == 0 && div != 3) { AllGood = false; break; } }
			Division = 0;
		}
		#endregion

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
}
