using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XeniaSettings.Models
{
    internal class ConfigFile
    {
        public string FilePath { get; set; }
        public List<SettingsSection> Sections { get; set; }
        internal Encoding Encoding { get; set; }
        internal byte[] OriginalBytes { get; set; }
        internal string OriginalContent { get; set; }

        public bool IsModified => Sections.SelectMany(s => s.Settings).Any(s => s.Value != s.OriginalValue);

        public string Content
        {
            get
            {
                var content = new StringBuilder(OriginalContent);
                foreach (Setting setting in Sections.SelectMany(s => s.Settings)
                    .Where(s => s.Value != s.OriginalValue).OrderByDescending(s => s.ValueIndex))
                {
                    content.Remove(setting.ValueIndex, setting.ValueLength);
                    content.Insert(setting.ValueIndex, setting.Value);
                }
                return content.ToString();
            }
        }

        internal void AcceptChanges(string content, byte[] bytes)
        {
            int offset = 0;
            foreach (Setting setting in Sections.SelectMany(s => s.Settings).OrderBy(s => s.ValueIndex))
            {
                setting.ValueIndex += offset;
                int length = setting.Value.Length;
                offset += length - setting.ValueLength;
                setting.ValueLength = length;
                setting.OriginalValue = setting.Value;
            }
            OriginalContent = content;
            OriginalBytes = bytes;
        }
    }
}
