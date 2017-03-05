namespace SonarLogAPI.Primitives
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	//can be replased to GeoCoordinate if nesesary https://msdn.microsoft.com/en-us/library/system.device.location.geocoordinate(v=vs.110).aspx
	//?

	/// <summary>
	/// Represents a geographical location point that is determined by <see cref="SonarLogAPI.Primitives.Latitude" /> and <see cref="SonarLogAPI.Primitives.Longitude" /> coordinates.
	/// </summary>
	public class CoordinatePoint : IEquatable<CoordinatePoint>
	{

		//https://en.wikipedia.org/wiki/World_Geodetic_System
		private const double _equatorialEarthRadius = 6378137.0D; //meters
		private const double _d2R = Math.PI / 180D;

		/// <summary>
		/// Point <see cref="Primitives.Latitude" />. Between −90°(South/low) and +90°(North/high)
		/// </summary>

		//Широта 
		public Latitude Latitude { get; }

		/// <summary>
		/// Point <see cref="Primitives.Longitude" />. Between −180°(West) and +180°(East)
		/// </summary>

		//Долгота
		public Longitude Longitude { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> class 
		/// from <see cref="SonarLogAPI.Primitives.Latitude" /> and <see cref="SonarLogAPI.Primitives.Longitude" /> objects.
		/// </summary>
		/// <param name="latitude"><see cref="SonarLogAPI.Primitives.Latitude" /></param>
		/// <param name="longitude"><see cref="SonarLogAPI.Primitives.Longitude" /></param>
		public CoordinatePoint(Latitude latitude, Longitude longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> class 
		/// from <see cref="SonarLogAPI.Primitives.Latitude" /> and <see cref="SonarLogAPI.Primitives.Longitude" /> degrees values.
		/// </summary>
		/// <param name="latitude"><see cref="SonarLogAPI.Primitives.Latitude" /> degrees value.</param>
		/// <param name="longitude"><see cref="SonarLogAPI.Primitives.Longitude" /> degrees value.</param>
		public CoordinatePoint(double latitude, double longitude)
			: this(new Latitude(latitude), new Longitude(longitude)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> class
		/// from <see cref="SonarLogAPI.Primitives.Latitude" /> and <see cref="SonarLogAPI.Primitives.Longitude" /> degrees, minutes and seconds values.
		/// </summary>
		/// <param name="latitudeDegrees"><see cref="SonarLogAPI.Primitives.Latitude" /> Degrees</param>
		/// <param name="latitudeMinutes"><see cref="SonarLogAPI.Primitives.Latitude" /> Minutes</param>
		/// <param name="latitudeSeconds"><see cref="SonarLogAPI.Primitives.Latitude" /> Seconds</param>
		/// <param name="longitudeDegrees"><see cref="SonarLogAPI.Primitives.Longitude" /> Degrees</param>
		/// <param name="longitudeMinutes"><see cref="SonarLogAPI.Primitives.Longitude" /> Minutes</param>
		/// <param name="longitudeSeconds"><see cref="SonarLogAPI.Primitives.Longitude" /> Seconds</param>
		public CoordinatePoint(double latitudeDegrees, double latitudeMinutes, double latitudeSeconds,
			double longitudeDegrees, double longitudeMinutes, double longitudeSeconds)
			: this(new Latitude(latitudeDegrees, latitudeMinutes, latitudeSeconds), new Longitude(longitudeDegrees, longitudeMinutes, longitudeSeconds)) { }

		public bool Equals(CoordinatePoint other)
		{
			//Check whether the compared object is null. 
			if (ReferenceEquals(other, null)) return false;

			//Check whether the compared object references the same data. 
			if (ReferenceEquals(this, other)) return true;

			//Check whether the products' properties are equal. 
			return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
		}

		/// <summary>
		/// Determines whether two <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> objects refer to the same location.
		/// </summary>
		/// <param name="left">The first <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> to compare.</param>
		/// <param name="right">The second <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> to compare.</param>
		/// <returns>true, if the <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> objects are determined to be equivalent; otherwise, false.</returns>
		public static bool operator ==(CoordinatePoint left, CoordinatePoint right)
		{
			return (object)left != null && (object)right != null && left.Equals(right);
		}

		/// <summary>Determines whether two <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> objects correspond to different locations.</summary>
		/// <param name="left">The first <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> to compare.</param>
		/// <param name="right">The second <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> to compare.</param>
		/// <returns>true, if the <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> objects are determined to be different; otherwise, false.</returns>
		public static bool operator !=(CoordinatePoint left, CoordinatePoint right)
		{
			return !(left == right);
		}


		/// <summary>
		/// Determines if a specified <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> 
		/// is equal to the current <see cref="SonarLogAPI.Primitives.CoordinatePoint" />.
		/// </summary>
		/// <param name="obj">The object to compare the <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> to.</param>
		/// <returns>True, if the <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> objects are equal; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			var item = obj as CoordinatePoint;

			return item != null && Equals(item);
		}

		public override int GetHashCode()
		{
			return (Latitude?.GetHashCode() ?? 0) ^ (Longitude?.GetHashCode() ?? 0);
		}

		/// <summary>
		/// Returns the distance between two <see cref="SonarLogAPI.Primitives.CoordinatePoint" />.
		/// </summary>
		/// <param name="firstPoint">First point</param>
		/// <param name="secondPoint">Second point</param>
		/// <returns><see cref="LinearDimension"/></returns>
		public static LinearDimension DistanceBetweenPoints(CoordinatePoint firstPoint, CoordinatePoint secondPoint)
		{
			return DistanceBetweenPoints(firstPoint.Latitude.ToDouble(),
				firstPoint.Longitude.ToDouble(), secondPoint.Latitude.ToDouble(), secondPoint.Longitude.ToDouble());
		}

		/// <summary>
		/// Returns the distance in meters between two <see cref="SonarLogAPI.Primitives.CoordinatePoint" />.
		/// </summary>
		/// <param name="lat1">First point <see cref="SonarLogAPI.Primitives.Latitude" /> degrees double value.</param>
		/// <param name="long1">First point <see cref="SonarLogAPI.Primitives.Longitude" /> degrees double value.</param>
		/// <param name="lat2">Second point <see cref="SonarLogAPI.Primitives.Latitude" /> degrees double value.</param>
		/// <param name="long2">Second point <see cref="SonarLogAPI.Primitives.Longitude" /> degrees double value.</param>
		/// <returns><see cref="LinearDimension"/></returns>
		/// <seealso cref="http://stackoverflow.com/a/7595937/5888216"/>
		public static LinearDimension DistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
		{
			double dlong = (long2 - long1) * _d2R;
			double dlat = (lat2 - lat1) * _d2R;
			double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(lat1 * _d2R) * Math.Cos(lat2 * _d2R) * Math.Pow(Math.Sin(dlong / 2D), 2D);
			double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
			double d = _equatorialEarthRadius * c;

			return new LinearDimension(d,LinearDimensionUnit.Meter);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0},{1}", Latitude.ToDouble(), Longitude.ToDouble());
		}
	}
}