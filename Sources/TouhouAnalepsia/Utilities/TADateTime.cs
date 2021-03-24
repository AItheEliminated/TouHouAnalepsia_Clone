using System;
using System.Globalization;

namespace TouhouAnalepsia
{
    public static class TADateTime
    {
        public static string GetLocalTime(string regionName = "en-US")
        {
            return DateTime.Now.ToString(new CultureInfo(regionName));
        }
    }
}
