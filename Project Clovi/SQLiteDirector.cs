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
	}

	/*
	 * !!!!!
	 * It seems I mistakenly combined the director and the database object itself.
	 * The SqliteConnection object is to be stored in individual SQLiteDatabase objects.
	 * The execute/query functions are to be stored there too,
	 * but allow them to be called from this director.
	 * !!!!!
	 */

	/// <summary>
	/// The connection to the SQLite database. Close this connection before exiting the parent program.
	/// </summary>
	public SqliteConnection Connection { get; private set; }

	/// <summary>
	/// The name of this database. Used to identify between databases for logging and debugging purposes.
	/// </summary>
	public string DatabaseName { get; private set; }

	/// <summary>
	/// Sends the command to the database and applies any changes.
	/// </summary>
	/// <param name="SQLCommand">The SQL command to be sent to the database.</param>
	/// <returns>(Integer) The number of rows affected by the command.</returns>
	public override Object Execute(String SQLCommand)
	{
		return new SqliteCommand(SQLCommand, Connection).ExecuteNonQuery();
	}

	/// <summary>
	/// Returns the results of an SQL query. Needs to be closed after use.
	/// </summary>
	/// <param name="SQLCommand">The SQL command to be sent to the database.</param>
	/// <returns>Results of the query.</returns>
	public override SqliteDataReader Query(String SQLCommand)
	{
		return new SqliteCommand(SQLCommand, Connection).ExecuteReader();
	}
}
