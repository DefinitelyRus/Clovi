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

	private LinkedList<String> SQLQueue = new();

	public Database SelectedDatabase { get; private set; }

	public Object? GetByPrimaryKey(String TableName, String ColumnName, String Key)
	{

		return null;
	}

}
