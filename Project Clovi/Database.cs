namespace Project_Clovi;

public class Database
{
	public Database()
	{

	}

	public String Id { get; set; }

	/// <summary>
	/// An in-memory copy of any active table.
	/// </summary>
	internal Dictionary<String, Dictionary<uint, Object>> Tables { get; set; }
	/*
	 * Dictionary
	 * -> Table (#)
	 * ---> Row (=)
	 * -----> Column (Single Item; [])
	 */
}
