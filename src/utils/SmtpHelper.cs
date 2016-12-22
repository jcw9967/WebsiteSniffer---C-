using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace WebsiteSnifferCSharp.utils
{
	internal class SmtpHelper
	{
		public static bool CanConnect( IPAddress address )
		{
			if( address == null )
			{
				throw new ArgumentNullException( nameof( address ) );
			}

			bool canConnect = false;

			using( TcpClient client = new TcpClient() )
			{
				client.Connect( address, 25 );

				using( SslStream sslStream = new SslStream( client.GetStream(), false ) )
				{
					sslStream.AuthenticateAsClient( address.ToString() );

					using( StreamWriter writer = new StreamWriter( sslStream ) )
					using( StreamReader reader = new StreamReader( sslStream ) )
					{
						writer.WriteLine( "HELO " + address );
						writer.Flush();

						string response = reader.ReadLine();
						if( response != null && ( response.StartsWith( "250" ) || response.StartsWith( "220" ) || response.StartsWith( "200" ) ) )
						{
							canConnect = true;
						}
					}
				}
			}

			return canConnect;
		}
	}
}