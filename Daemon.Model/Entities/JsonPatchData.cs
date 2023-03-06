namespace Daemon.Model.Entities
{
    public class JsonPatchData
    {
        public string Id { get; set; }

        public string From { get; set; }

        public string Op { get; set; }

        public string Path { get; set; }

        public string Value { get; set; }

        public bool? NotifyStopfinder { get; set; }
    }
}
