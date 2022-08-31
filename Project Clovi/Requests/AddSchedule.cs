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
		SQLiteDatabase DB = DBDir.GetDatabase("GuildsSettings");
		SqliteDataReader Reader;

		CD.W("Attempting to add schedule to the database...");
		DB.Connection.Open();
		try
		{
			Reader = DB.Query("SELECT sched_name FROM schedule_remidner WHERE sched_name = \"test_sched\"");
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
						"\"sched_end\" TEXT NOT NULL," +
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

		String SchedName = "Untitled Schedule", SchedDesc = "",
			StartDate = "01/01/01", StartTime = "00:00", ParsedStartDateTime = "BAD TIME",
			EndDate = "12/31/99", EndTime = "00:00", ParsedEndDateTime = "BAD TIME";

		foreach (SocketSlashCommandDataOption option in Command.Data.Options)
		{
			switch (option.Name)
			{
				case "name":
					SchedName = (String) option.Value;
					break;
				case "description":
					SchedDesc = (String) option.Value;
					break;
				case "start-date":
					StartDate = (String) option.Value;
					break;
				case "start-time":
					StartTime = (String) option.Value;
					break;
				case "end-date":
					EndDate = (String) option.Value;
					break;
				case "end-time":
					EndTime = (String) option.Value;
					break;
			}

		}
		bool AllGood = true;

		//Filtering date inputs
		StringBuilder[] DateHolder = new StringBuilder[3];
		byte Division = 0;
		foreach (char c in StartDate)
		{
			if (c.Equals('/')) { DateHolder[Division].Append('-'); Division++; }
			else if (Char.IsNumber(c))
			{
				if (DateHolder[Division].Length >= 2) { AllGood = false; break; }
				DateHolder[Division].Append(c);
			}
		}

		if (AllGood)
		{
			DB.Execute("INSERT INTO schedule_reminder" +
				"( sched_name, sched_desc, sched_start, sched_end, sched_creation )" +
				"VALUES (" +
					$"\"{SchedName}\"," +
					$"\"{SchedDesc}\"," +
					$"\"{ParsedStartDateTime}\"" +
					$"\"{ParsedEndDateTime}\"))");
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
