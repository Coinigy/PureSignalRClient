using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PureSignalR.Types
{
    public class WsDataItem
    {
        /// <summary>
        ///     name of the hub
        /// </summary>
        [DataMember(Name = "H")]
        public string H { get; set; }

		/// <summary>
		///     name of the hub method
		/// </summary>
		[DataMember(Name = "M")]
		public string M { get; set; }

		/// <summary>
		///     method parameters (an array, empty if the method does not have any parameters)
		/// </summary>
		[DataMember(Name = "A")]
		public List<object> A { get; set; }
    }
}