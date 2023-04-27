﻿namespace Project_Clovi;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Handles all file reads and writes stored locally on the host device.
/// </summary>
public class FileIODirector
{
	#region Constructor
	/// <summary>
	/// Constructor for FileIODirector.
	/// </summary>
	public FileIODirector()
	{

	}
	#endregion

	#region Attributes
	/// <summary>
	/// A lazy shortcut for lazy people... like me.
	/// </summary>
	private readonly ConsoleDirector CD = YuukaCore.ConDirector;

	/// <summary>
	/// A pre-set directory to the current OS User's Desktop folder.
	/// </summary>
	readonly String DESKTOP_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Yuuka";

	/// <summary>
	/// A pre-set directory to the current OS User's Documents folder.
	/// </summary>
	readonly String DOCUMENTS_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Yuuka";

	/// <summary>
	/// A pre-set directory to the current OS User's Application Data folder.
	/// </summary>
	readonly String APPDATA_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Yuuka";
	#endregion

	#region Methods
	#region File Management
	/// <summary>
	/// Gets the target file from the target directory.
	/// </summary>
	/// <param name="FileName">The name of the file, including the extension name. (e.g. "example.txt")</param>
	/// <param name="Index">A preset directory where local files are stored.</param>
	/// <returns>The target file.</returns>
	public StreamReader GetFile(String FileName, String Directory)
	{
		return File.OpenText(Directory + @$"\{FileName}");
	}

	/// <summary>
	/// Creates or overwrites the target file.
	/// </summary>
	/// <param name="FileName">The name of the file, including the extension name. (e.g. "example.txt")</param>
	/// <param name="Index">A preset directory where local files are stored.</param>
	/// <param name="Text">The String to be written on the file.</param>
	/// <returns>This FileIODirector object.</returns>
	public FileIODirector WriteFile(String FileName, String Directory, String Text)
	{
		File.WriteAllText(Directory + $@"\{FileName}", Text);
		return this;
	}

	/// <summary>
	/// Creates or empties the target file.
	/// </summary>
	/// <param name="FileName">The name of the file, including the extension name. (e.g. "example.txt")</param>
	/// <param name="Index">A preset directory where local files are stored.</param>
	/// <returns>This FileIODirector object.</returns>
	public FileIODirector CreateFile(String FileName, String Directory)
	{
		CD.W(Directory + $@"\{FileName}");
		System.IO.Directory.CreateDirectory(Directory);
		File.CreateText(Directory + $@"\{FileName}");
		return this;
	}

	/// <summary>
	/// Deletes the target file.
	/// </summary>
	/// <param name="FileName">The name of the file, including the extension name. (e.g. "example.txt")</param>
	/// <param name="Index">A preset directory where local files are stored.</param>
	/// <returns>This FileIODirector object.</returns>
	public FileIODirector DeleteFile(String FileName, String Directory)
	{
		File.Delete(Directory + $@"{FileName}");
		return this;
	}
	#endregion

	#region JSON Handling
	/// <summary>
	/// Gets the locally-saved instance data of any pre-existing instance of this bot on the host device.
	/// If the bot alraedy successfully booted up from the same device in the past,
	/// this function will return any data stored from the previous session.
	/// </summary>
	/// <returns>A Dictionary parsed from a JSON file containing JsonElement objects.</returns>
	public Dictionary<String, JsonElement>? GetInstanceData()
	{
		//Gets the JSON file then parses into a JSON String.
		CD.W("Attempting to retrieve instance data...");
		StreamReader InstanceDataFile = GetFile("instancedata.json", APPDATA_DIRECTORY);
		String InstanceDataString = InstanceDataFile.ReadToEnd();
		InstanceDataFile.Close();

		//Parses JSON String into a Dictionary.
		Dictionary<String, JsonElement>? ParsedJson = JsonSerializer.Deserialize<Dictionary<String, JsonElement>>(InstanceDataString);

		return ParsedJson;
	}

	/// <summary>
	/// Replaces the locally-saved instance data on the host device.
	/// </summary>
	/// <param name="NewDictionary">The replacement Dictionary object.</param>
	/// <returns>This FileIODirector object.</returns>
	public FileIODirector UpdateInstanceData(Dictionary<String, Object> NewDictionary)
	{
		//Serializes a Dictionary into a JSON String.
		String NewJsonString = JsonSerializer.Serialize<Dictionary<String, Object>>(NewDictionary);

		//Writes the JSON String into a file.
		WriteFile("instancedata.json", APPDATA_DIRECTORY, NewJsonString);

		return this;
	}

