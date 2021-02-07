using System;
using System.Collections.Generic;
using System.Text;

namespace YeeLightAPI
{
    namespace YeeLightExceptions
    {
        public static class Exceptions
        {
            [Serializable]
            public class DeviceIsNotConnected : Exception
            {
                //TODO: add something here
            }

            [Serializable]
            public class DeviceIsAlreadyInMusicMode : Exception
            {
                //TODO: add something here
            }

            [Serializable]
            public class InvalidHostnameArgument : Exception
            {
                //TODO: add something here
            }

            [Serializable]
            public class APIError : Exception
            {
                public APIError(int code, string message) : base(message)
                {
                    ErrorCode = code;
                }

                public int ErrorCode;
            }
        }
    }
}
