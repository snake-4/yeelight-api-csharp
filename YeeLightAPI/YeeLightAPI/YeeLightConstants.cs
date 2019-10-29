using System;
using System.Collections.Generic;
using System.Text;

namespace YeeLightAPI
{
    namespace YeeLightConstants
    {
        public static class Constants
        {
            public const int MinValueForDurationParameter = 30;

            public enum PowerStateParamValues
            {
                ON,
                OFF
            }

            public enum EffectParamValues
            {
                SUDDEN,
                SMOOTH
            }

            public const ushort DefaultCommandPort = 55443;
        }
    }
}
