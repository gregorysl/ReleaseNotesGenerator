using System;
using System.Collections.Generic;

namespace RNA.Model
{
    public class ChangesetInfo
    {
        public bool Selected { get; set; } = true;

        public int Id { get; set; }
        public string CommitedBy { get; set; }
        public DateTime Created { get; set; }
        public string Comment { get; set; } = "";
        public List<string> Categories { get; set; }
        public override string ToString()
        {
            return $"{Id} {Comment} {CommitedBy} {Created.ToShortDateString()} ";
        }
    }
}
