namespace Project_Clovi;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
public class FileIODirector
{
	FileIODirector()
	{
		String app = @"\Project Clovi";
		Directories = new string[]
		{
			Environment.GetFolderPath(Environment.SpecialFolder.Desktop),				//Desktop
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + app,		//My Documents
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + app		//Program Files
		};
	}

	public String[] Directories { get; internal set; }

	//TODO: Create a string Directory overload.
	public StreamReader GetFile(String FileName, byte Directory)
	{
		return File.OpenText(Directories[Directory] + @$"\{FileName}");
	}

	/// <summary>
	/// Checks all required files to run the bot.
	/// In the event that certain files are corrupted or missing, this function will replace/create new files in its place.
	/// </summary>
	public void CheckRequiredFiles()
	{
		ConsoleDirector CD = CloviHost.ConDirector;

		//TODO: Do a file checker here.
		try
		{
			StreamReader OpenFile = GetFile("instancedata.json", 2);
			Dictionary<String, Object>? ParsedJson =
				JsonSerializer.Deserialize<Dictionary<String, Object>>(OpenFile.ReadToEnd());

			if (ParsedJson is null) throw new NullReferenceException("\"instancedata.json\" returned null and cannot be parsed back to Dictionary<String, Object?>.");

			CloviHost.Token = (String)ParsedJson["BotToken"];
			CloviHost.GuildsData = (Dictionary<ulong, Object>)ParsedJson["GuildsData"];

		}
		catch (FileNotFoundException e)
		{
			CD.W($"{e.Message}\n\nStack Trace:\n{e.StackTrace}");
			CD.W($"Creating new \"{e.FileName}\"...");

			File.CreateText(Directories[2] + @$"\{e.FileName}");

			Dictionary<String, Object> newJson = new()
			{
				{"BotToken", "secret"},
				{"GuildsData", new Dictionary<ulong, Object>()} //Move to database. This isn't good for large-scale deployment.
			};
		}
		catch (Exception e)
		{
			CD.W($"{e.Message}\n\nStack Trace:\n{e.StackTrace}");


		}
	}
}
