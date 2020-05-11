using System.Collections.Generic;
using TfsData;

namespace ReleaseNotesGenerator.CLI
{
    public class Settings
    {
        public ReleaseData Data { get; set; }
        public TfsSettings Tfs { get; set; }
        public TfsSettings Azure { get; set; }
        public string DocumentLocation { get; set; }
        public string TestReport { get; set; }
        public List<string> WorkItemStateInclude { get; set; }
    }

}
