namespace Project_Clovi;

using Microsoft.Data.Sqlite;
using Discord.WebSocket;
using System.Data;

/// <summary>
/// Handles all transactions between the bot and the database.
/// </summary>
public class SQLiteDirector : DatabaseDirector
{
	public SQLiteDirector() : base(SQLDialect: "SQLite")
	{
		DatabaseList = new();
		CD = CloviHost.ConDirector;
	}

	public List<SQLiteDatabase> DatabaseList { get; internal set; }

	private ConsoleDirector CD { get; }

	
	public override Object GetRecord(String Key, String ColumnName, String TableName, String DatabaseName)
	{
		throw new NotImplementedException();
	}

	public override SQLiteDatabase GetDatabase(string DatabaseName)
	{
		foreach (SQLiteDatabase db in DatabaseList)
		{
			if (db.Name.Equals(DatabaseName)) return db;
		}
		throw new SqliteException($"Database \"{DatabaseName}\" cannot be found in SQLiteDirector.DatabaseList.", 0);
	}

	/// <summary>
	/// Looks for the target database then executes the SQL command.
	/// </summary>
	/// <param name="DatabaseName">The name of the database.</param>
	/// <param name="SQLCommand">The SQL command to be executed.</param>
	/// <returns>(Integer) The number of rows affected.</returns>
	/// <exception cref="FileNotFoundException">Thrown if the database does not exist.</exception>
	public override Object Execute(String DatabaseName, String SQLCommand)
	{
		foreach (SQLiteDatabase db in DatabaseList)
		{
			if (DatabaseName.Equals(db.Name)) return db.Execute(SQLCommand);
		}
		throw new SqliteException($"Database \"{DatabaseName}\" cannot be found in SQLiteDirector.DatabaseList.", 0);
	}

	/// <summary>
	/// Looks for the target database then returns the result of the query.
	/// </summary>
	/// <param name="DatabaseName">The name of the database.</param>
	/// <param name="SQLCommand">The SQL query (typically a SELECT command).</param>
	/// <returns>The results of this query.</returns>
	/// <exception cref="FileNotFoundException">Thrown if the database does not exist.</exception>
	public override SqliteDataReader Query(string DatabaseName, string SQLCommand)
	{
		foreach (SQLiteDatabase db in DatabaseList)
		{
			if (DatabaseName.Equals(db.Name)) return db.Query(SQLCommand);
		}
		throw new SqliteException($"Database \"{DatabaseName}\" cannot be found in SQLiteDirector.DatabaseList.", 0);
	}

	public void CheckDatabase(string DatabaseName)
	{
		try
		{
			SqliteDataReader reader = Query(DatabaseName, "SELECT * FROM guilds_settings LIMIT 1");
			if (reader.GetString(2).Equals("testname"))
			{
				//Inserts a dummy record for testing purposes.
				Execute("GuildsData", "INSERT INTO guilds_settings (guild_id, setting_name, setting_value) VALUES {{ \"0\", \"testname\", \"testvalue\"}}");
			}
		}
		catch (Exception e)
		{
			CD.W(e.ToString(), true);
		}
	}

	//Consults CheckDatabase() if a reset is necessary.
	public void ResetDatabase()
	{
		GetDatabase("GuildsData").Connection.Open();

		try
		{
			#region Initialization of premade values.
			//Commands to execute
			String[] SQLCommandArray =
			{
			"CREATE TABLE [IF NOT EXISTS] guilds_settings (" +
				"rowid INTEGER PRIMARY KEY AUTOINCREMENT," +
				"guild_id TEXT NOT NULL," +
				"setting_name TEXT NOT NULL," +
				"setting_value TEXT" +
			");",
			"CREATE TABLE [IF NOT EXISTS] guilds_users_data (" +
				"rowid INTEGER PRIMARY KEY AUTOINCREMENT," +
				"guild_id TEXT NOT NULL," +
				"user_id TEXT NOT NULL," +
				"data_name TEXT NOT NULL," +
				"data_value TEXT" +
			");",
		};

			//Values for setting_name and setting_value columns
			Dictionary<String, String> DefaultSettings = new()
		{
			{ "IsBotEnabled", "true" },
			{ "LoggerChannelId", "0" }
		};
			#endregion

			//For each guild the bot is in...
			foreach (SocketGuild g in CloviHost.CloviCore.Guilds.ToList())
			{
				CD.W($"Checking Guild \"{g.Name}\" ({g.Id})...");

				String DiscordGuildId, SQLQuery, SQLCommand;
				bool IsResultEmpty;
				DiscordGuildId = g.Id.ToString();

				//For each item in DefaultSettings...
				foreach (KeyValuePair<String, String> pair in DefaultSettings)
				{
					CD.W($"Searching Key: {pair.Key}...");

					#region SQL Entries
					//Get all setting_name and guild_id 
					SQLQuery = $"SELECT guild_id, setting_name, setting_value FROM guilds_settings WHERE guild_id = \"{DiscordGuildId}\" AND setting_name = \"{pair.Key}\"";
					SQLCommand = $"INSERT INTO guilds_settings (guild_id, setting_name, setting_value) VALUES (\"{g.Id.ToString()}\", \"{pair.Key}\", \"{pair.Value}\")";
					#endregion

					SqliteDataReader Result = Query("GuildsData", SQLQuery);

					IsResultEmpty = !Result.HasRows;

					if (!IsResultEmpty)
					{
						while (Result.Read())
						{
							CD.W($"Found setting \"{Result.GetString("setting_name")}\" with value \"{Result.GetString("setting_value")}\" for server \"{g.Name}\" (ID: {Result.GetString("guild_id")}).");
						}
					}
					else
					{
						CD.W("Resetting database...", true);
						Execute("GuildsData", SQLCommand);
					}
				}
			}
			CD.SendLog();
		}
		catch (Exception e)
		{
			CD.W(e.ToString(), true);
			//Drop all tables, then logout and self-exit.
			CloviHost.CloviCore.LogoutAsync();
			Environment.Exit(0);
		}
		GetDatabase("GuildsData").Connection.Close();

		//TODO: Set the first row of guilds settings as: guild_id = 0, setting_name = testname, setting_value = testvalue
		//TODO: By extension, this also means an exception block will need to be added anytime a function checks
		//		the whole table. A guild_id of 0 will always fail and may crash the program.
	}
}
