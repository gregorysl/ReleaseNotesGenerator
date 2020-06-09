using System.Text.Json.Serialization;

namespace RNA.Model
{
    public class ClientWorkItem
    {
        private string _legacyClientProject;
        private string _clientProject;

        [JsonPropertyName("System.Id")]
        public int Id { get; set; }

        [JsonPropertyName("System.Title")]
        public string Title { get; set; }

        [JsonPropertyName("System.State")]
        public string State { get; set; }

        [JsonPropertyName("Custom.ClientProject")]
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


        [JsonPropertyName("client.project")]
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

        [JsonPropertyName("System.WorkItemType")]
        public string WorkItemType { get; set; }
        [JsonPropertyName("System.BoardColumn")]
        public string BoardColumn { get; set; }
        public override string ToString()
        {
            return $"{Id} {Title} {ClientProject} {State}";
        }
    }
}
