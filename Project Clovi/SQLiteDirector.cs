namespace Project_Clovi;

using Microsoft.Data.Sqlite;

/// <summary>
/// Handles all transactions between the bot and the database.
/// </summary>
public class SQLiteDirector : DatabaseDirector
{
	SQLiteDirector(String Name) : base(SQLDialect: "SQLite")
	{
		Connection = new($@"Data Source={CloviHost.FIODirector.Directory[0]}\GuildsData.s3db");
		DatabaseName = Name;
		DatabaseList = new();
	}

	/*
	 * !!!!!
	 * It seems I mistakenly combined the director and the database object itself.
	 * The SqliteConnection object is to be stored in individual SQLiteDatabase objects.
	 * The execute/query functions are to be stored there too,
	 * but allow them to be called from this director.
	 * !!!!!
	 */
	public List<SQLiteDatabase> DatabaseList { get; internal set; }

	/// <summary>
	/// The connection to the SQLite database. Close this connection before exiting the parent program.
	/// </summary>
	public SqliteConnection Connection { get; private set; }

	/// <summary>
	/// The name of this database. Used to identify between databases for logging and debugging purposes.
	/// </summary>
	public string DatabaseName { get; private set; }
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
	public override SqliteDataReader? Query(string DatabaseName, string SQLCommand)
	{
		foreach (SQLiteDatabase db in DatabaseList)
		{
			if (DatabaseName.Equals(db.Name)) return db.Query(SQLCommand);
		}
		throw new FileNotFoundException($"Database \"{DatabaseName}\" does not exist.");
	}
}
