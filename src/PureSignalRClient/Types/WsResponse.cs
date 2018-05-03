using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PureSignalR.Types
{
    public class WsResponse
    {
		/// <summary>
		///     message id
		/// </summary>
		[DataMember(Name = "C")]
		public string C { get; set; }

		/// <summary>
		///     indicates that the transport was initialized (internal use)
		/// </summary>
		[DataMember(Name = "S")]
		public int? S { get; set; }

		/// <summary>
		///     groups token
		/// </summary>
		[DataMember(Name = "G")]
		public string G { get; set; }

		/// <summary>
		///     an array containing actual data
		/// </summary>
		[DataMember(Name = "M")]
		public List<WsDataItem> M { get; set; }

		/// <summary>
		///     invocation identifier, allows matchingp responses with requests
		/// </summary>
		[DataMember(Name = "I")]
		public string I { get; set; }

		/// <summary>
		///     the value returned by a server method (unless void)
		/// </summary>
		[DataMember(Name = "R")]
		public object R { get; set; }

		/// <summary>
		///     error message
		/// </summary>
		[DataMember(Name = "E")]
		public string E { get; set; }

		/// <summary>
		///     is hub error
		/// </summary>
		[DataMember(Name = "H")]
		public bool? H { get; set; }

		/// <summary>
		///     contains additional error data for hub errors)
		/// </summary>
		[DataMember(Name = "D")]
		public string D { get; set; }
    }
}