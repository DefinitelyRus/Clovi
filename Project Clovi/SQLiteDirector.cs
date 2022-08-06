namespace Project_Clovi;

using Microsoft.Data.Sqlite;

/// <summary>
/// Handles all transactions between the bot and the database.
/// </summary>
public class SQLiteDirector : DatabaseDirector
{
	public SQLiteDirector() : base(SQLDialect: "SQLite")
	{
		DatabaseList = new();
	}

	public List<SQLiteDatabase> DatabaseList { get; internal set; }

	public override Object? GetRecord(String Key, String ColumnName, String TableName, String DatabaseName)
	{
		throw new NotImplementedException();
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
		throw new FileNotFoundException($"Database \"{DatabaseName}\" does not exist.");
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
		throw new FileNotFoundException($"Database \"{DatabaseName}\" does not exist.");
	}

	public void CheckDatabase(string DatabaseName)
	{
		try
		{
			SqliteDataReader reader = Query(DatabaseName, "SELECT * FROM guilds_settings LIMIT 1");
			if (reader.GetString(2).Equals("testname") && reader.GetString(3).Equals("testvalue"))
			{
				
			}
		}
		catch (Exception)
		{
			//TODO: Catch only the known exception.
		}
	}

}
