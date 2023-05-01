namespace Project_Clovi;

public abstract class Database
{
	public Database(String Name)
	{
		this.Name = Name;
	}

	/// <summary>
	/// The name of this database. Used to identify between databases for logging and debugging purposes.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Sends the command to the database and applies any changes.
	/// </summary>
	/// <param name="SQLCommand">The SQL command to be sent to the database.</param>
	/// <returns>(Integer) The number of rows affected by the command.</returns>
	public abstract Object Execute(String SQLCommand);

	/// <summary>
	/// Returns the results of an SQL query. Needs to be closed after use.
	/// </summary>
	/// <param name="SQLCommand">The SQL command to be sent to the database.</param>
	/// <returns>Results of the query.</returns>
	public abstract Object Query(String SQLCommand);
}
