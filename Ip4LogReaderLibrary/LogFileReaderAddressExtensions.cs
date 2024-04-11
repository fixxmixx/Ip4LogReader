namespace Ip4LogReaderLibrary
{
    public static class LogFileReaderAddressExtensions
    {
        public static string ToAddressString(this uint aAddress)
        {
            uint[] aBytes = new uint[4];
            aBytes[0] = (aAddress >> 24) & 0xff;
            aBytes[1] = (aAddress >> 16) & 0xff;
            aBytes[2] = (aAddress >> 8 ) & 0xff;
            aBytes[3] =  aAddress        & 0xff;
            return string.Join('.', aBytes);
        }
    }
}