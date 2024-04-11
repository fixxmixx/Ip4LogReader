// See https://aka.ms/new-console-template for more information

using Ip4LogReader;
using Ip4LogReaderLibrary;

var aOptionsReader = new OptionsReader(args);

string aMessage = OptionsReader.Help;
if (!aOptionsReader.ShowHelp)
{
    var aOptions = aOptionsReader.Options;
    if (aOptions is not null)
    {
        var aLogFileReader = new LogFileReader(aOptions);
        int aResult = aLogFileReader.Execute();
        aMessage = $"Parsing the ip4 log file \"{aOptions.FileLog}\": " + ((aResult < 0) ? "failed." : $"{aResult} entries were written into the output file.");
    }
    else if (aOptionsReader.ErrorMessage != null)
        aMessage = aOptionsReader.ErrorMessage;
}

Console.WriteLine(aMessage);
