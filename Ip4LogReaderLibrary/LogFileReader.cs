using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ip4LogReaderLibrary
{
    public class LogFileReader
    {
        private const string OutputDelimiter = ":";

        private readonly LogFileReaderOptions mOptions;

        private readonly uint mAddressStart;
        private readonly uint mAddressEnd;
        private readonly DateTime mTimeStart;
        private readonly DateTime mTimeEnd;

        public LogFileReader(LogFileReaderOptions? aOptions)
        {
            mOptions = aOptions ?? throw new ArgumentNullException(nameof(aOptions));

            uint aAddressStart = 0;
            uint aAddressEnd = uint.MaxValue;
            if (mOptions.AddressStart != 0)
            {
                aAddressStart = mOptions.AddressStart;
                if (mOptions.AddressMask != 0)
                    aAddressEnd = (aAddressStart & (aAddressEnd << (32 - (int)mOptions.AddressMask))) | (aAddressEnd >> (int)mOptions.AddressMask);
            }

            mAddressStart = aAddressStart;
            mAddressEnd = aAddressEnd;
            mTimeStart = (mOptions.TimeStart != default) ? mOptions.TimeStart : DateTime.MinValue;
            mTimeEnd = (mOptions.TimeEnd != default) ? mOptions.TimeEnd : DateTime.MaxValue;

        }

        public Dictionary<uint, int> Result { get; } = new();

        private static bool ParseLine(string aLine, out uint aAddress, out DateTime aDate)
        {
            aAddress = default;
            aDate = default;
            string[] aParts = aLine.Split(':', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return (aParts.Length >= 2) && aParts[0].ParseAddressString(out aAddress) && aParts[1].ParseTimeString(out aDate);
        }

        private bool IsInRange(uint aAddress, DateTime aDate)
        {
            return (aAddress >= mAddressStart) && (aAddress <= mAddressEnd) && (aDate >= mTimeStart) && (aDate <= mTimeEnd);
        }

        public int Execute()
        {
            Result.Clear();

            int aResult = -1;
            if (!string.IsNullOrWhiteSpace(mOptions.FileLog))
            {
                string[]? aLines = null;
                try
                {
                    aLines = File.ReadAllLines(mOptions.FileLog);
                }
                catch
                {
                }

                if (aLines != null)
                {
                    foreach (string aLine in aLines)
                        if (ParseLine(aLine, out uint aAddress, out DateTime aDate) && IsInRange(aAddress, aDate))
                        {
                            int aCount = 0;
                            Result.TryGetValue(aAddress, out aCount);
                            Result[aAddress] = ++aCount;
                        }

                    aResult = Result.Count;
                    StringBuilder aOutput = new();
                    foreach (var aItem in Result)
                        aOutput.AppendLine(aItem.Key.ToAddressString() + $"{OutputDelimiter}{aItem.Value}");

                    if (!string.IsNullOrWhiteSpace(mOptions.FileOutput))
                        try
                        {
                            File.WriteAllText(mOptions.FileOutput, aOutput.ToString(), Encoding.Unicode);
                        }
                        catch
                        {
                        }
                }
            }

            return aResult;
        }

    }
}