using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XeniaSettings.Models
{
    internal class PatchFile
    {
        public string FilePath { get; set; }
        public string TitleName { get; set; }
        public string TitleId { get; set; }
        public string Description { get; set; }
        public List<Patch> Patches { get; set; }
        internal Encoding Encoding { get; set; }
        internal byte[] OriginalBytes { get; set; }
        internal string OriginalContent { get; set; }

        public string Content => Description + string.Concat(Patches.Select(p => p.TableHeader + p.Description));
        public bool IsModified => Content != OriginalContent;
    }
}
