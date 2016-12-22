using System.Collections.Generic;
using Mono.Data.Sqlite;
using WebsiteSnifferCSharp.src.models;

namespace WebsiteSnifferCSharp.utils
{
	internal class DatabaseHelper
	{
		private static DatabaseHelper _instance;
		private readonly string _connectionStr = "Data Source=" + Program.OutputFilename + ";Version=3";

		private DatabaseHelper()
		{
			createDatabase();
		}

		public static DatabaseHelper Instance
		{
			get
			{
				if( _instance == null )
				{
					_instance = new DatabaseHelper();
				}

				return _instance;
			}
		}

		public List<Domain> GetDomains()
		{
			List<Domain> domains = new List<Domain>();

			using( SqliteConnection connection = new SqliteConnection( _connectionStr ) )
			{
				connection.Open();

				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "SELECT "
					                      + Domains.FieldId + ','
					                      + Domains.FieldUrl
					                      + " FROM "
					                      + Domains.TableName;

					using( SqliteDataReader reader = command.ExecuteReader() )
					{
						while( reader.Read() )
						{
							domains.Add( new Domain( reader.GetInt32( 0 ), reader.GetString( 1 ) ) );
						}
					}
				}
			}

			return domains;
		}

		public void InsertDomains( string[] domains )
		{
			using( SqliteConnection connection = new SqliteConnection( _connectionStr ) )
			{
				connection.Open();

				using( SqliteTransaction transaction = connection.BeginTransaction() )
				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "INSERT INTO " + Domains.TableName + '('
					                      + Domains.FieldUrl
					                      + ") VALUES (@url)";

					foreach( string domain in domains )
					{
						command.Parameters.AddWithValue( "@url", domain );
						command.ExecuteNonQuery();

						command.Parameters.Clear();
					}

					transaction.Commit();
				}
			}
		}

		public Location GetLocation( string city, string country )
		{
			Location location = null;

			using( SqliteConnection connection = new SqliteConnection( _connectionStr ) )
			{
				connection.Open();

				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "SELECT "
					                      + Locations.FieldId + ','
					                      + Locations.FieldCity + ','
					                      + Locations.FieldCountry
					                      + " FROM "
					                      + Locations.TableName
					                      + " WHERE "
					                      + Locations.FieldCity + "=@city AND "
					                      + Locations.FieldCountry + "=@country LIMIT 1";

					command.Parameters.AddWithValue( "@city", city );
					command.Parameters.AddWithValue( "@country", country );

					using( SqliteDataReader reader = command.ExecuteReader() )
					{
						if( reader.Read() )
						{
							location = new Location( reader.GetInt32( 0 ), reader.GetString( 1 ), reader.GetString( 2 ) );
						}
					}
				}
			}

			return location;
		}

		public void InsertLocation( string city, string country, double? latitude, double? longitude )
		{
			using( SqliteConnection connection = new SqliteConnection( _connectionStr ) )
			{
				connection.Open();

				using( SqliteTransaction transaction = connection.BeginTransaction() )
				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "INSERT INTO " + Locations.TableName + '('
					                      + Locations.FieldCity + ','
					                      + Locations.FieldCountry + ','
					                      + Locations.FieldLatitude + ','
					                      + Locations.FieldLongitude
					                      + ") VALUES (@city,@country,@latitude,@longitude)";

					command.Parameters.AddWithValue( "@city", city );
					command.Parameters.AddWithValue( "@country", country );
					command.Parameters.AddWithValue( "@latitude", latitude );
					command.Parameters.AddWithValue( "@longitude", longitude );
					command.ExecuteNonQuery();

					transaction.Commit();
				}
			}
		}

		private int GetNextTestNumber( int domainId )
		{
			int nextTestNumber = 1;

			using( SqliteConnection connection = new SqliteConnection( _connectionStr ) )
			{
				connection.Open();

				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "SELECT COALESCE(MAX(" + Tests.FieldTestNumber + "),0)+1"
					                      + " FROM "
					                      + Tests.TableName
					                      + " WHERE "
					                      + Tests.FieldFkDomainId + "=? LIMIT 1";

					command.Parameters.Add( domainId );

					using( SqliteDataReader reader = command.ExecuteReader() )
					{
						if( reader.Read() )
						{
							nextTestNumber = reader.GetInt32( 0 );
						}
					}
				}
			}

			return nextTestNumber;
		}

		public void InsertTest( Test test )
		{
			using( SqliteConnection connection = new SqliteConnection( _connectionStr ) )
			{
				connection.Open();

				using( SqliteTransaction transaction = connection.BeginTransaction() )
				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "INSERT INTO " + Tests.TableName + '('
					                      + Tests.FieldFkDomainId + ','
					                      + Tests.FieldTestNumber + ','
					                      + Tests.FieldTimestamp + ','
					                      + Tests.FieldFkLocationId + ','
					                      + Tests.FieldFkIpv4TestId + ','
					                      + Tests.FieldFkIpv6TestId
					                      + ") VALUES (@domainId,@testNumber,@timestamp,@locationId,@ipv4TestId,@ipv6TestId)";

					command.Parameters.AddWithValue( "@domainId", test.Domain.Id );
					command.Parameters.AddWithValue( "@testNumber", GetNextTestNumber( test.Domain.Id ) );
					command.Parameters.AddWithValue( "@timestamp", test.Timestamp );
					command.Parameters.AddWithValue( "@locationId", test.UserLocation.Id );

					int? ipv4TestPk = InsertIpv4Test( connection, test.Ipv4Test );
					command.Parameters.AddWithValue( "@ipv4TestId", ipv4TestPk );

					int? ipv6TestPk = InsertIpv6Test( connection, test.Ipv6Test );
					command.Parameters.AddWithValue( "@ipv6TestId", ipv6TestPk );

					command.ExecuteNonQuery();

					transaction.Commit();
				}
			}
		}

		private int? InsertIpv4Test( SqliteConnection connection, Ipv4Test ipv4Test )
		{
			int? pk = null;

			if( ipv4Test != null )
			{
				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "INSERT INTO " + Ipv4Tests.TableName + '('
					                      + Ipv4Tests.FieldAddress + ','
					                      + Ipv4Tests.FieldAddressPing + ','
					                      + Ipv4Tests.FieldFkAddressLocationId + ','
					                      + Ipv4Tests.FieldHttpStatusCode + ','
					                      + Ipv4Tests.FieldMxAddress + ','
					                      + Ipv4Tests.FieldFkMxAddressLocationId + ','
					                      + Ipv4Tests.FieldHasWorkingSmtp
					                      + ") VALUES (@address,@ping,@addressLocationId,@httpStatusCode,@mxAddress,@mxAddressLocationId,@hasWorkingSmtp)";

					command.Parameters.AddWithValue( "@address", ipv4Test.GetAddress() );
					command.Parameters.AddWithValue( "@ping", ipv4Test.GetPing() );
					command.Parameters.AddWithValue( "@addressLocationId", ipv4Test.GetAddressLocation()?.Id );
					command.Parameters.AddWithValue( "@httpStatusCode", ipv4Test.GetHttpStatusCode() );
					command.Parameters.AddWithValue( "@mxAddress", ipv4Test.GetMxAddress() );
					command.Parameters.AddWithValue( "@mxAddressLocationId", ipv4Test.GetMxAddressLocation()?.Id );
					command.Parameters.AddWithValue( "@hasWorkingSmtp", ipv4Test.HasWorkingSmtp() );

					pk = (int) command.ExecuteScalar();
				}
			}

			return pk;
		}

		private int? InsertIpv6Test( SqliteConnection connection, Ipv6Test ipv6Test )
		{
			int? pk = null;

			if( ipv6Test != null )
			{
				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "INSERT INTO " + Ipv6Tests.TableName + '('
					                      + Ipv6Tests.FieldAddress + ','
					                      + Ipv6Tests.FieldAddressPing + ','
					                      + Ipv6Tests.FieldFkAddressLocationId + ','
					                      + Ipv6Tests.FieldHttpStatusCode + ','
					                      + Ipv6Tests.FieldMxAddress + ','
					                      + Ipv6Tests.FieldFkMxAddressLocationId + ','
					                      + Ipv6Tests.FieldHasWorkingSmtp
					                      + ") VALUES (@address,@ping,@addressLocationId,@httpStatusCode,@mxAddress,@mxAddressLocationId,@hasWorkingSmtp)";

					command.Parameters.AddWithValue( "@address", ipv6Test.GetAddress() );
					command.Parameters.AddWithValue( "@ping", ipv6Test.GetPing() );
					command.Parameters.AddWithValue( "@addressLocationId", ipv6Test.GetAddressLocation()?.Id );
					command.Parameters.AddWithValue( "@httpStatusCode", ipv6Test.GetHttpStatusCode() );
					command.Parameters.AddWithValue( "@mxAddress", ipv6Test.GetMxAddress() );
					command.Parameters.AddWithValue( "@mxAddressLocationId", ipv6Test.GetMxAddressLocation()?.Id );
					command.Parameters.AddWithValue( "@hasWorkingSmtp", ipv6Test.HasWorkingSmtp() );

					pk = (int) command.ExecuteScalar();
				}
			}

			return pk;
		}

		private void createDatabase()
		{
			using( SqliteConnection connection = new SqliteConnection( _connectionStr ) )
			{
				connection.Open();

				using( SqliteTransaction transaction = connection.BeginTransaction() )
				using( SqliteCommand command = connection.CreateCommand() )
				{
					command.CommandText = "CREATE TABLE IF NOT EXISTS " + Domains.TableName + "("
					                      + Domains.FieldId + " INTEGER,"
					                      + Domains.FieldUrl + " TEXT NOT NULL,"
					                      + "PRIMARY KEY(" + Domains.FieldId + "),"
					                      + "UNIQUE(" + Domains.FieldUrl + ") ON CONFLICT IGNORE"
					                      + ")";
					command.ExecuteNonQuery();

					command.CommandText = "CREATE TABLE IF NOT EXISTS " + Locations.TableName + "("
					                      + Locations.FieldId + " INTEGER,"
					                      + Locations.FieldCity + " TEXT,"
					                      + Locations.FieldCountry + " TEXT,"
					                      + Locations.FieldLatitude + " REAL,"
					                      + Locations.FieldLongitude + " REAL,"
					                      + "PRIMARY KEY(" + Locations.FieldId + "),"
					                      + "UNIQUE(" + Locations.FieldCity + ',' + Locations.FieldCountry + ") ON CONFLICT IGNORE"
					                      + ")";
					command.ExecuteNonQuery();

					command.CommandText = "CREATE TABLE IF NOT EXISTS " + Ipv4Tests.TableName + "("
					                      + Ipv4Tests.FieldId + " INTEGER,"
					                      + Ipv4Tests.FieldAddress + " TEXT,"
					                      + Ipv4Tests.FieldAddressPing + " INTEGER,"
					                      + Ipv4Tests.FieldFkAddressLocationId + " INTEGER,"
					                      + Ipv4Tests.FieldHttpStatusCode + " INTEGER,"
					                      + Ipv4Tests.FieldMxAddress + " TEXT,"
					                      + Ipv4Tests.FieldFkMxAddressLocationId + " INTEGER,"
					                      + Ipv4Tests.FieldHasWorkingSmtp + " INTEGER,"
					                      + "FOREIGN KEY(" + Ipv4Tests.FieldFkAddressLocationId + ") REFERENCES " + Locations.TableName + "(" + Locations.FieldId + "),"
					                      + "FOREIGN KEY(" + Ipv4Tests.FieldFkMxAddressLocationId + ") REFERENCES " + Locations.TableName + "(" + Locations.FieldId + "),"
					                      + "PRIMARY KEY(" + Ipv4Tests.FieldId + ")"
					                      + ")";
					command.ExecuteNonQuery();

					command.CommandText = "CREATE TABLE IF NOT EXISTS " + Ipv6Tests.TableName + "("
					                      + Ipv6Tests.FieldId + " INTEGER,"
					                      + Ipv6Tests.FieldAddress + " TEXT,"
					                      + Ipv6Tests.FieldAddressPing + " INTEGER,"
					                      + Ipv6Tests.FieldFkAddressLocationId + " INTEGER,"
					                      + Ipv6Tests.FieldHttpStatusCode + " INTEGER,"
					                      + Ipv6Tests.FieldMxAddress + " TEXT,"
					                      + Ipv6Tests.FieldFkMxAddressLocationId + " INTEGER,"
					                      + Ipv6Tests.FieldHasWorkingSmtp + " INTEGER,"
					                      + "FOREIGN KEY(" + Ipv6Tests.FieldFkAddressLocationId + ") REFERENCES " + Locations.TableName + "(" + Locations.FieldId + "),"
					                      + "FOREIGN KEY(" + Ipv6Tests.FieldFkMxAddressLocationId + ") REFERENCES " + Locations.TableName + "(" + Locations.FieldId + "),"
					                      + "PRIMARY KEY(" + Ipv6Tests.FieldId + ")"
					                      + ")";
					command.ExecuteNonQuery();

					command.CommandText = "CREATE TABLE IF NOT EXISTS " + Tests.TableName + "("
					                      + Tests.FieldFkDomainId + " INTEGER,"
					                      + Tests.FieldTestNumber + " INTEGER,"
					                      + Tests.FieldTimestamp + " INTEGER NOT NULL,"
					                      + Tests.FieldFkLocationId + " INTEGER NOT NULL,"
					                      + Tests.FieldFkIpv4TestId + " INTEGER,"
					                      + Tests.FieldFkIpv6TestId + " INTEGER,"
					                      + "FOREIGN KEY(" + Tests.FieldFkDomainId + ") REFERENCES " + Domains.TableName + "(" + Domains.FieldId + "),"
					                      + "FOREIGN KEY(" + Tests.FieldFkLocationId + ") REFERENCES " + Locations.TableName + "(" + Locations.FieldId + "),"
					                      + "FOREIGN KEY(" + Tests.FieldFkIpv4TestId + ") REFERENCES " + Ipv4Tests.TableName + "(" + Ipv4Tests.FieldId + "),"
					                      + "FOREIGN KEY(" + Tests.FieldFkIpv6TestId + ") REFERENCES " + Ipv6Tests.TableName + "(" + Ipv6Tests.FieldId + "),"
					                      + "PRIMARY KEY(" + Tests.FieldFkDomainId + ',' + Tests.FieldTestNumber + ")"
					                      + ")";

					transaction.Commit();
				}
			}
		}

		public static class Domains
		{
			public const string TableName = "Domains";
			public const string FieldId = "DomainID";
			public const string FieldUrl = "URL";
		}

		public static class Locations
		{
			public const string TableName = "Locations";
			public const string FieldId = "LocationID";
			public const string FieldCity = "City";
			public const string FieldCountry = "Country";
			public const string FieldLatitude = "Latitude";
			public const string FieldLongitude = "Longitude";
		}

		public static class Tests
		{
			public const string TableName = "Tests";
			public const string FieldFkDomainId = "FK_DomainID";
			public const string FieldTestNumber = "TestNumber";
			public const string FieldTimestamp = "Timestamp";
			public const string FieldFkLocationId = "FK_LocationID";
			public const string FieldFkIpv4TestId = "FK_IPv4TestID";
			public const string FieldFkIpv6TestId = "FK_IPv6TestID";
		}

		public static class Ipv4Tests
		{
			public const string TableName = "IPv4Tests";
			public const string FieldId = "IPv4TestID";
			public const string FieldAddress = "IPv4Address";
			public const string FieldAddressPing = "IPv4AddressPing";
			public const string FieldFkAddressLocationId = "FK_IPv4AddressLocationID";
			public const string FieldHttpStatusCode = "IPv4HttpStatusCode";
			public const string FieldMxAddress = "IPv4MXAddress";
			public const string FieldFkMxAddressLocationId = "FK_IPv4MXAddressLocationID";
			public const string FieldHasWorkingSmtp = "IPv4HasWorkingSMTP";
		}

		public static class Ipv6Tests
		{
			public const string TableName = "IPv6Tests";
			public const string FieldId = "IPv6TestID";
			public const string FieldAddress = "IPv6Address";
			public const string FieldAddressPing = "IPv6AddressPing";
			public const string FieldFkAddressLocationId = "FK_IPv6AddressLocationID";
			public const string FieldHttpStatusCode = "IPv6HttpStatusCode";
			public const string FieldMxAddress = "IPv6MXAddress";
			public const string FieldFkMxAddressLocationId = "FK_IPv6MXAddressLocationID";
			public const string FieldHasWorkingSmtp = "IPv6HasWorkingSMTP";
		}
	}
}