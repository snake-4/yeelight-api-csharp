using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace YeeLightAPI
{
    internal class MessageTypes
    {
        internal class CommandMessage
        {
            [JsonProperty("id")]
            public int Id;

            [JsonProperty("method")]
            public string Method;

            [JsonProperty("params")]
            public object[] Params;
        };

        internal class ResponseMessage
        {
            [JsonProperty("id")]
            public int Id;

            [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
            public object[] Result;

            [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
            public ResponseError Error;
        };

        internal class ResponseError
        {
            [JsonProperty("code")]
            public int Code;

            [JsonProperty("message")]
            public string Message;
        };
    }
}
