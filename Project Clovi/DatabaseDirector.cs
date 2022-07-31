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
		//<-- SELECT ColumnName FROM TableName WHERE Id = Key
		return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="TableName"></param>
	/// <param name="GroupByRow">Defines which direction the output will be.
	/// 0 sorts by row, which uses more memory. 1 sorts by column, limiting its use.</param>
	private void GetTable(String TableName, bool GroupByRow)
	{
		//<-- SELECT * FROM TableName
		//NOTE: This is dialect-specific. How on earth do I make this standard?
		//TODO: Create an object that contains all the results, regardless of its contents. Then have it retrievable here.

		if (GroupByRow)
		{
			//foreach row in table, make a new dictionary, then add all items into dictionary then add dictionary into table (also dict)
		}
		else
		{
			//foreach column in table, make new list then add all items into list then add list into dictionary
		}
	}

	public Dictionary<String, Object> GetRow(String PrimaryKey, String TableName)
	{
		Dictionary<String, Object> Row = new();
		//<-- select * from TableName where id = PrimaryKey
		//foreach item in row, add item into dict.
		return Row;
	}

	public abstract Object? Execute(String SQLCommand);

	public abstract Object? Query(String SQLCommand);
}
