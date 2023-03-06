namespace Daemon.Model
{
    using System;
    public partial class RecordIDFilter
    {
        public int Id { get; set; }
        public int RecordID { get; set; }
        public string Hash { get; set; }
        public string Type { get; set; }
        public Nullable<System.DateTime> CreateDateTime { get; set; }
    }
}
