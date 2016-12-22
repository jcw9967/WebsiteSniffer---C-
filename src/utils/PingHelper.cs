using System.Net;
using System.Net.NetworkInformation;

namespace WebsiteSnifferCSharp.utils
{
	public static class PingHelper
	{
		private const int PingTimeout = 4000;

		public static long? MinimumPing( IPAddress address, byte numPings )
		{
			long? minimumPing = null;
			Ping ping = new Ping();

			for( byte i = 0; i < numPings; ++i )
			{
				var reply = ping.Send( address, PingTimeout );
				if( reply != null && reply.Status == IPStatus.Success )
				{
					minimumPing = reply.RoundtripTime;
				}
			}

			ping.Dispose();
			return minimumPing;
		}

		public static long? MinimumPing( string url, byte numPings )
		{
			long? minimumPing = null;
			Ping ping = new Ping();

			for( byte i = 0; i < numPings; ++i )
			{
				PingReply reply = ping.Send( url, PingTimeout );
				if( reply != null && reply.Status == IPStatus.Success )
				{
					minimumPing = reply.RoundtripTime;
				}
			}

			ping.Dispose();
			return minimumPing;
		}

		public static bool GotResponse( IPAddress address, byte maxAttempts )
		{
			bool returnVal = false;
			Ping ping = new Ping();

			for( byte i = 0; i < maxAttempts; ++i )
			{
				PingReply reply = ping.Send( address, PingTimeout );
				if( reply != null && reply.Status == IPStatus.Success )
				{
					returnVal = true;
					break;
				}
			}

			ping.Dispose();
			return returnVal;
		}
	}
}