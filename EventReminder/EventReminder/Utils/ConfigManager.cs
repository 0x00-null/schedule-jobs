using System.Configuration;

namespace EventReminder.Utils
{
    public static class ConfigManager
    {
        /// <summary>
        ///     Returns an application setting based on the passed in string
        ///     used primarily to cut down on typing
        /// </summary>
        /// <param name="key">The name of the key</param>
        /// <returns>
        ///     The value of the app setting in the web.Config
        ///     or String.Empty if no setting found
        /// </returns>
        public static string StringSetting(this string key)
        {
            var value = string.Empty;
            var setting = ConfigurationManager.AppSettings[key];
            if (setting != null)
                value = setting;
            return value;
        }

        public static int IntSetting(this string key)
        {
            var value = 0;
            var setting = ConfigurationManager.AppSettings[key];
            if (setting == null) return value;
            
            int result;
            if (int.TryParse(setting, out result))
            {
                value = result;
            }
            return value;
        }
    }
}