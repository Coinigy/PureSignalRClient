using System.Collections.Generic;

namespace PureSignalRClient.Types
{
    public class WsResponse
    {
        /// <summary>
        ///     message id
        /// </summary>
        public string C { get; set; }

        /// <summary>
        ///     indicates that the transport was initialized (internal use)
        /// </summary>
        public int? S { get; set; }

        /// <summary>
        ///     groups token
        /// </summary>
        public string G { get; set; }

        /// <summary>
        ///     an array containing actual data
        /// </summary>
        public List<WsDataItem> M { get; set; }

        /// <summary>
        ///     invocation identifier, allows matchingp responses with requests
        /// </summary>
        public string I { get; set; }

        /// <summary>
        ///     the value returned by a server method (unless void)
        /// </summary>
        public object R { get; set; }

        /// <summary>
        ///     error message
        /// </summary>
        public string E { get; set; }

        /// <summary>
        ///     is hub error
        /// </summary>
        public bool? H { get; set; }

        /// <summary>
        ///     contains additional error data for hub errors)
        /// </summary>
        public string D { get; set; }
    }
}