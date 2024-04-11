using System;

namespace Ip4LogReaderLibrary
{
    public record LogFileReaderOptions(string? FileLog, string? FileOutput, uint AddressStart, uint AddressMask, DateTime TimeStart, DateTime TimeEnd);
}