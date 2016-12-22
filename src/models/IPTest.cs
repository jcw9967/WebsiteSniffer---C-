using System.Net;
using WebsiteSnifferCSharp.src.models;
using WebsiteSnifferCSharp.utils;

namespace WebsiteSnifferCSharp.models
{
	internal abstract class IpTest
	{
		public readonly Domain Domain;
		private Location _addressLocation;

		private bool _hasTestedAddressLocation;
		private bool _hasTestedHttpStatusCode;
		private bool _hasTestedMxAddressLocation;
		private bool _hasTestedPing;
		private bool _hasTestedWorkingSmtp;
		private bool _hasWorkingSmtp;
		private int? _httpStatusCode;
		private Location _mxAddressLocation;
		private long? _ping;

		protected bool HasTestedAddress;
		protected IPAddress Address;
		protected bool HasTestedMxAddress;
		protected IPAddress MxAddress;

		protected IpTest( Domain domain )
		{
			Domain = domain;
		}

		public Location GetAddressLocation()
		{
			if( HasTestedAddress && !_hasTestedAddressLocation )
			{
				_hasTestedAddressLocation = true;

				if( Address != null )
				{
					_addressLocation = LocationHelper.Instance.GetLocationByIP( Address );
				}
			}

			return _addressLocation;
		}

		public Location GetMxAddressLocation()
		{
			if( HasTestedMxAddress && !_hasTestedMxAddressLocation )
			{
				_hasTestedMxAddressLocation = true;

				if( MxAddress != null )
				{
					_mxAddressLocation = LocationHelper.Instance.GetLocationByIP( MxAddress );
				}
			}

			return _mxAddressLocation;
		}

		public bool HasWorkingSmtp()
		{
			if( HasTestedMxAddress && !_hasTestedWorkingSmtp )
			{
				_hasTestedWorkingSmtp = true;

				if( MxAddress != null )
				{
					_hasWorkingSmtp = SmtpHelper.CanConnect( MxAddress );
				}
			}

			return _hasWorkingSmtp;
		}

		public int? GetHttpStatusCode()
		{
			if( HasTestedAddress && !_hasTestedHttpStatusCode )
			{
				_hasTestedHttpStatusCode = true;

				if( Address != null )
				{
					HttpWebRequest request = WebRequest.CreateHttp( Address.ToString() );
					request.Method = "HEAD";

					using( HttpWebResponse response = (HttpWebResponse) request.GetResponse() )
					{
						_httpStatusCode = (int) response.StatusCode;
					}
				}
			}

			return _httpStatusCode;
		}

		public long? GetPing()
		{
			if( HasTestedAddress && !_hasTestedPing )
			{
				_hasTestedPing = true;

				_ping = PingHelper.MinimumPing( Address, 4 );
			}

			return _ping;
		}

		public abstract IPAddress GetAddress();
		public abstract IPAddress GetMxAddress();
	}
}