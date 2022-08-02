namespace Project_Clovi;

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
		Connection = new($@"Data Source={CloviHost.FIODirector.Directory[0]}\{Name}.db");
		Connection.Open();
		Connection.Close();
	}

	/// <summary>
	/// Constructs this SQLite database object with the file being stored in a custom location.
	/// </summary>
	/// <param name="Name">The name of this database.</param>
	/// <param name="Directory">The directory of which this database will be stored in. Do not include "\" at the end.</param>
	public SQLiteDatabase(String Name, String Directory) : base(Name)
	{
		Connection = new($@"Data Source ={Directory}\{Name}.s3db");
	}

	/// <summary>
	/// The connection to the SQLite database. Close this connection before exiting the parent program.
	/// </summary>
	public SqliteConnection Connection { get; private set; }

	/// <summary>
	/// Sends the command to the database and applies any changes.
	/// </summary>
	/// <param name="SQLCommand">The SQL command to be sent to the database.</param>
	/// <returns>(Integer) The number of rows affected by the command.</returns>
	public override Object Execute(String SQLCommand)
	{
		Connection.Open();
		int Result = new SqliteCommand(SQLCommand, Connection).ExecuteNonQuery();
		Connection.Close();
		return Result;
	}

	/// <summary>
	/// Returns the results of an SQL query. Needs to be closed after use.
	/// </summary>
	/// <param name="SQLCommand">The SQL command to be sent to the database.</param>
	/// <returns>Results of the query.</returns>
	public override SqliteDataReader Query(String SQLCommand)
	{
		Connection.Open();
		SqliteDataReader Reader = new SqliteCommand(SQLCommand, Connection).ExecuteReader();
		Connection.Close();
		return Reader;
	}
}
