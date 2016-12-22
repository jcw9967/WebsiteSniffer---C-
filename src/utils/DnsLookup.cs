using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace WebsiteSnifferCSharp.utils
{
	internal class DnsLookup
	{
		public static IPAddress GetIpv4AddressForDomain( string domain )
		{
			if( domain == null )
			{
				throw new ArgumentNullException( nameof( domain ) );
			}

			DnsMessage message = DnsClient.Default.Resolve( DomainName.Parse( domain ) );
			IEnumerable<ARecord> records = message.AnswerRecords.OfType<ARecord>();
			string address = null;
			foreach( ARecord record in records )
			{
				address = record.Address.ToString();
				break;
			}

			return IPAddress.Parse( address );
		}

		public static IPAddress GetIpv6AddressForDomain( string domain )
		{
			if( domain == null )
			{
				throw new ArgumentNullException( nameof( domain ) );
			}

			DnsMessage message = DnsClient.Default.Resolve( DomainName.Parse( domain ), RecordType.Aaaa );
			IEnumerable<AaaaRecord> records = message.AnswerRecords.OfType<AaaaRecord>();
			string address = null;
			foreach( AaaaRecord record in records )
			{
				address = record.Address.ToString();
				break;
			}

			return IPAddress.Parse( address );
		}

		public static string GetMxHostname( string domain )
		{
			if( domain == null )
			{
				throw new ArgumentNullException( nameof( domain ) );
			}

			DnsMessage message = DnsClient.Default.Resolve( DomainName.Parse( domain ), RecordType.Mx );
			IEnumerable<MxRecord> records = message.AnswerRecords.OfType<MxRecord>();
			MxRecord record = null;
			foreach( MxRecord tmpRecord in records )
			{
				if( record == null || tmpRecord.Preference < record.Preference )
				{
					record = tmpRecord;
				}
			}

			return record?.ExchangeDomainName.ToString();
		}
	}
}