namespace Project_Clovi;
public abstract class DatabaseDirector
{
	public DatabaseDirector(string SQLDialect)
	{
		Dialect = SQLDialect;
	}

	public String Dialect { get; private set; }

	private LinkedList<String> SQLQueue = new();

	public Object? GetRecord(String Key, String ColumnName, String TableName)
	{
	}
	public abstract Object? GetRecord(String Key, String ColumnName, String TableName, String DatabaseName);

	public Dictionary<String, Object> GetRow(String PrimaryKey, String TableName)
	{
		Dictionary<String, Object> Row = new();
		//<-- select * from TableName where id = PrimaryKey
		//foreach item in row, add item into dict.
		return Row;
	}

	public abstract Object? Execute(String DatabaseName, String SQLCommand);

	public abstract Object? Query(String DatabaseName, String SQLCommand);
}
