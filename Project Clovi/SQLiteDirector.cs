namespace Project_Clovi;

using Microsoft.Data.Sqlite;

public class SQLiteDirector : DatabaseDirector
{
	SQLiteDirector(String Name) : base(SQLDialect: "SQLite")
	{
		Connection = new($@"Data Source={CloviHost.FIODirector.Directory[0]}\GuildsData.s3db");
		DatabaseName = Name;
	}

	public SqliteConnection Connection { get; private set; }

	public string DatabaseName { get; private set; }

	public override Object Execute(String SQLCommand)
	{
		return new SqliteCommand(SQLCommand, Connection).ExecuteNonQuery();
	}

	public override SqliteDataReader Query(String SQLCommand)
	{
		return new SqliteCommand(SQLCommand, Connection).ExecuteReader();
	}
}
