using YeeLightAPI.YeeLightConstants;

namespace YeeLightAPI
{
    internal static class Utils
    {
        public static string GetJsonStringFromParamEnum(Constants.EffectParamValues value)
        {
            switch (value)
            {
                case Constants.EffectParamValues.SUDDEN:
                    return "\"sudden\"";
                case Constants.EffectParamValues.SMOOTH:
                    return "\"smooth\"";
            }
            return string.Empty;
        }
        public static string GetJsonStringFromParamEnum(Constants.PowerStateParamValues value)
        {
            switch (value)
            {
                case Constants.PowerStateParamValues.ON:
                    return "\"on\"";
                case Constants.PowerStateParamValues.OFF:
                    return "\"off\"";
            }
            return string.Empty;
        }
    }
}
