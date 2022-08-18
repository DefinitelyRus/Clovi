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

	#region Attributes
	public List<SQLiteDatabase> DatabaseList { get; internal set; }

	private ConsoleDirector CD { get; }
	#endregion

	#region Methods
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

	//Consults CheckDatabase() if a reset is necessary.
	public bool DatabaseCheckup(byte ResetType = 0, byte AttemptCount = 0)
	{
		String[] SQLEntries;

		switch (ResetType)
		{
			#region Case 0: Checkup of default tables.
			case 0:
				GetDatabase("GuildsData").Connection.Open();
				#region SQL Entries
				SQLEntries = new String[2];

				//Gets all the default checker values.
				SQLEntries[0] = $"SELECT guild_id, setting_name, setting_value FROM guilds_settings WHERE guild_id = \"0\"";

				//Inserts all the default checker values in case they don't already exist.
				SQLEntries[1] = "INSERT INTO guilds_settings (guild_id, setting_name, setting_value) VALUES (\"0\", \"test_name\", \"test_value\")";
				#endregion

				try
				{
					SqliteDataReader Result = Query("GuildsData", SQLEntries[0]);

					if (Result.HasRows)
					{
						while (Result.Read())
						{
							CD.W($"Found setting \"{Result.GetString("setting_name")}\" with value \"{Result.GetString("setting_value")}\" for server ID: {Result.GetString("guild_id")}.");
						}
					}
					else
					{
						CD.W($"No results for \"test_name\" for server ID: \"0\". Creating entry...");
						Execute("GuildsData", SQLEntries[1]);
					}

					GetDatabase("GuildsData").Connection.Close();
					return true;
				}
				catch (SqliteException e)
				{
					CD.W($"{e.Message} Creating new tables...");

					if (AttemptCount == 0)
					{
						String[] NewTableCommand =
						{
						"CREATE TABLE guilds_settings (" +
							"onlykey INTEGER NOT NULL," +
							"guild_id INTEGER NOT NULL," +
							"setting_name TEXT NOT NULL," +
							"setting_value TEXT," +
							"PRIMARY KEY(onlykey AUTOINCREMENT)" +
						")"
						};

						Execute("GuildsData", NewTableCommand[0]);

						CD.W("Running checkup again...");
						return DatabaseCheckup(0, 1);
					}
					else
					{
						GetDatabase("GuildsData").Connection.Close();
						return false;
					}
				}
			#endregion

			#region Case 1: Checkup of default values.
			case 1:
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

							String DiscordGuildId;
							DiscordGuildId = g.Id.ToString();

							//For each item in DefaultSettings...
							foreach (KeyValuePair<String, String> pair in DefaultSettings)
							{
								CD.W($"Searching Key: {pair.Key}...");

								#region SQL Entries
								SQLEntries = new String[2];

								//Get all setting_name and guild_id 
								SQLEntries[0] = $"SELECT guild_id, setting_name, setting_value FROM guilds_settings WHERE guild_id = \"{DiscordGuildId}\" AND setting_name = \"{pair.Key}\"";
								
								//Set all setting_name, value, and guild_id
								SQLEntries[1] = $"INSERT INTO guilds_settings (guild_id, setting_name, setting_value) VALUES (\"{g.Id}\", \"{pair.Key}\", \"{pair.Value}\")";
								#endregion

								GetDatabase("GuildsData").Connection.Open();
								SqliteDataReader Result = Query("GuildsData", SQLEntries[0]);

								if (Result.HasRows)
								{
									while (Result.Read())
									{
										CD.W($"Found setting \"{Result.GetString("setting_name")}\" with value \"{Result.GetString("setting_value")}\" for server \"{g.Name}\".");
									}
								}
								else
								{
									CD.W($"No results found for \"{pair.Key}\" for server \"{g.Name}\". Creating entry...");
									Execute("GuildsData", SQLEntries[1]);
								}
							}
						}
						GetDatabase("GuildsData").Connection.Close();
						return true;
					}
					catch (Exception e)
					{
						CD.W(e.ToString());
						//Drop all tables, then logout and self-exit.
						GetDatabase("GuildsData").Connection.Close();
						CloviHost.CloviCore.LogoutAsync();
						Environment.Exit(0);
						return false;
					}

					//TODO: Set the first row of guilds settings as: guild_id = 0, setting_name = testname, setting_value = testvalue
					//TODO: By extension, this also means an exception block will need to be added anytime a function checks
					//		the whole table. A guild_id of 0 will always fail and may crash the program.
				}
			#endregion

			default:
				return false;
		}
	}
	#endregion
}
