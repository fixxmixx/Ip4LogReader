using Ip4LogReaderLibrary;
using getopt.net;
using Newtonsoft.Json;

namespace Ip4LogReader
{
    internal class OptionsReader
    {
        public OptionsReader(string[] aArgs)
        {
            var aGetOpt = new GetOpt {
                    AppArgs = aArgs,
                    Options = new[] {
                        new Option("help",          ArgumentType.None,      'h'),
                        new Option("file-config",   ArgumentType.Required,  'c'),
                        new Option("file-log",      ArgumentType.Required,  'i'),
                        new Option("file-output",   ArgumentType.Required,  'o'),
                        new Option("address-start", ArgumentType.Required,  'a'),
                        new Option("address-mask",  ArgumentType.Required,  'm'),
                        new Option("time-start",    ArgumentType.Required,  't'),
                        new Option("time-end",      ArgumentType.Required,  'e')
                    },
                    ShortOpts = "hc:i:o:",
                    AllowParamFiles = true,
                    AllowWindowsConventions = true,
                    AllowPowershellConventions = true
                };

            string? aFileLog = default;
            string? aFileOutput = default;
            uint aAddressStart = default;
            uint aAddressMask = default;
            DateTime aTimeStart = default;
            DateTime aTimeEnd = default;
            string? aFileConfig = default;

            bool aAnyOption = false;
            int optChar = 0;
            try
            {
                while ((optChar = aGetOpt.GetNextOpt(out var optArg)) != -1)
                {
                    switch (optChar)
                    {
                        case 'h':
                            ShowHelp = true;
                            return;
                        case 'c':
                            if (CheckFileExists(optArg))
                                aFileConfig = optArg;
                            else
                            {
                                ErrorMessage = $"Configuration file \"{optArg}\" does not exist.";
                                return;
                            }
                            break;
                        case 'i':
                            aAnyOption = true;
                            if (CheckFileExists(optArg))
                                aFileLog = optArg;
                            else
                            {
                                ErrorMessage = $"Input file \"{optArg}\" does not exist.";
                                return;
                            }
                            break;
                        case 'o':
                            aAnyOption = true;
                            aFileOutput = optArg;
                            break;
                        case 'a':
                            aAnyOption = true;
                            if (!string.IsNullOrEmpty(optArg) && optArg.ParseAddressString(out var aAddressValue))
                                aAddressStart = aAddressValue;
                            else
                            {
                                ErrorMessage = $"address-start \"{optArg}\" is not a valid IP4 address string.";
                                return;
                            }
                            break;
                        case 'm':
                            aAnyOption = true;
                            if (!string.IsNullOrEmpty(optArg) && int.TryParse(optArg, out int aAddressMask1))
                            {
                                if ((aAddressMask1 > 0) && (aAddressMask1 <= 32))
                                    aAddressMask = (uint)aAddressMask1;
                                else
                                {
                                    ErrorMessage = "address-mask must be greater than 0 and less than 33.";
                                    return;
                                }
                            }
                            else
                            {
                                ErrorMessage = $"address-mask \"{optArg}\" is not a valid integer value.";
                                return;
                            }
                            break;
                        case 'e':
                        case 't':
                            aAnyOption = true;
                            if (!string.IsNullOrEmpty(optArg) && optArg.ParseDateString(out var aDateTimeValue1))
                            {
                                if (optChar == 't')
                                    aTimeStart = aDateTimeValue1;
                                else
                                    aTimeEnd = aDateTimeValue1;
                            }
                            else
                            {
                                string aArgName = "time-" + ((optChar == 't') ? "start" : "end");
                                ErrorMessage = $"{aArgName} is not a valid date.";
                                return;
                            }
                            break;
                    }
                }
            }
            catch (ParseException ex)
            {
                ErrorMessage = $"Error occurred while parsing {ex.Option}: {ex.Message}";
                return;
            }

            if (!aAnyOption && string.IsNullOrEmpty(aFileConfig))
            {
                ShowHelp = true;
                return;
            }

            if (!string.IsNullOrEmpty(aFileConfig))
            {
                if (aAnyOption)
                {
                    ErrorMessage = "The file-config option must be the only option.";
                    return;
                }

                try
                {
                    Dictionary<string, string>? aConfigDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(aFileConfig));

                    if (aConfigDictionary is not null)
                    {
                        string? optArg = "";
                        if (aConfigDictionary.TryGetValue("file-log", out optArg) && CheckFileExists(optArg))
                            aFileLog = optArg;
                        else
                        {
                            ErrorMessage = $"Input file \"{optArg}\" does not exist.";
                            return;
                        }
                        optArg = "";
                        if (aConfigDictionary.TryGetValue("file-output", out optArg))
                            aFileOutput = optArg;
                        optArg = "";
                        if (aConfigDictionary.TryGetValue("address-start", out optArg))
                        {
                            if (!string.IsNullOrEmpty(optArg) && optArg.ParseAddressString(out var aAddressValue))
                                aAddressStart = aAddressValue;
                            else
                            {
                                ErrorMessage = $"address-start \"{optArg}\" is not a valid IP4 address string.";
                                return;
                            }
                        }
                        optArg = "";
                        if (aConfigDictionary.TryGetValue("address-mask", out optArg))
                        {
                            if (!string.IsNullOrEmpty(optArg) && int.TryParse(optArg, out int aAddressMask1))
                            {
                                if ((aAddressMask1 > 0) && (aAddressMask1 <= 32))
                                    aAddressMask = (uint)aAddressMask1;
                                else
                                {
                                    ErrorMessage = "address-mask must be greater than 0 and less than 33.";
                                    return;
                                }
                            }
                            else
                            {
                                ErrorMessage = $"address-mask \"{optArg}\" is not a valid integer value.";
                                return;
                            }
                        }
                        optArg = "";
                        if (aConfigDictionary.TryGetValue("time-start", out optArg))
                        {
                            if (!string.IsNullOrEmpty(optArg) && optArg.ParseDateString(out var aDateTimeValue1))
                            {
                                aTimeStart = aDateTimeValue1;
                            }
                            else
                            {
                                ErrorMessage = $"time-start is not a valid date.";
                                return;
                            }
                        }
                        optArg = "";
                        if (aConfigDictionary.TryGetValue("time-end", out optArg))
                        {
                            if (!string.IsNullOrEmpty(optArg) && optArg.ParseDateString(out var aDateTimeValue1))
                            {
                                aTimeEnd = aDateTimeValue1;
                            }
                            else
                            {
                                ErrorMessage = $"time-end is not a valid date.";
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    ErrorMessage = "Error reading the configuration file.";
                    return;
                }
            }

            if (string.IsNullOrEmpty(aFileLog))
            {
                ErrorMessage = "The file-log option must be set.";
                return;
            }

            if (string.IsNullOrEmpty(aFileOutput))
            {
                ErrorMessage = "The file-output option must be set.";
                return;
            }

            if ((aAddressMask != 0) && (aAddressStart == 0))
            {
                ErrorMessage = "The address-mask option can be set only if the address-start option is set.";
                return;
            }

            if ((aTimeStart != default) && (aTimeEnd != default) && (aTimeStart > aTimeEnd))
            {
                ErrorMessage = "time-end must be greater or equal to time-start.";
                return;
            }

            Options = new LogFileReaderOptions(aFileLog, aFileOutput, aAddressStart, aAddressMask, aTimeStart, aTimeEnd);
        }

        private static bool CheckFileExists(string? aFileName)
        {
            return !string.IsNullOrWhiteSpace(aFileName) && File.Exists(aFileName);
        }

        public bool ShowHelp { get; }
        public string? ErrorMessage { get; }
        public LogFileReaderOptions? Options { get; }

        public static string Help => @"The tool parses the input log with ip4 addresses and tells you how many queries came from the addresses that meet the given criteria.

Usage:
    --file-config — configuration file. Must be the only option if given.
    --file-log — the input log file.
    --file-output — the output file where the tool put the result summary into.
    --address-start — the starting ip4 address. Optional.
    --address-mask — the subnet mask. Optional. May be set only if address-start is set.
    --time-start — the low bound of the time interval (dd.MM.yyyy). Optional.
    --time-end — the high bound of the time interval (dd.MM.yyyy). Optional.";
    }
}