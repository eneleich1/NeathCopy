using System.Collections.Generic;

namespace NeathCopyEngine.Helpers
{
    public sealed class MultiDestinationCopyItem
    {
        public string SourcePath { get; set; }
        public string SourceDisplayPath { get; set; }
        public string RelativePath { get; set; }
        public long Length { get; set; }
    }

    public sealed class MultiDestinationCopyRequest
    {
        public List<MultiDestinationCopyItem> Items { get; set; } = new List<MultiDestinationCopyItem>();
        public List<string> DestinationRoots { get; set; } = new List<string>();
        public int Threads { get; set; } = 1;
    }
}
