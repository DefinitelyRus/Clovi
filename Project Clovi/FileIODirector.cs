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
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + app		//Program Files
		};
	}

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
		CloviHost.ConDirector.W(Directories[Directory] + $@"\{FileName}");
		System.IO.Directory.CreateDirectory(Directories[Directory]);
		File.CreateText(Directories[Directory] + $@"\{FileName}");
		return this;
	}

	public FileIODirector DeleteFile(String FileName, byte Directory)
	{
		File.Delete(Directories[Directory] + $@"{FileName}");
		return this;
	}

	/// <summary>
	/// Checks all required files to run the bot.
	/// In the event that certain files are corrupted or missing, this function will replace/create new files in its place.
	/// </summary>
	public bool CheckRequiredFiles()
	{
		ConsoleDirector CD = CloviHost.ConDirector;

		try
		{
			//Opens the file.
			StreamReader OpenFile = GetFile("instancedata.json", 2);

			//Parses the file into JSON String and assigns it to a Dictionary.
			Dictionary<String, Object>? ParsedJson = null;

			String Token = "secret";
			try
			{
				ParsedJson = JsonSerializer.Deserialize<Dictionary<String, Object>>(OpenFile.ReadToEnd());

				//Assign the BotToken from the ParsedJson to a String.
				Token = (String)ParsedJson["BotToken"];
			}
			catch (JsonException e) { CD.W(e.ToString()); ParsedJson = null;  }

			//Closes the file.
			OpenFile.Close();

			//If ParsedJson returns null, throw an exception. (Cancels this whole TRY block.)
			//if (ParsedJson is null) throw new NullReferenceException("\"instancedata.json\" returned null and cannot be parsed back to Dictionary<String, Object?>.");

			//If the Token is "secret"... (Default value; invalid.)
			if (Token.Equals("secret"))
			{
				CD.W(Token);
				CD.W(CloviHost.Token);
				//Find a file called "BotToken.txt" on the user's Desktop, then assigns the first line to the Token String.
				StreamReader TokenFile = GetFile("BotToken.txt", 0);
				Token = TokenFile.ReadToEnd();
				TokenFile.Close();

				DeleteFile("BotToken.txt", 0);
				ParsedJson["BotToken"] = Token;
			}

			CloviHost.Token = Token;
			CloviHost.GuildsData = (Dictionary<ulong, Object>) ParsedJson["GuildsData"];

			String NewJson = JsonSerializer.Serialize<Dictionary<String, Object>>(ParsedJson);

			WriteFile("instancedata.json", 2, NewJson);
			return true;
		}
		catch (Exception e)
		{
			CD.W(e.Message);
			CD.W(e.StackTrace);

			if (e is FileNotFoundException || e is DirectoryNotFoundException || e is NullReferenceException)
			{
				CD.W($"{e.Message}\n\nStack Trace:\n{e.StackTrace}");

				#region Replacing the missing/corrupt JSON file.
				CD.W("Creating new \"instancedata.json\"...");
				CreateFile("instancedata.json", 2);

				Dictionary<String, Object> newJson = new()
				{
					{"BotToken", "secret"},
					{"GuildsData", new Dictionary<ulong, Object>()} //Move to database. This isn't good for large-scale deployment.
				};
				#endregion

				CD.W("Creating new \"BotToken.txt\" in the current user's Desktop.");
				CD.W("[!!!] ALERT: Please paste your bot token in BotToken.txt in your Desktop, then restart this program.");
				CreateFile("BotToken.txt", 0);
			}

			return false;
		}
	}
}
