using System.Collections.Generic;

namespace RNA.Model
{
    public class Settings
    {
        public ReleaseData Data { get; set; }
        public ServerDetails Tfs { get; set; }
        public ServerDetails Azure { get; set; }
        public string DocumentLocation { get; set; }
        public string TestReport { get; set; }
        public List<string> WorkItemStateInclude { get; set; }
    }
}
