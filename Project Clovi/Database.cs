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

	private void RetrieveTable(String TableName)
	{
		//Get table from DB file.

		Dictionary<uint, Dictionary<String, Object>> Table = new();
		Dictionary<String, Object> Row = new();

		//Foreach row in DB...
			uint index = 0;

			Row.Add("Column Name", "Value");

			//Foreach column...
			Table.Add(index, Row);
	}
}
