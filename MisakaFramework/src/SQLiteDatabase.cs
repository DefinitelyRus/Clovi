namespace MisakaFramework;

using Microsoft.Data.Sqlite;

/// <summary>
/// An SQLite database object housing shortcuts to the database.
/// </summary>
public class SQLiteDatabase : Database
{
	/// <summary>
	/// Constructs this SQLite database object.
	/// </summary>
	/// <param name="Name">The name of this database.</param>
	public SQLiteDatabase(String Name) : base(Name)
	{
		Connection = new($@"Data Source={MisakaCore.FIOManager.DIRECTORY_APPDATA}\{Name}.db");
		Connection.Open(); Connection.Close();

		CD = MisakaCore.ConManager;
	}

	/// <summary>
	/// Constructs this SQLite database object with the file being stored in a custom location.
	/// </summary>
	/// <param name="Name">The name of this database.</param>
	/// <param name="Directory">The directory of which this database will be stored in. Do not include "\" at the end.</param>
	public SQLiteDatabase(String Name, String Directory) : base(Name)
	{
		Connection = new($@"Data Source ={Directory}\{Name}.s3db");
		Connection.Open(); Connection.Close();

		CD = MisakaCore.ConManager;
	}

	/// <summary>
	/// The connection to the SQLite database. Close this connection before exiting the parent program.
	/// </summary>
	public SqliteConnection Connection { get; private set; }

	private ConsoleManager CD { get; }

	/// <summary>
	/// Sends the command to the database and applies any changes. Connection must be opened before using this method.
	/// </summary>
	/// <param name="SQLCommand">The SQL command to be sent to the database.</param>
	/// <returns>(Integer) The number of rows affected by the command.</returns>
	public override Object Execute(String SQLCommand)
	{
		int Result = new SqliteCommand(SQLCommand, Connection).ExecuteNonQuery();
		return Result;
	}

	/// <summary>
	/// Sends the command to the database and applies any changes. Connection is opened automatically and optionally closed manually. 
	/// </summary>
	/// <param name="SQLCommand"></param>
	/// <param name="AutoCloseConnection"></param>
	/// <returns></returns>
	public Object Execute(String SQLCommand, bool AutoCloseConnection)
	{
		Connection.Open();
		int Result = new SqliteCommand(SQLCommand, Connection).ExecuteNonQuery();
		if (AutoCloseConnection) Connection.Close();
		return Result;
	}

	/// <summary>
	/// Returns the results of an SQL query. Connection must be opened before using this method.
	/// Use SqliteDataReader.Read() in a WHILE loop to iterate through results!
	/// </summary>
	/// <param name="SQLCommand">The SQL command to be sent to the database.</param>
	/// <returns>Results of the query.</returns>
	public override SqliteDataReader Query(String SQLCommand)
	{
		SqliteDataReader Reader = new SqliteCommand(SQLCommand, Connection).ExecuteReader();
		return Reader;
	}

	/// <summary>
	/// Returns the results of an SQL query. Connection is opened automatically and optionally closed manually.
	/// Use SqliteDataReader.Read() in a WHILE loop to iterate through results!
	/// </summary>
	/// <param name="SQLCommand"></param>
	/// <param name="AutoCloseConnection"></param>
	/// <returns></returns>
	public SqliteDataReader Query(String SQLCommand, bool AutoCloseConnection)
	{
		Connection.Open();
		SqliteDataReader Reader = new SqliteCommand(SQLCommand, Connection).ExecuteReader();
		if (AutoCloseConnection) Connection.Close();
		return Reader;
	}
}
