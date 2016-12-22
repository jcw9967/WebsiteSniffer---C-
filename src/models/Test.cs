using System;

namespace WebsiteSnifferCSharp.src.models
{
	internal class Test
	{
		public Test( Domain domain, Location userLocation )
		{
			Domain = domain;
			Timestamp = (long) ( DateTime.Now - DateTime.Parse( "1/1/1970 0:0:0" ) ).TotalMilliseconds;
			UserLocation = userLocation;
		}

		public Domain Domain { get; }
		public long Timestamp { get; }
		public Location UserLocation { get; }
		public Ipv4Test Ipv4Test { get; set; }
		public Ipv6Test Ipv6Test { get; set; }
	}
}