namespace Daemon.Model
{
    using System;
    public class ClientConfig
    {
        public short Id { get; set; }
        public string SMTPHost { get; set; }
        public Nullable<int> SMTPPort { get; set; }
        public string SMTPUserName { get; set; }
        public string SMTPPassword { get; set; }
        public bool SMTPSSL { get; set; }
        public string EmailAddress { get; set; }
        public string EmailName { get; set; }
        public string TimeZone { get; set; }
        public bool RespectDaylightSavings { get; set; }
        public bool GroupPoints { get; set; }
        public int ClusterMinMapPoints { get; set; }
        public int ClusterMinZoomLevel { get; set; }
        public decimal ClusterTolerance { get; set; }
        public string ClusterToleranceUnit { get; set; }
        public string TripsID { get; set; }
        public string Extent { get; set; }
        public Nullable<System.DateTime> LastAutoReportLoad { get; set; }
        public bool PromptToExit { get; set; }
        public bool SafetyPrompts { get; set; }
        public bool ShowOpenOnStart { get; set; }
        public bool ShowTipOfDay { get; set; }
        public bool ArchivePrompt { get; set; }
        public bool OpenAsGrid { get; set; }
        public bool UseDetailedLog { get; set; }
        public string PictureImportFolder { get; set; }
        public Nullable<int> SessionTimeout { get; set; }
        public string WayfinderLayout { get; set; }
        public short TripCalcOption { get; set; }
        public bool AllowMultifactorAuthEmail { get; set; }
        public bool AllowMultifactorAuthSMS { get; set; }
        public string UnitOfMeasure { get; set; }
    }
}
