namespace MisakaFramework;

// Refactor to only use SQLite.
public abstract class _DatabaseDirector
{
	public _DatabaseDirector() { }

	public String Dialect { get; private set; }

	public abstract Object GetRecord(String Key, String ColumnName, String TableName, String DatabaseName);

	public abstract Object GetDatabase(String DatabaseName);

	public abstract Object Execute(String DatabaseName, String SQLCommand);

	public abstract Object Query(String DatabaseName, String SQLCommand);
}
