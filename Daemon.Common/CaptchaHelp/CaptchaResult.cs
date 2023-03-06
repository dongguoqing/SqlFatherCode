using System.IO;
using System;
using System.Runtime.Serialization;
namespace Daemon.Common.CaptchaHelp
{
    public class CaptchaResult
    {
        /// <summary>
        /// CaptchaCode
        /// </summary>
        public string CaptchaCode { get; set; }

        /// <summary>
        /// CaptchaMemoryStream
        /// </summary>
        [IgnoreDataMember]
        public MemoryStream CaptchaMemoryStream { get; set; }

        /// <summary>
        /// 64
        /// </summary>
        /// <value></value>
        public string Base64Str { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
