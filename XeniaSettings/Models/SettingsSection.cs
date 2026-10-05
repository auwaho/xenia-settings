using System.Collections.Generic;

namespace XeniaSettings.Models
{
    internal class SettingsSection
    {
        public string SectionName { get; set; }
        public List<Setting> Settings { get; set; }
    }
}
