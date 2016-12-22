using System;
using System.IO;
using System.Net;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using WebsiteSnifferCSharp.src.models;

namespace WebsiteSnifferCSharp.utils
{
	internal class LocationHelper
	{
		private static LocationHelper _instance;

		public static LocationHelper Instance => _instance ?? ( _instance = new LocationHelper() );

		public Location GetLocationForHost()
		{
			IPAddress ip;

			HttpWebRequest request = (HttpWebRequest) WebRequest.Create( "http://checkip.amazonaws.com" );
			request.Method = "GET";

			using( HttpWebResponse response = (HttpWebResponse) request.GetResponse() )
			using( Stream stream = response.GetResponseStream() )
			using( StreamReader reader = new StreamReader( stream ) )
			{
				ip = IPAddress.Parse( reader.ReadToEnd().Trim() );
			}

			return GetLocationByIP( ip );
		}

		public Location GetLocationByIP( IPAddress ip )
		{
			if( ip == null )
			{
				throw new ArgumentNullException( nameof( ip ) );
			}

			using( DatabaseReader reader = new DatabaseReader( Program.LocationDbFilename ) )
			{
				CityResponse response = reader.City( ip );

				string city = response.City.Name;
				string country = response.Country.Name;
				double? latitude = response.Location.Latitude;
				double? longitude = response.Location.Longitude;

				Location location = DatabaseHelper.Instance.GetLocation( city, country );
				if( location != null )
				{
					DatabaseHelper.Instance.InsertLocation( city, country, latitude, longitude );
					location = DatabaseHelper.Instance.GetLocation( city, country );
				}

				return location;
			}
		}
	}
}