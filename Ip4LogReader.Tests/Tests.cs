using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

using Ip4LogReaderLibrary;
using NUnit.Options;

namespace Ip4LogReader.Tests
{
    public class Ip4LogReaderTests
    {
        const string aFileLogName = "input.log";
        const string aFileOutputName = "output.txt";
        const string aFileOptionsName = "options.txt";

        private Random mRandom;

        [SetUp]
        public void Setup()
        {
            mRandom = new Random();
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestOne(bool aSetAddressStart)
        {
            uint aAddressStart = aSetAddressStart ? (uint)mRandom.Next() : 0;
            uint aAddressMask = (uint)mRandom.Next(31);
            DateTime aTimeStart = new(mRandom.NextInt64(DateTime.MaxValue.Ticks));
            DateTime aTimeEnd = new(mRandom.NextInt64(DateTime.MaxValue.Ticks));

            int aRecordCount = mRandom.Next(16384);

            uint aAddressEnd = uint.MaxValue;
            if (aAddressStart != 0)
            {
                if (aAddressMask != 0)
                    aAddressEnd = (aAddressStart & (aAddressEnd << (32 - (int)aAddressMask))) | (aAddressEnd >> (int)aAddressMask);
            }

            Dictionary<uint, int> aOutputResult = new Dictionary<uint, int>();

            StringBuilder aInput = new();
            for (int i = 0; i < aRecordCount; i++)
            {
                uint aAddress = (uint)mRandom.Next();
                uint b4 = (aAddress >> 24) & 0xff;
                if ((b4 > 0) && (b4 < 255))
                {
                    int aRepeatCount = mRandom.Next(128) + 1;
                    DateTime aDate = new(mRandom.NextInt64(DateTime.MaxValue.Ticks));
                    for (int j = 0; j < aRepeatCount; j++)
                        aInput.AppendLine(aAddress.ToAddressString() + ":" + aDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    if ((aAddress >= aAddressStart) && (aAddress <= aAddressEnd) && (aDate >= aTimeStart) && (aDate <= aTimeEnd))
                    {
                        int aCount = 0;
                        aOutputResult.TryGetValue(aAddress, out aCount);
                        aOutputResult[aAddress] = aCount + aRepeatCount;
                    }
                }
            }

            if (aAddressStart != 0)
            {
                for (int i = 0; i < aRecordCount; i++)
                {
                    uint aAddress = aAddressStart + (uint)mRandom.Next(255);
                    uint b4 = (aAddress >> 24) & 0xff;
                    if ((b4 > 0) && (b4 < 255))
                    {
                        int aRepeatCount = mRandom.Next(128) + 1;
                        DateTime aDate = new(mRandom.NextInt64(DateTime.MaxValue.Ticks));
                        for (int j = 0; j < aRepeatCount; j++)
                            aInput.AppendLine(aAddress.ToAddressString() + ":" + aDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        if ((aAddress >= aAddressStart) && (aAddress <= aAddressEnd) && (aDate >= aTimeStart) && (aDate <= aTimeEnd))
                        {
                            int aCount = 0;
                            aOutputResult.TryGetValue(aAddress, out aCount);
                            aOutputResult[aAddress] = aCount + aRepeatCount;
                        }
                    }
                }
            }

            File.WriteAllText(aFileLogName, aInput.ToString(), Encoding.Unicode);
            File.WriteAllText(aFileOptionsName, $"{aAddressStart.ToAddressString()}\n{aAddressEnd.ToAddressString()}/{aAddressMask}\n{aTimeStart.ToShortDateString()}\n{aTimeEnd.ToShortDateString()}", Encoding.Unicode);

            var aOptions = new LogFileReaderOptions(aFileLogName, aFileOutputName, aAddressStart, aAddressMask, aTimeStart, aTimeEnd);
            var aLogFileReader = new LogFileReader(aOptions);
            aLogFileReader.Execute();

            //Assert.That(File.Exists(aFileOutputName), Is.True);
            Assert.That(aLogFileReader.Result, Is.EquivalentTo(aOutputResult));
        }
    }
}
