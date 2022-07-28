namespace Project_Clovi;

using Microsoft.Data.Sqlite;

public class SQLiteDirector : DatabaseDirector
{
	SQLiteDirector() : base(SQLDialect: "SQLite")
	{
		//write stuff here dammit
		//put the implementations here. make dbdirector fully abstract or something
	}
}
