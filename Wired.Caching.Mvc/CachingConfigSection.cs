using System.Configuration;

namespace Wired.Caching.Mvc
{
    /// <summary>
    /// Confirguration section class for setting global configuration.
    /// </summary>
    public class CachingConfigSection : ConfigurationSection
    {
        /// <summary>
        /// Always cache based on current user, effectively sets KeyOnUser for all attributes in project
        /// </summary>
        [ConfigurationProperty("alwaysKeyOnUser", DefaultValue = "false", IsRequired = false)]
        public bool AlwaysKeyOnUser
        {
            get { return (bool) this["alwaysKeyOnUser"]; }
            set { this["alwaysKeyOnUser"] = value; }
        }
    }
}
