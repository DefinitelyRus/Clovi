namespace Project_Clovi;
using Discord.WebSocket;

public abstract class IODirector
{
	public IODirector(DiscordSocketClient CoreArg, RequestDirector ReqDirectorArg, String IdArg = "ConsoleApp")
	{
		Core = CoreArg;
		ReqDirector = ReqDirectorArg;
		Id = IdArg;
	}

	internal DiscordSocketClient Core { get; set; }

	internal RequestDirector ReqDirector { get; set; }

	public String Id { get; set; }

	public abstract void Print(String Text);

	public abstract void Input(String Text);
}
