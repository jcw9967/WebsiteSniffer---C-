using System.Net;
using WebsiteSnifferCSharp.models;
using WebsiteSnifferCSharp.utils;

namespace WebsiteSnifferCSharp.src.models
{
	internal class Ipv4Test : IpTest
	{
		public Ipv4Test( Domain domain ) : base( domain )
		{
		}

		public override IPAddress GetAddress()
		{
			if( !HasTestedAddress )
			{
				HasTestedAddress = true;

				Address = DnsLookup.GetIpv4AddressForDomain( Domain.Url );
			}

			return Address;
		}

		public override IPAddress GetMxAddress()
		{
			if( !HasTestedMxAddress )
			{
				HasTestedMxAddress = true;

				MxAddress = DnsLookup.GetIpv4AddressForDomain( DnsLookup.GetMxHostname( Domain.Url ) );
			}

			return MxAddress;
		}
	}
}