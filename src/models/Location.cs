using System.Text;

namespace WebsiteSnifferCSharp.src.models
{
	internal class Location
	{
		public Location( int id, string city, string country )
		{
			Id = id;
			City = city;
			Country = country;
		}

		public int Id { get; }
		public string City { get; }
		public string Country { get; }

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			if( City != null && !City.Equals( "" ) )
			{
				builder.Append( City ).Append( ", " );
			}

			builder.Append( Country );

			return builder.ToString();
		}
	}
}