using System.Net;
using WebsiteSnifferCSharp.models;
using WebsiteSnifferCSharp.utils;

namespace WebsiteSnifferCSharp.src.models
{
	internal class Ipv6Test : IpTest
	{
		public Ipv6Test( Domain domain ) : base( domain )
		{
		}

		public override IPAddress GetAddress()
		{
			if( !HasTestedAddress )
			{
				HasTestedAddress = true;

				Address = DnsLookup.GetIpv6AddressForDomain( Domain.Url );
			}

			return Address;
		}

		public override IPAddress GetMxAddress()
		{
			if( !HasTestedMxAddress )
			{
				HasTestedMxAddress = true;

				MxAddress = DnsLookup.GetIpv6AddressForDomain( DnsLookup.GetMxHostname( Domain.Url ) );
			}

			return MxAddress;
		}
	}
}