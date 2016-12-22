using CommandLine;
using CommandLine.Text;

namespace WebsiteSnifferCSharp
{
	internal class Options
	{
		[Option( 'f', "urlfile", HelpText = "Add URLs from a given file" )]
		public string URLFile { get; set; }

		[Option( 't', "threads", DefaultValue = 50, HelpText = "Specify the number of concurrent threads to use. Default = 50" )]
		public int Threads { get; set; }

		[Option( 'o', "output", DefaultValue = "sniffer.db", HelpText = "Specify the output filename of the SQLite database" )]
		public string OutputFilename { get; set; }

		[Option( 'l', "locationdb", DefaultValue = "GeoLite2-City.mmdb", HelpText = "Specify the name of the location database to use. Default = 'GeoLite2-City.mmdb'" )]
		public string LocationDbFilename { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild( this, current => HelpText.DefaultParsingErrorsHandler( this, current ) );
		}
	}
}