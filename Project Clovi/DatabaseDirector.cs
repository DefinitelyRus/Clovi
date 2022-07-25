namespace Project_Clovi;
public abstract class DatabaseDirector
{
	public DatabaseDirector(string SQLDialect)
	{
		Dialect = SQLDialect;
		DatabaseList = new();
	}

	List<Database> DatabaseList { get; set; }

	public DatabaseDirector AddDatabase(Database database)
	{
		DatabaseList.Add(database);
		return this;
	}

	public DatabaseDirector RemoveDatabasae(String Id)
	{
		foreach (Database db in DatabaseList)
			if (db.Id == Id) DatabaseList.Remove(db);

		return this;
	}

	public String Dialect { get; private set; }
}
