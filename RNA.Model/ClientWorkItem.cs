using Newtonsoft.Json;

namespace RNA.Model
{
    public class ClientWorkItem
    {
        private string _legacyClientProject;
        private string _clientProject;

        [JsonProperty("System.Id")]
        public string Id { get; set; }

        [JsonProperty("System.Title")]
        public string Title { get; set; }

        [JsonProperty("System.State")]
        public string State { get; set; }

        [JsonProperty("Custom.ClientProject")]
        public string ClientProject
        {
            get => _clientProject;
            set
            {
                if (string.IsNullOrWhiteSpace(ClientProject))
                {
                    _clientProject = value;
                }
            }
        }


        [JsonProperty("client.project")]
        public string LegacyClientProject

        {
            get => _legacyClientProject;
            set
            {
                if (string.IsNullOrWhiteSpace(ClientProject))
                {
                    ClientProject = value;
                }
                _legacyClientProject = value;
            }
        }

        [JsonProperty("System.WorkItemType")]
        public string WorkItemType { get; set; }
        [JsonProperty("System.BoardColumn")]
        public string BoardColumn { get; set; }
        public override string ToString()
        {
            return $"{Id} {Title} {ClientProject} {State}";
        }
    }
}
