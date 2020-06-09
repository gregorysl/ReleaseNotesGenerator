namespace RNA.Model
{
    public class ReleaseData
    {
        public string TfsProject { get; set; }
        public string TfsBranch { get; set; }
        public string Iteration { get; set; }
        public string ChangesetFrom { get; set; }
        public string ChangesetTo { get; set; }
        public string ReleaseName { get; set; }
        public string ReleaseDate { get; set; }
        public string QaBuildName { get; set; }
        public string QaBuildDate { get; set; }
    }
}
