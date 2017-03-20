using System.Collections.Generic;

namespace PureSignalRClient.Types
{
    public class WsDataItem
    {
        /// <summary>
        ///     name of the hub
        /// </summary>
        public string H { get; set; }

        /// <summary>
        ///     name of the hub method
        /// </summary>
        public string M { get; set; }

        /// <summary>
        ///     method parameters (an array, empty if the method does not have any parameters)
        /// </summary>
        public List<object> A { get; set; }
    }
}