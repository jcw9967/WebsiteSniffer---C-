using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CommandLine;
using WebsiteSnifferCSharp.src.models;
using WebsiteSnifferCSharp.utils;

namespace WebsiteSnifferCSharp
{
	internal class Program
	{
		private static int _threadCount;
		public static string OutputFilename { get; set; }
		public static string LocationDbFilename { get; set; }

		private static void Main( string[] args )
		{
			Options options = new Options();
			if( Parser.Default.ParseArguments( args, options ) )
			{
				OutputFilename = options.OutputFilename;
				LocationDbFilename = options.LocationDbFilename;
				_threadCount = options.Threads;

				if( options.URLFile != null )
				{
					string[] urls = File.ReadAllLines( options.URLFile );
					DatabaseHelper.Instance.InsertDomains( urls );
				}

				PerformTests();
			}
			else
			{
				Console.WriteLine( "Failure parsing args." );
			}
		}

		private static void PerformTests()
		{
			List<Domain> domains = DatabaseHelper.Instance.GetDomains();
			if( domains.Count > 0 )
			{
				Console.Write( "IPv6 is... " );
				Console.WriteLine( ( HasIpv6() ? "" : "NOT " ) + "available" );

				Console.Write( "Your location is... " );
				Location userLocation = LocationHelper.Instance.GetLocationForHost();
				Console.WriteLine( userLocation );

				if( userLocation != null )
				{
					Console.WriteLine( domains.Count + " URLs available. Beginning tests..." );

					//TODO
				}
				else
				{
					Console.WriteLine( "User location can't be found!" );
				}
			}
			else
			{
				Console.WriteLine( "There are no URLs to test! Did you use the '-urlfile <FILE>' argument?" );
			}
		}

		private static bool HasIpv6()
		{
			try
			{
				IPAddress address = DnsLookup.GetIpv6AddressForDomain( "google.com" );

				return PingHelper.GotResponse( address, 1 );
			}
			catch( Exception e ) when ( e is PingException || e is SocketException )
			{
			}

			return false;
		}
	}
}