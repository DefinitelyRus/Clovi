using System.Collections;
namespace Project_Clovi
{
	/// <summary>
	/// Directs all Request reads and changes.
	/// </summary>
	class RequestDirector
	{
		/// <summary>
		/// A list of available requests.
		/// Sorted alphabetically based on Request Ids.
		/// </summary>
		ArrayList RequestList { get; set; }

		/// <summary>
		/// Searches for and returns the first Request Object that matches the Request Id argument.
		/// </summary>
		/// <param name="RequestId">The Id of the target Request.</param>
		/// <returns>The Request Object with a matching Id.</returns>
		Request? GetRequestItem(String RequestId)
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
		RequestDirector(ArrayList RequestListArg)
		{
			RequestList = RequestListArg; //Sort alphabetically.
		}

		/// <summary>
		/// Adds the inputted Request Object to the RequestList.
		/// Also auto-sorts the list alphabetically.
		/// </summary>
		/// <param name="NewRequest">The new Request to be added.</param>
		/// <returns></returns>
		RequestDirector AddRequestItem(Request NewRequest)
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
		List<Request> Sort(ArrayList ListToSort)
		{
			List<Request> NewList = ListToSort.Cast<Request>().ToList();

			//TODO: Implement quicksort here

			return NewList;
		}
	}
}
