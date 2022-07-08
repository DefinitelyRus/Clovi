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
		public Request? GetRequestItem(String RequestId)
		{
			foreach (Request Req in RequestList)
			{
				if (Req.Id.Equals(RequestId)) return Req;
			}


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
			List<Request> NewList = ListToSort.Cast<Request>().ToList();

			//TODO: Implement quicksort here

			return NewList;
		}



		/// <summary>
		/// Executes the request based on the given RequestId and Args. For use in console or in-text commands. 
		/// </summary>
		/// <param name="RequestId">The Id of the target Request.</param>
		/// <param name="Args">The arguments required to execute the Request.</param>
		/// <returns>This RequestDirector for method chaining.</returns>
		public RequestDirector ExecuteRequest(String RequestId, String[] Args)
		{
			Request RequestItem = GetRequestItem(RequestId);

			if (RequestItem == null) Console.WriteLine($"No Request \"{RequestId}\" found."); //TODO: Move to a dedicated logger.

			RequestItem.Execute(Args);

			return this;
		}



		/// <summary>
		/// Executes the request based on the SocketSlashCommand's attributes. For use in Discord /commands only.
		/// </summary>
		/// <param name="Cmd"></param>
		/// <returns></returns>
		public RequestDirector ExecuteRequest(SocketSlashCommand Cmd)
		{
			Request RequestItem = GetRequestItem(Cmd.CommandName);

			if (RequestItem == null) Console.WriteLine($"No Request \"{Cmd.CommandName}\" found."); //TODO: Move to a dedicated logger.

			SocketSlashCommandDataOption[] OptionArray = Cmd.Data.Options.ToArray();
			Object[] Args = new object[OptionArray.Length];
			
			for (int i = 0; i < OptionArray.Length; i++) Args[i] = OptionArray[i].Value;

			RequestItem.Execute(Args);

			return this;
		}
	}
}
