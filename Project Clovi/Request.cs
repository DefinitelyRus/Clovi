namespace Project_Clovi
{
	public abstract class Request
	{
		/// <summary>
		/// This is used to chain back to the parent RequestDirector. It is set automatically when added to a RequestList.
		/// </summary>
		public RequestDirector Parent { get; internal set; }

		/// <summary>
		/// The arguments that this Request can receive and process.
		/// </summary>
		public Object[] Params { get; set; }

		/// <summary>
		/// The unique identifier for this command.
		/// </summary>
		public String Id { get; set; }

		/// <summary>
		/// Constructs a Request object.
		/// </summary>
		/// <param name="ParentArg"></param>
		/// <param name="IdArg"></param>
		/// <param name="ParamsArg"></param>
		public Request(RequestDirector ParentArg, String IdArg, Object[] ParamsArg)
		{
			Parent = ParentArg;
			Id = IdArg;
			Params = ParamsArg;
		}

		/// <summary>
		/// Executes the custom request command.
		/// </summary>
		/// <param name="Args">Any arguments required to execute the request.</param>
		/// <returns>This Request for method chaining.</returns>
		public abstract Request Execute(Object[]? Args);
	}
}
