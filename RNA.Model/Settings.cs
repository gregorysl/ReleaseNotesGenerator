using System.Collections.Generic;

namespace RNA.Model
{
    public class Settings
    {
        public ReleaseData Data { get; set; }
        public ServerDetails ChangesetsServer { get; set; }
        public ServerDetails WorkItemServer { get; set; }
        public Jive UploadSettings { get; set; }
        public string DocumentLocation { get; set; }
        public string TestReport { get; set; }
        public List<string> WorkItemStateInclude { get; set; }
        public string DateFormat { get; set; } = "yyyy-MM-dd";
    }

    public class Jive
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string PlaceId { get; set; }
    }
}
