using System;
using System.Globalization;

namespace Ip4LogReaderLibrary
{
    public static class LogFileReaderStringExtensions
    {
        public static bool ParseAddressString(this string aAddressString, out uint aAddressValue)
        {
            bool aResult = false;
            aAddressValue = 0;
            string[] aAddressBytes = aAddressString.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if ((aAddressBytes.Length == 4)
                && int.TryParse(aAddressBytes[0], out int b4)
                && int.TryParse(aAddressBytes[1], out int b3)
                && int.TryParse(aAddressBytes[2], out int b2)
                && int.TryParse(aAddressBytes[3], out int b1)
                && (b4 >  0) && (b4 < 255)
                && (b3 >= 0) && (b3 < 256)
                && (b2 >= 0) && (b2 < 256)
                && (b1 >= 0) && (b1 < 256)
                )
            {
                aAddressValue = ((uint)b4 << 24) | ((uint)b3 << 16) | ((uint)b2 << 8) | (uint)b1;
                aResult = true;
            }

            return aResult;
        }

        public static bool ParseDateString(this string aDateString, out DateTime aDateValue)
        {
            return DateTime.TryParseExact(aDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out aDateValue);
        }

        public static bool ParseTimeString(this string aTimeString, out DateTime aDateValue)
        {
            return DateTime.TryParseExact(aTimeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out aDateValue);
        }
    }
}