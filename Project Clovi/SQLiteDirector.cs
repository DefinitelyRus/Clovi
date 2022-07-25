namespace Project_Clovi;

using Microsoft.Data.Sqlite;

public class SQLiteDirector : DatabaseDirector
{
	SQLiteDirector() : base(SQLDialect: "SQLite")
	{

	}
}
