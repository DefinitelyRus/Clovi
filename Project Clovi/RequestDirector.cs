using System.Collections;
using Discord.WebSocket;
namespace Project_Clovi
{
	/// <summary>
	/// Directs all Request reads and changes.
	/// </summary>
	public class RequestDirector
	{
		/// <summary>
		/// A list of available requests.
		/// Sorted alphabetically based on Request Ids.
		/// </summary>
		public ArrayList RequestList { get; internal set; }



		/// <summary>
		/// Searches for and returns the first Request Object that matches the Request Id argument.
		/// </summary>
		/// <param name="RequestId">The Id of the target Request.</param>
		/// <returns>The Request Object with a matching Id.</returns>
		public Request GetRequest(String RequestId)
		{
			foreach (Request Req in RequestList) if (Req.Id.Equals(RequestId)) return Req;


			//TODO: Add no-match-found error message.
			return null;
		}



		/// <summary>
		/// The function used to declare this RequestDirector.
		/// </summary>
		/// <param name="RequestListArg">An ArrayList of Requests.</param>
		public RequestDirector(ArrayList RequestListArg)
		{
			RequestList = RequestListArg; //Sort alphabetically.
		}



		/// <summary>
		/// Adds the inputted Request Object to the RequestList.
		/// Also auto-sorts the list alphabetically.
		/// </summary>
		/// <param name="NewRequest">The new Request to be added.</param>
		/// <returns></returns>
		public RequestDirector AddRequestItem(Request NewRequest)
		{
			RequestList.Add(NewRequest);
			//Sort alphabetically.
			return this;
		}



		/// <summary>
		/// Sorts the list alphabnumerically based on its IDs.
		/// </summary>
		/// <param name="ListToSort">The ArrayList to be sorted.</param>
		/// <returns>The sorted ArrayList.</returns>
		internal List<Request> Sort(ArrayList ListToSort)
		{
			List<Request> OrderedList = ListToSort.Cast<Request>().ToList();

			//TODO: Implement quicksort here

			return OrderedList;
		}



		/// <summary>
		/// Executes the request based on the given RequestId and Args. For use in console or in-text commands. Only applicable for context insensitive commands.
		/// </summary>
		/// <param name="RequestId">The Id of the target Request.</param>
		/// <param name="Args">The arguments required to execute the Request.</param>
		/// <returns>This RequestDirector for method chaining.</returns>
		public RequestDirector ExecuteRequest(String RequestId, String[] Args, SocketSlashCommand? Cmd = null)
		{
			Request RequestItem = GetRequest(RequestId);

			if (RequestItem == null)
			{
				Console.WriteLine($"No Request \"{RequestId}\" found."); //TODO: Move to a dedicated logger.

				return this;
			}

			if (RequestItem.IsContextSensitive == true || Cmd is null)
			{
				Console.WriteLine($"Request \"{RequestId}\" is context sensitive and can only be executed in Discord as a slash command.");
			}

			//Loops through the dictionary, gets all the values, then sets the values in order.
			//NOTE: Might cause an error. The array might be of length 0.
			for (int i = 0; i < Args.Length; i++)
			{
				RequestItem.Params.Values.ToArray()[i] = Args[i];
			}

			RequestItem.Execute(null);

			return this;
		}



		/// <summary>
		/// Executes the request based on the SocketSlashCommand's attributes. For use in-Discord /commands only.
		/// </summary>
		/// <param name="Cmd"></param>
		/// <returns></returns>
		public RequestDirector ExecuteRequest(SocketSlashCommand Cmd)
		{
			Request RequestItem = GetRequest(Cmd.CommandName);

			//Cancels execution if Request is null.
			if (RequestItem == null) Console.WriteLine($"No Request \"{Cmd.CommandName}\" found."); //TODO: Move to a dedicated logger.

			IReadOnlyCollection<SocketSlashCommandDataOption> OptionCollection = Cmd.Data.Options;
			Dictionary<Object, Object> Args = new();

			//Adds every Option's Name and Value in the Dictionary.
			foreach (SocketSlashCommandDataOption option in OptionCollection)
				Args.Add(option.Name, option.Value);

			RequestItem.Execute(Args);

			return this;
		}
	}
}