	/// <summary>
	/// Replaces one element from the locally-saved instance data on the host device.
	/// </summary>
	/// <param name="Key">The key of the target element to be updated.</param>
	/// <param name="Value">The value to be set.</param>
	/// <returns>This FileIODirector object.</returns>
	public FileIODirector UpdateInstanceData(String Key, Object Value)
	{
		Dictionary<String, JsonElement>? InstanceData = GetInstanceData();
		
		//Cancels if InstanceData is null.
		if (InstanceData is null)
		{
			CD.W("InstanceData is null at FileIODirector.UpdateInstanceData().");
			return this;
		}

		//Updates the value in the Dictionary.
		InstanceData[Key] = JsonSerializer.SerializeToElement(Value);

		//Serializes the Dictionary into a JSON String.
		String NewJsonString = JsonSerializer.Serialize<Dictionary<String, JsonElement>>(InstanceData);

		//Writes the JSON Sting into a file.
		WriteFile("instancedata.json", APPDATA_DIRECTORY, NewJsonString);

		return this;
	}

	/// <summary>
	/// Checks all required files to run the bot.
	/// In the event that certain files are corrupted or missing,
	/// this function will replace/create new files in its place.
	/// </summary>
	public bool CheckRequiredFiles()
	{
		Dictionary<String, Object>? ParsedJson;

		//The instance data file is a JSON file containing the bot's Discord API token.
		StreamReader InstanceDataFile;

		//Attempts to check for existing instance data.
		try
		{
			CD.W("Attempting to check for existing instance data...");
			InstanceDataFile = GetFile("instancedata.json", APPDATA_DIRECTORY);
		}
		catch (Exception e)
		{
			#region Replacement of instancedata file.
			//If File or Directory is missing...
			if (e is FileNotFoundException || e is DirectoryNotFoundException)
			{
				CD.W("Instance data not found. Creating new files...");

				//Creates a new empty-ish Dictionary.
				Dictionary<String, Object> NewJson = new()
				{
					{ "BotToken", "secret" },
					{ "GuildsData", new Dictionary<ulong, Object>() }
				};

				//Serializes into a JSON String.
				String NewJsonString = JsonSerializer.Serialize<Dictionary<String, Object>>(NewJson);

				//Creates the default directory if it doesn't already exist.
				System.IO.Directory.CreateDirectory(APPDATA_DIRECTORY);

				//Sets the default text for the bot token prompt.
				String BotTokenFileString = $"\n{new String('-', 70)}\nPlease paste your bot token above this line.";

				//Creates the new files needed to start the bot upon next bootup.
				WriteFile("instancedata.json", APPDATA_DIRECTORY, NewJsonString);
				WriteFile("BotToken.txt", APPDATA_DIRECTORY, BotTokenFileString);

				CD.W("[ALERT] No bot token. Paste your bot token in \"BotToken.txt\" in your Desktop.");
				return false;
			}
			#endregion

			#region Other unexpected exceptions.
			else
			{
				CD.W("[FATAL] An unexpected error has occured.");
				CD.W(e.ToString());
				return false;
			}
			#endregion
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

		//If for some reason ParsedJson is null, this cancels the program.
		if (ParsedJson is null) { CD.W("ParsedJson returned null."); return false; }

		#region Retrieval of new bot token.
		//If BotToken is "secret"... (default value)
		if (ParsedJson["BotToken"].Equals("secret"))
		{
			CD.W("Parsed BotToken returned \"secret\", reading BotToken.txt...");

			StreamReader BotTokenFile;

			//Gets the bot token from BotToken.txt, this file is deleted upon successful login.
			try { BotTokenFile = GetFile("BotToken.txt", APPDATA_DIRECTORY); }
			catch (FileNotFoundException)
			{
				CD.W("[FATAL] BotToken.txt not found. Creating replacement...");
				//Sets the default text for the bot token prompt.
				String BotTokenFileString = $"\n{new String('-', 70)}\nPlease paste your bot token above this line.";

				//Creates a replacement BotToken.txt.
				WriteFile("BotToken.txt", APPDATA_DIRECTORY, BotTokenFileString);

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
			WriteFile("instancedata.json", APPDATA_DIRECTORY, NewJsonString);
		}
		#endregion

		YuukaCore.Token = (String) ParsedJson["BotToken"];

		CD.W("Instance data retrieved.");
		return true;
	}
	#endregion
	#endregion
}
