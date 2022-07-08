namespace Project_Clovi
{
	public abstract class Request
	{
		/// <summary>
		/// DO NOT USE. This is used to chain back to the parent RequestDirector. It is set automatically when added to a RequestList.
		/// </summary>
		RequestDirector Parent { get; set; }

		/// <summary>
		/// The arguments that this Request can receive and process.
		/// </summary>
		Object[] Args { get; set; }

		/// <summary>
		/// The unique identifier for this command.
		/// </summary>
		public String Id { get; set; }

		/// <summary>
		/// Constructs a Request object.
		/// </summary>
		/// <param name="ParentArg"></param>
		/// <param name="IdArg"></param>
		/// <param name="ArgsArg"></param>
		Request(RequestDirector ParentArg, String IdArg, Object[] ArgsArg)
		{
			Parent = ParentArg;
			Id = IdArg;
			Args = ArgsArg;
		}

		/// <summary>
		/// Executes the custom request command.
		/// </summary>
		/// <returns name="This">This Request for method chaining.</returns>
		public abstract Request Execute();
	}
}
