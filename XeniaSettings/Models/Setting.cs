namespace XeniaSettings.Models
{
    internal class Setting
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        internal string OriginalValue { get; set; }
        internal int ValueIndex { get; set; }
        internal int ValueLength { get; set; }
    }
}
