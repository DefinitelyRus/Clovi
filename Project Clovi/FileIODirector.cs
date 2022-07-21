namespace Project_Clovi;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
public class FileIODirector
{
	public FileIODirector()
	{
		String app = @"\Project Clovi";
		Directories = new string[]
		{
			Environment.GetFolderPath(Environment.SpecialFolder.Desktop),				//Desktop
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + app,		//My Documents
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + app	//Program Files
		};
	}

	private ConsoleDirector CD = CloviHost.ConDirector;

	public String[] Directories { get; internal set; }

	//TODO: Create a string Directory overload.
	public StreamReader GetFile(String FileName, byte Directory)
	{
		return File.OpenText(Directories[Directory] + @$"\{FileName}");
	}

	public FileIODirector WriteFile(String FileName, byte Directory, String Text)
	{
		File.WriteAllText(Directories[Directory] + $@"\{FileName}", Text);
		return this;
	}

	public FileIODirector CreateFile(String FileName, byte Directory)
	{
		CD.W(Directories[Directory] + $@"\{FileName}");
		System.IO.Directory.CreateDirectory(Directories[Directory]);
		File.CreateText(Directories[Directory] + $@"\{FileName}");
		return this;
	}

	public FileIODirector DeleteFile(String FileName, byte Directory)
	{
		File.Delete(Directories[Directory] + $@"{FileName}");
		return this;
	}

	public Dictionary<String, JsonElement>? GetInstanceData()
	{

		CD.W("Attempting to retrieve instance data...");
		StreamReader InstanceDataFile = GetFile("instancedata.json", 2);
		String InstanceDataString = InstanceDataFile.ReadToEnd();
		InstanceDataFile.Close();

		Dictionary<String, JsonElement>? ParsedJson;
		ParsedJson = JsonSerializer.Deserialize<Dictionary<String, JsonElement>>(InstanceDataString);

		return ParsedJson;
	}

	public void UpdateInstanceData(Dictionary<String, Object> NewDictionary)
	{
		String NewJsonString = JsonSerializer.Serialize<Dictionary<String, Object>>(NewDictionary);

		WriteFile("instancedata.json", 2, NewJsonString);
	}

	public void UpdateInstanceData(String Key, Object Value)
	{
		Dictionary<String, JsonElement>? InstanceData = GetInstanceData();
		
		if (InstanceData is null)
		{
			CD.W("InstanceData is null at FileIODirector.UpdateInstanceData().");
			return;
		}

		InstanceData[Key] = JsonSerializer.SerializeToElement(Value);

		String NewJsonString = JsonSerializer.Serialize<Dictionary<String, JsonElement>>(InstanceData);

		WriteFile("instancedata.json", 2, NewJsonString);
	}

	/// <summary>
	/// Checks all required files to run the bot.
	/// In the event that certain files are corrupted or missing, this function will replace/create new files in its place.
	/// </summary>
	public bool CheckRequiredFiles()
	{
		Dictionary<String, Object>? ParsedJson;
		StreamReader InstanceDataFile;

		try
		{
			//Attempts to check for existing instance data.
			CD.W("Attempting to check for existing instance data...");
			InstanceDataFile = GetFile("instancedata.json", 2);
		}
		catch (Exception e)
		{
			//If File or Directory is missing...
			if (e is FileNotFoundException || e is DirectoryNotFoundException)
			{
				CD.W("Instance data not found. Creating new files...");

				//Creates a new empty-ish Dictionary.
				Dictionary<String, Object> NewJson = new();
				NewJson.Add("BotToken", "secret");
				NewJson.Add("GuildsData", new Dictionary<ulong, Object>());

				//Serializes into a JSON String.
				String NewJsonString = JsonSerializer.Serialize<Dictionary<String, Object>>(NewJson);

				//Creates the default directory if it doesn't already exist.
				Directory.CreateDirectory(Directories[2]);

				//Sets the default text for the bot token prompt.
				String BotTokenFileString = $"\n{new String('-', 70)}\nPlease paste your bot token above this line.";

				//Creates the new files needed to start the bot upon next bootup.
				WriteFile("instancedata.json", 2, NewJsonString);
				WriteFile("BotToken.txt", 0, BotTokenFileString);

				CD.W("[ALERT] No bot token. Paste your bot token in \"BotToken.txt\" in your Desktop.");
				return false;
			}
			else
			{
				CD.W("[FATAL] An unexpected error has occured.");
				CD.W(e.ToString());
				return false;
			}
		}

		//Reads the instance data JSON file.
		String InstanceDataString = InstanceDataFile.ReadToEnd();
		InstanceDataFile.Close();

		//Parses the file back into a Dictionary.
		ParsedJson = JsonSerializer.Deserialize<Dictionary<String, Object>>(InstanceDataString);

		//Cast BotToken into JsonElement, then into String.
		#pragma warning disable CS8602
		#pragma warning disable CS8601
		JsonElement BotTokenJson = (JsonElement) ParsedJson["BotToken"];
		ParsedJson["BotToken"] = BotTokenJson.GetString();
		#pragma warning restore CS8601
		#pragma warning restore CS8602

		//If for some reason, ParsedJson is null, this cancels the program.
		if (ParsedJson is null) { CD.W("ParsedJson returned null."); return false; }

		//If BotToken is "secret"... (default value)
		if (ParsedJson["BotToken"].Equals("secret"))
		{
			CD.W("Parsed BotToken returned \"secret\", reading BotToken.txt...");

			StreamReader BotTokenFile;

			//Gets the bot token from BotToken.txt, this file is deleted upon successful login.
			try { BotTokenFile = GetFile("BotToken.txt", 0); }
			catch (FileNotFoundException)
			{
				CD.W("[FATAL] BotToken.txt not found. Creating replacement...");
				//Sets the default text for the bot token prompt.
				String BotTokenFileString = $"\n{new String('-', 70)}\nPlease paste your bot token above this line.";

				//Creates a replacement BotToken.txt.
				WriteFile("BotToken.txt", 0, BotTokenFileString);

				CD.W("Replacement created. Exiting...");

				return false;
			}

			String? BotTokenString = BotTokenFile.ReadLine();
			BotTokenFile.Close();

			//If for some reason, BotTokenString is null, this cancels the program.
			if (BotTokenString is null) { CD.W("BotTokenString returned null."); return false; }
			
			//Assigns the new BotToken to the Dictionary.
			CD.W(BotTokenString);
			ParsedJson["BotToken"] = BotTokenString;

			CD.W("Attempting to rewrite instance data...");

			//Serializes back into a JSON String, then writes to instancedata.json.
			String NewJsonString = JsonSerializer.Serialize<Dictionary<String, Object>>(ParsedJson);
			WriteFile("instancedata.json", 2, NewJsonString);
		}

		CloviHost.Token = (String) ParsedJson["BotToken"];
		//CloviHost.GuildsData = (Dictionary<ulong, Object>) ParsedJson["GuildsData"];

		CD.W("Instance data retrieved.");
		return true;
	}
}
