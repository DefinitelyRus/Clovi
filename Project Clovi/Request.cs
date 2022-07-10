namespace Project_Clovi
{
	public abstract class Request
	{
		/// <summary>
		/// This is used to chain back to the parent RequestDirector. It is set automatically when added to a RequestList.
		/// </summary>
		public RequestDirector Parent { get; internal set; }

		/// <summary>
		/// Used as a read-only copy of Params, in case the public Params' keys get modified.
		/// </summary>
		private Dictionary<Object, Object?> InternalParams { get; set; }

		/// <summary>
		/// The arguments that this Request can receive and process.
		/// </summary>
		public Dictionary<Object, Object?> Params { get; set; }

		/// <summary>
		/// The unique identifier for this command.
		/// </summary>
		public String Id { get; set; }

		/// <summary>
		/// <para>Identifies the request as context sensitive or context insensitive.
		/// Context sensitive requests need information such as the source channel or any direct interactions with Discord API.
		/// Context insensitive requests are used for internal changes, with feedback that can be generalized.</para>
		/// 
		/// <para>For example:</para>
		/// <para>"/WhoIs @User#0000" - Returns details about User#0000, which needs direct contact with Discord API.</para>
		/// <para>"/RestartBot" - Only needs to restart the bot, no need to access Discord API.</para>
		/// </summary>
		public Boolean IsContextSensitive { get; }

		/// <summary>
		/// Constructs a Request object.
		/// </summary>
		/// <param name="ParentArg"></param>
		/// <param name="IdArg"></param>
		/// <param name="ParamsArg"></param>
		public Request(RequestDirector ParentArg, String IdArg, Dictionary<Object, Object?> ParamsArg, Boolean IsContextSensitiveArg = true)
		{
			Parent = ParentArg;
			Id = IdArg;
			Params = ParamsArg;
		}

		/// <summary>
		/// Executes the custom request command.
		/// The Dictionary argument should be retrieved from this Request.Params, then modify its values.
		/// </summary>
		/// <param name="Args">Any arguments required to execute the request. Typically this Request.Param with modified values.</param>
		/// <returns>This Request for method chaining.</returns>
		public abstract Request Execute(Dictionary<Object, Object>? Args);
	}
}
