using System.Collections.Generic;
namespace Daemon.Model.Entities
{
    public class MassUpdateResult
    {
        public List<int> UpdatedIds { get; set; } = new List<int>();

        public List<int> FailedIds { get; set; } = new List<int>();

        public List<int> IgnoredIds { get; } = new List<int>();

        public string Error { get; set; }
    }
}
