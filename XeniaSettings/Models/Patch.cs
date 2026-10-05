namespace XeniaSettings.Models
{
    internal class Patch
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        internal string TableHeader { get; set; }
        internal string OriginalBody { get; set; }
        internal int EnabledValueIndex { get; set; }
        internal int EnabledValueLength { get; set; }

        public string Description
        {
            get
            {
                return OriginalBody.Remove(EnabledValueIndex, EnabledValueLength)
                    .Insert(EnabledValueIndex, IsEnabled ? "true" : "false");
            }
        }
    }
}
