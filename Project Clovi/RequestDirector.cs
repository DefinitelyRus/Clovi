namespace Project_Clovi;

using System.Collections;
using Discord.WebSocket;

/// <summary>
/// Directs all Request reads and changes.
/// </summary>
public class RequestDirector
{
	#region Constructor
	/// <summary>
	/// The function used to declare this RequestDirector.
	/// </summary>
	/// <param name="RequestListArg">An ArrayList of Requests.</param>
	public RequestDirector(List<Request> RequestListArg)
	{
		RequestList = RequestListArg; //Sort alphabetically.
	}
	#endregion

	#region Attributes
	/// <summary>
	/// Shortcut to CloviHost.ConDirector;
	/// </summary>
	private static readonly ConsoleDirector CD = YuukaCore.ConDirector;

	/// <summary>
	/// A list of available requests.
	/// Sorted alphabetically based on Request Ids.
	/// </summary>
	public List<Request> RequestList { get; internal set; }
	#endregion

	#region Methods
	/// <summary>
	/// Searches for and returns the first Request Object that matches the Request Id argument.
	/// </summary>
	/// <param name="RequestId">The Id of the target Request.</param>
	/// <returns>The Request Object with a matching Id.</returns>
	public Request? GetRequest(String RequestId)
	{
		foreach (Request Req in RequestList)
			if (Req.Name.Equals(RequestId)) return Req;

		CD.W($"No request \"{RequestId}\" found.");
		//TODO: Add no-match-found error message.
		return null;
	}

	/// <summary>
	/// Adds the inputted Request Object to the RequestList.
	/// Also auto-sorts the list alphabetically.
	/// </summary>
	/// <param name="NewRequest">The new Request to be added.</param>
	/// <returns></returns>
	public RequestDirector AddRequestItem(Request NewRequest)
	{
		NewRequest.Director = this;
		RequestList.Add(NewRequest);

		//Sort alphabetically based on Request.Id attribute.
		return this;
	}

	/// <summary>
	/// Sorts the list alphabnumerically based on its IDs.
	/// </summary>
	/// <param name="ListToSort">The ArrayList to be sorted.</param>
	/// <returns>The sorted ArrayList.</returns>
	static internal List<Request> Sort(ArrayList ListToSort)
	{
		List<Request> OrderedList = ListToSort.Cast<Request>().ToList();

		//TODO: Implement quicksort here

		return OrderedList;
	}

	/// <summary>
	/// Executes the request based on the SocketSlashCommand's attributes. For use in-Discord /commands only.
	/// </summary>
	/// <param name="Command"></param>
	/// <returns></returns>
	public void ExecuteRequest(SocketSlashCommand Command, DiscordSocketClient Core)
	{
		Request? RequestItem = GetRequest(Command.CommandName);

		//Cancels execution if Request is null.
		if (RequestItem is null) return;

		//Finally executes the request.
		RequestItem.Execute(Command, Core);
	}
	#endregion
}
