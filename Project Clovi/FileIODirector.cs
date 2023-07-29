namespace MisakaFramework;

using System;
using System.IO;
using System.Text.Json;

/// <summary>
/// Handles all file reads and writes stored locally on the host device.
/// </summary>
public class FileIODirector
{
	#region Constructor
	/// <summary>
	/// Constructor for FileIODirector. It basically does nothing.
	/// </summary>
	public FileIODirector()
	{

	}
	#endregion

	#region Attributes
	/// <summary>
	/// A lazy shortcut for lazy people... like me.
	/// </summary>
	private readonly ConsoleDirector CD = MisakaCore.ConDirector;

	/// <summary>
	/// A pre-set directory to the current OS User's Desktop folder.
	/// </summary>
	public readonly String DIRECTORY_DESKTOP = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

	/// <summary>
	/// A pre-set directory to the current OS User's Documents folder.
	/// </summary>
	public readonly String DIRECTORY_DOCUMENTS = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"{Path.DirectorySeparatorChar}Yuuka";

	/// <summary>
	/// A pre-set directory to the current OS User's Application Data folder.
	/// </summary>
	public readonly String DIRECTORY_APPDATA = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"{Path.DirectorySeparatorChar}Yuuka";
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
		return File.OpenText(Directory + @$"{Path.DirectorySeparatorChar}{FileName}");
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
		File.WriteAllText(Directory + $@"{Path.DirectorySeparatorChar}{FileName}", Text);
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
		CD.W(Directory + $@"{Path.DirectorySeparatorChar}{FileName}");
		System.IO.Directory.CreateDirectory(Directory);
		File.CreateText(Directory + $@"{Path.DirectorySeparatorChar}{FileName}");
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
		File.Delete(Directory + $@"{Path.DirectorySeparatorChar}{FileName}");
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
		StreamReader InstanceDataFile = GetFile("instancedata.json", DIRECTORY_APPDATA);
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
		WriteFile("instancedata.json", DIRECTORY_APPDATA, NewJsonString);

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
		WriteFile("instancedata.json", DIRECTORY_APPDATA, NewJsonString);

		return this;
	}

	/// <summary>
	/// Checks all required files to run the bot.
	/// In the event that certain files are corrupted or missing,
	/// this function will replace/create new files in its place.
	/// </summary>
	public bool CheckRequiredFiles()
	{
		#pragma warning disable CS8604

		//The instance data file is a JSON file containing the bot's Discord API token.
		StreamReader TokenContainerFile;
		bool IsTokenOverridden;
		String OverrideTokenFileName = "_YuukaBotToken.txt";
		String TokenFileName = "PersistentTokenContainer.dat";
		String? Token = "secret";

		CD.W($"Using the following directories for this host:\nDesktop: \"{DIRECTORY_DESKTOP}\"\nDocuments: \"{DIRECTORY_DOCUMENTS}\"\nAppData: \"{DIRECTORY_APPDATA}\"");

		//Check if _YuukaBotToken.txt exists.
		try
		{
			CD.W($"Attempting to check for override token file \"{OverrideTokenFileName}\"...");
			TokenContainerFile = GetFile(OverrideTokenFileName, DIRECTORY_DESKTOP);
			Token = TokenContainerFile.ReadLine();

			//Throw a FileNotFoundException if the TokenContainerFile remains unmodified.
			if (TokenContainerFile.ReadToEnd().Equals($"\n{new String('-', 70)}\nPlease paste your bot token above this line."))
				throw new FileNotFoundException("");

			IsTokenOverridden = true;
		}

		//If _YuukaBotToken.txt does not exist...
		catch (Exception)
		{
			CD.W($"\"{OverrideTokenFileName}\" could not be found. Proceeding...");
			IsTokenOverridden = false;

			//Check if PersistentTokenContainer.dat exists.
			try
			{
				CD.W($"Attempting to find \"{TokenFileName}\"...");
				TokenContainerFile = GetFile(TokenFileName, DIRECTORY_APPDATA);
			}
			catch (Exception e)
			{	
				//If PersistentTokenContainer.dat does not exist...
				if (e is FileNotFoundException || e is DirectoryNotFoundException)
				{
					CD.W($"Could not find \"{TokenFileName}\". Creating \"{OverrideTokenFileName}\"...");
					System.IO.Directory.CreateDirectory(DIRECTORY_APPDATA); //TODO: Check if this is still necessary.

					//Sets the default text for the bot token prompt.
					String BotTokenFileString = $"\n{new String('-', 70)}\nPlease paste your bot token above this line.";
					WriteFile(OverrideTokenFileName, DIRECTORY_DESKTOP, BotTokenFileString);

					CD.W("[ALERT] No tokens detected! Launch is cancelled.");
					CD.W($"[ALERT] Paste your bot token in \"{OverrideTokenFileName}\" in your Desktop.");
					return false;
				}

				//If something else caused the issue...
				else
				{
					CD.W($"[FATAL] An unexpected error has occurred! Launch is cancelled.\n{e}");
					return false;
				}
			}
		}

		//TODO: Add a 2-way encryption to this, so the token won't be stored as plain text.
		if (IsTokenOverridden)
		{
			System.IO.Directory.CreateDirectory(DIRECTORY_APPDATA);
			WriteFile(TokenFileName, DIRECTORY_APPDATA, Token);

			TokenContainerFile.Close();
			TokenContainerFile = GetFile(TokenFileName, DIRECTORY_APPDATA);
			DeleteFile(OverrideTokenFileName, DIRECTORY_DESKTOP);
			CD.W("[NOTICE] Override token detected. Replacing existing token...");
		}

		Token = TokenContainerFile.ReadLine();

		if (Token != null)
		{
			MisakaCore.Token = Token;
			TokenContainerFile.Close();
			CD.W("Bot token retrieved. Continuing launch...");
			return true;
		}
		else
		{
			CD.W("Bot token is null. Launch is cancelled.");
			return false;
		}


		#pragma warning restore CS8604
	}
	#endregion
	#endregion
}
