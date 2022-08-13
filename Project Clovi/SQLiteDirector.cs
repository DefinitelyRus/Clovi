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

		//TODO: Check CloviCore for any joined servers, then add each server to guilds_settings with default values.
		List<SocketGuild> Guilds = CloviHost.CloviCore.Guilds.ToList();
		Dictionary<String, String> DefaultSettings = new()
		{
			{ "IsBotEnabled", "true" },
			{ "LoggerChannelId", "0" }
		};

		//Opens the GuildsData database.
		GetDatabase("GuildsData").Connection.Open();

		//For each guild the bot is in...
		foreach (SocketGuild g in Guilds) {
			CD.W($"Guild: {g.Name} ({g.Id})");
			String DiscordGuildId, SQLQuery, SQLCommand;
			bool IsMissing;
			DiscordGuildId = g.Id.ToString();
			
			//For each item in DefaultSettings...
			foreach (KeyValuePair<String, String> pair in DefaultSettings)
			{
				CD.W($"Searching Key: {pair.Key}...");
				SQLQuery = $"SELECT * FROM guilds_settings WHERE guild_id = \"{DiscordGuildId}\" AND setting_name = \"{pair.Key}\"";
				SQLCommand = $"INSERT INTO guilds_settings (guild_id, setting_name, setting_value) VALUES (\"{g.Id.ToString()}\", \"{pair.Key}\", \"{pair.Value}\")";

				try
				{
					SqliteDataReader Result = Query("GuildsData", SQLQuery);

					IsMissing = !Result.HasRows;
					CD.W($"Is {pair.Key} missing: {IsMissing}");

					if (!IsMissing)
					{
						while (Result.Read())
						{
							CD.W($"{Result.GetString("setting_name")} -> {Result.GetString("guild_id")}");
						}
						CD.SendLog();
					}
					else
					{
						CD.W("Resetting database...", true);
						Execute("GuildsData", SQLCommand);
					}
				}
				catch (Exception e)
				{
					Execute("GuildsData", SQLCommand);
					CD.W(e.ToString(), true);
				}
			}
		}
		GetDatabase("GuildsData").Connection.Close();

		//TODO: Set the first row of guilds settings as: guild_id = 0, setting_name = testname, setting_value = testvalue
		//TODO: By extension, this also means an exception block will need to be added anytime a function checks
		//		the whole table. A guild_id of 0 will always fail and may crash the program.
	}
}
