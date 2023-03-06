using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Daemon.Common
{
    public static class Utility
    {
        public static DateTime MinDate => new DateTime(1899, 12, 30, 12, 0, 0);

        public static long ToTimestamp(this DateTime dateTime)
        {
            var dateTimeOffset = new DateTimeOffset(dateTime.ToUniversalTime());
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        public static DateTime ParseUnixTime(long unixTime)
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        public static DateTime? ParseUnixTime(string text)
        {
            text = text?.Trim();
            if (!long.TryParse(text, out long unixTime))
            {
                return null;
            }

            return ParseUnixTime(unixTime);
        }

        public static (string name, int number) ExtractName(string text)
        {
            text = text ?? string.Empty;
            var match = Regex.Match(text, @"([\s\S]*)\((\d+)\)$");
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var number = int.Parse(match.Groups[2].Value);
                return (name, number);
            }

            return (text, 0);
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> items, int size)
        {
            for (int i = 0; i < items.Count; i += size)
            {
                yield return items.GetRange(i, Math.Min(size, items.Count - i));
            }
        }

        public static int ExchangeRedBlue(int color)
        {
            var bytes = BitConverter.GetBytes(color);
            var exchanged = new byte[] { bytes[2], bytes[1], bytes[0], 0 };
            return BitConverter.ToInt32(exchanged, 0);
        }

        /// <summary>
        /// Try parsing given Sql Db ConnectionString and output the Host and Port component in it.
        /// </summary>
        /// <param name="connStr"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns>Boolean value indicate whether valid Host and Port are specified in connection string.</returns>
        public static bool TryParseSqlDbConnectionString(string connStr, out string host, out int port)
        {
            try
            {
                host = string.Empty;
                port = 1433;

                string sqlServerDataSource = string.Empty;
                Regex regex = new Regex(@"Data Source\s*=\s*([^;]*);");
                Match m = regex.Match(connStr);
                if (m != null && m.Groups != null && m.Groups.Count == 2)
                {
                    sqlServerDataSource = !m.Groups[1].Value.Contains("\\") ? m.Groups[1].Value : m.Groups[1].Value.Split('\\')[0];

                    string[] arr = sqlServerDataSource.Split('\'')[0].Split(',');
                    if (arr.Length == 2)
                    {
                        sqlServerDataSource = arr[0];
                        if (!int.TryParse(arr[1], out port))
                        {
                            return false;
                        }
                    }

                    host = sqlServerDataSource;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                host = string.Empty;
                port = 0;
                return false;
            }
        }

        public static bool ValidateEmail(string email)
        {
            try
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
                return Regex.IsMatch(email, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch
            {
                return false;
            }
        }

        public static string ConvertADGuidToProGuid(string globallyUniqueId)
        {
            string converted = string.Empty;
            string[] data = globallyUniqueId.ToUpper().Split('-');

            for (int i = 0; i < data.Length; i++)
            {
                int count = data[i].Length;
                while (count > 0 && i <= 2)
                {
                    converted += data[i].Substring(data[i].Length - 2);
                    data[i] = data[i].Substring(0, data[i].Length - 2);
                    count -= 2;
                }

                converted += data[i];
            }

            return converted;
        }

        public static string ConvertProGuidToADGuid(string proGuid)
        {
            if (!ResolveGuid(proGuid, out string guid))
            {
                return null;
            }

            return string.Join(string.Empty, guid.Substring(0, 8).Split(2).Reverse())
                + string.Join(string.Empty, guid.Substring(8, 4).Split(2).Reverse())
                + string.Join(string.Empty, guid.Substring(12, 4).Split(2).Reverse())
                + guid.Substring(16);
        }

        public static string EscapeProGuid(string proGuid)
        {
            if (!ResolveGuid(proGuid, out string guid))
            {
                return null;
            }

            var byteStrings = guid.Split(2);
            var escapedByteString = byteStrings.Aggregate(string.Empty, (current, next) => current + "\\" + next);
            return escapedByteString;
        }

        public static IEnumerable<string> Split(this string text, int size)
        {
            for (var i = 0; i < text.Length / size; i++)
            {
                yield return new string(text.Skip(i * size).Take(size).ToArray());
            }
        }

        public static bool ResolveGuid(string text, out string resolvedGuid)
        {
            resolvedGuid = null;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            resolvedGuid = text.Replace("-", string.Empty);
            return Guid.TryParse(resolvedGuid, out _);
        }
    }
}
