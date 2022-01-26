namespace SonarLogAPI.Primitives
{
	using System;
	using System.Globalization;

	//can be replaced to GeoCoordinate if necessary https://msdn.microsoft.com/en-us/library/system.device.location.geocoordinate(v=vs.110).aspx
	//?

	/// <summary>
	/// Represents a geographical location point that is determined by <see cref="Primitives.Latitude" /> and <see cref="Primitives.Longitude" /> coordinates.
	/// </summary>
	public class CoordinatePoint : IEquatable<CoordinatePoint>
	{

		//https://en.wikipedia.org/wiki/Earth_radius#Mean_radius
		private const double _earthWgs84MeanRadius = 6371008.8D; //meters

		//https://en.wikipedia.org/wiki/World_Geodetic_System#WGS84
		private const double _earthWgs84EquatorialRadius = 6378137.0D; //The Earth's equatorial radius "a" in meters.
		private const double _earthWgs84PolarRadius = 6356752.31424518d; // The Earth's polar radius "b" in meters.

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
		/// Initializes a new instance of the <see cref="CoordinatePoint" /> class 
		/// from <see cref="Primitives.Latitude" /> and <see cref="Primitives.Longitude" /> objects.
		/// </summary>
		/// <param name="latitude"><see cref="Primitives.Latitude" /></param>
		/// <param name="longitude"><see cref="SonarLogAPI.Primitives.Longitude" /></param>
		public CoordinatePoint(Latitude latitude, Longitude longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatePoint" /> class 
		/// from <see cref="Primitives.Latitude" /> and <see cref="Primitives.Longitude" /> degrees values.
		/// </summary>
		/// <param name="latitude"><see cref="Primitives.Latitude" /> degrees value.</param>
		/// <param name="longitude"><see cref="Primitives.Longitude" /> degrees value.</param>
		public CoordinatePoint(double latitude, double longitude)
			: this(new Latitude(latitude), new Longitude(longitude)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatePoint" /> class
		/// from <see cref="Primitives.Latitude" /> and <see cref="Primitives.Longitude" /> degrees, minutes and seconds values.
		/// </summary>
		/// <param name="latitudeDegrees"><see cref="Primitives.Latitude" /> Degrees</param>
		/// <param name="latitudeMinutes"><see cref="Primitives.Latitude" /> Minutes</param>
		/// <param name="latitudeSeconds"><see cref="Primitives.Latitude" /> Seconds</param>
		/// <param name="longitudeDegrees"><see cref="Primitives.Longitude" /> Degrees</param>
		/// <param name="longitudeMinutes"><see cref="Primitives.Longitude" /> Minutes</param>
		/// <param name="longitudeSeconds"><see cref="Primitives.Longitude" /> Seconds</param>
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
		/// Determines whether two <see cref="CoordinatePoint" /> objects refer to the same location.
		/// </summary>
		/// <param name="left">The first <see cref="CoordinatePoint" /> to compare.</param>
		/// <param name="right">The second <see cref="CoordinatePoint" /> to compare.</param>
		/// <returns>true, if the <see cref="CoordinatePoint" /> objects are determined to be equivalent; otherwise, false.</returns>
		public static bool operator ==(CoordinatePoint left, CoordinatePoint right)
		{
			return (object)left != null && (object)right != null && left.Equals(right);
		}

		/// <summary>Determines whether two <see cref="CoordinatePoint" /> objects correspond to different locations.</summary>
		/// <param name="left">The first <see cref="CoordinatePoint" /> to compare.</param>
		/// <param name="right">The second <see cref="CoordinatePoint" /> to compare.</param>
		/// <returns>true, if the <see cref="CoordinatePoint" /> objects are determined to be different; otherwise, false.</returns>
		public static bool operator !=(CoordinatePoint left, CoordinatePoint right)
		{
			return !(left == right);
		}


		/// <summary>
		/// Determines if a specified <see cref="CoordinatePoint" /> 
		/// is equal to the current <see cref="CoordinatePoint" />.
		/// </summary>
		/// <param name="obj">The object to compare the <see cref="CoordinatePoint" /> to.</param>
		/// <returns>True, if the <see cref="CoordinatePoint" /> objects are equal; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			var item = obj as CoordinatePoint;

			return item != null && Equals(item);
		}

		public override int GetHashCode()
		{
			return (Latitude?.GetHashCode() ?? 0) ^ (Longitude?.GetHashCode() ?? 0);
		}
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0},{1}", Latitude.ToDegrees(), Longitude.ToDegrees());
		}

		#region Direct and inverse problem on the flat

		/// <summary>
		/// Get <see cref="CoordinatePoint" /> at distance and direction on the flat.
		/// </summary>
		/// <param name="basePoint">Source <see cref="CoordinatePoint"/></param>
		/// <param name="distance">Distance to a new point.</param>
		/// <param name="azimuth">Direction from one point to another in radians.</param>
		/// <returns><see cref="CoordinatePoint"/> at specified distance and direction from the given point.</returns>
		public static CoordinatePoint GetCoordinatePointAtDistanceAndDirectionOnTheFlat(CoordinatePoint basePoint, LinearDimension distance, double azimuth)
		{
			var deltaLatitudeGrad = distance.GetMeters() * Math.Cos(azimuth) / (_earthWgs84MeanRadius * _d2R);
			var deltaLongitudeGrad = distance.GetMeters() * Math.Sin(azimuth) / (_earthWgs84MeanRadius * _d2R);

			return new CoordinatePoint(basePoint.Latitude.ToDegrees() + deltaLatitudeGrad, basePoint.Longitude.ToDegrees() + deltaLongitudeGrad);
		}

		/// <summary>
		/// Returns the distance between two <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> on the flat.
		/// </summary>
		/// <param name="lat1">First point <see cref="SonarLogAPI.Primitives.Latitude" /> degrees double value.</param>
		/// <param name="long1">First point <see cref="SonarLogAPI.Primitives.Longitude" /> degrees double value.</param>
		/// <param name="lat2">Second point <see cref="SonarLogAPI.Primitives.Latitude" /> degrees double value.</param>
		/// <param name="long2">Second point <see cref="SonarLogAPI.Primitives.Longitude" /> degrees double value.</param>
		/// <param name="altitude">Point altitude above surface(meters). Zero by default.</param>
		/// <returns>Distance between points</returns>	
		public static LinearDimension GetDistanceBetweenPointsOnTheFlat(double lat1, double long1, double lat2, double long2, double altitude = 0)
		{
			double deltaLatRad = (lat2 - lat1) * _d2R;
			double deltaLonRad = (long2 - long1) * _d2R;
			double deltaY = deltaLatRad * (_earthWgs84MeanRadius + altitude);
			double deltaX = deltaLonRad * (_earthWgs84MeanRadius + altitude);

			double distanceMeters = Math.Sqrt(Math.Pow(deltaX, 2d) + Math.Pow(deltaY, 2d));
			return LinearDimension.FromMeters(distanceMeters);
		}

		/// <summary>
		/// Returns the distance between two <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> on the flat.
		/// </summary>
		/// <param name="firstPoint">First point</param>
		/// <param name="secondPoint">Second point</param>
		/// <param name="altitude">Point altitude above surface. Zero by default.</param>
		/// <returns>Distance between points.</returns>	
		public static LinearDimension GetDistanceBetweenPointsOnTheFlat(CoordinatePoint firstPoint, CoordinatePoint secondPoint, double altitude = 0)
		{
			return GetDistanceBetweenPointsOnTheFlat(firstPoint.Latitude.ToDegrees(),
					firstPoint.Longitude.ToDegrees(), secondPoint.Latitude.ToDegrees(), secondPoint.Longitude.ToDegrees());
		}

		#endregion

		#region Direct and inverse problem with Haversine on sphere (great circle).

		/// <summary>
		/// Get <see cref="CoordinatePoint" /> at distance and direction on sphere with Haversine.
		/// </summary>
		/// <param name="basePoint">Source <see cref="CoordinatePoint"/>.</param>
		/// <param name="distance">Distance to a new point, meters.</param>
		/// <param name="azimuth">Azimuth to a new point, radians.</param>
		/// <param name="backAzimuth">Azimuth from a new point to base point, radians.</param>
		/// <returns><see cref="CoordinatePoint" /> at specified distance and direction from the base point.</returns>
		/// <seealso cref="http://www.movable-type.co.uk/scripts/latlong.html"/>
		public static CoordinatePoint GetCoordinatePointAtDistanceAndDirectionWithHaversine(CoordinatePoint basePoint, LinearDimension distance, double azimuth, out double backAzimuth)
		{
			var startLatitudeRadians = basePoint.Latitude.ToRadians();
			var dR = distance.GetMeters() / _earthWgs84MeanRadius;

			var finishLatitudeRadians = Math.Asin(Math.Sin(startLatitudeRadians) * Math.Cos(dR) +
							   Math.Cos(startLatitudeRadians) * Math.Sin(dR) * Math.Cos(azimuth));
			var finishLongitudeRadians = basePoint.Longitude.ToRadians() +
				Math.Atan2(Math.Sin(azimuth) * Math.Sin(dR) * Math.Cos(startLatitudeRadians), Math.Cos(dR) - Math.Sin(startLatitudeRadians) * Math.Sin(finishLatitudeRadians));

			var y = Math.Sin(finishLongitudeRadians - basePoint.Longitude.ToRadians()) * Math.Cos(finishLatitudeRadians);
			var x = Math.Cos(startLatitudeRadians) * Math.Sin(finishLatitudeRadians) -
					Math.Sin(startLatitudeRadians) * Math.Cos(finishLatitudeRadians) * Math.Cos(finishLongitudeRadians - basePoint.Longitude.ToRadians());

			backAzimuth = Math.Atan2(y, x) + Math.PI;

			return new CoordinatePoint(Latitude.FromRadians(finishLatitudeRadians), Longitude.FromRadians(finishLongitudeRadians));
		}

		/// <summary>
		/// Returns the distance between two <see cref="CoordinatePoint" /> at sphere with WGS84 Mean Radius with Haversine formula.
		/// </summary>
		/// <param name="firstPoint">First point.</param>
		/// <param name="secondPoint">Second point.</param>
		/// <param name="altitude">Point altitude above surface. Zero by default.</param>
		/// <returns>Distance between points.</returns>
		/// <seealso cref="http://en.wikipedia.org/wiki/Great-circle_distance"/>
		public static LinearDimension GetDistanceBetweenPointsWithHaversine(CoordinatePoint firstPoint, CoordinatePoint secondPoint, double altitude = 0)
		{
			return GetDistanceBetweenPointsWithHaversine(firstPoint.Latitude.ToDegrees(),
				firstPoint.Longitude.ToDegrees(), secondPoint.Latitude.ToDegrees(), secondPoint.Longitude.ToDegrees(), altitude);
		}

		/// <summary>
		/// Returns the distance between two <see cref="SonarLogAPI.Primitives.CoordinatePoint" /> at sphere with WGS84 Mean Radius with Haversine formula.
		/// </summary>
		/// <param name="lat1">First point <see cref="SonarLogAPI.Primitives.Latitude" /> degrees double value.</param>
		/// <param name="long1">First point <see cref="SonarLogAPI.Primitives.Longitude" /> degrees double value.</param>
		/// <param name="lat2">Second point <see cref="SonarLogAPI.Primitives.Latitude" /> degrees double value.</param>
		/// <param name="long2">Second point <see cref="SonarLogAPI.Primitives.Longitude" /> degrees double value.</param>
		/// <param name="altitude">Point altitude above surface(meters). Zero by default.</param>
		/// <returns>Distance between points</returns>
		/// <seealso cref="http://en.wikipedia.org/wiki/Great-circle_distance"/>
		public static LinearDimension GetDistanceBetweenPointsWithHaversine(double lat1, double long1, double lat2, double long2, double altitude = 0)
		{
			//with antipode points modification 
			//https://wikimedia.org/api/rest_v1/media/math/render/svg/c3159d773b79d31c3f5ff176a6262fabd20cdbc9
			//converts all degrees to radians
			double lat1Rad = lat1 * _d2R;
			double lat2Rad = lat2 * _d2R;
			double longDeltaRad = (long2 - long1) * _d2R;

			double y = Math.Sqrt(Math.Pow(Math.Cos(lat2Rad) * Math.Sin(longDeltaRad), 2d) + Math.Pow(Math.Cos(lat1Rad) * Math.Sin(lat2Rad) - Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(longDeltaRad), 2d));
			double x = Math.Sin(lat1Rad) * Math.Sin(lat2Rad) + Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(longDeltaRad);
			double deltaSigma = Math.Atan2(y, x);

			return LinearDimension.FromMeters(deltaSigma * (_earthWgs84MeanRadius + altitude));
		}

		#endregion

		#region Direct and inverse problems on an ellipsoid

		/// <summary>
		/// Get <see cref="CoordinatePoint" /> at distance and direction on an ellipsoid.
		/// </summary>
		/// <param name="basePoint">Source <see cref="CoordinatePoint"/>.</param>
		/// <param name="distance">Distance to a new point, meters.</param>
		/// <param name="azimuth">Azimuth to a new point, radians.</param>
		/// <param name="backAzimuth">Azimuth from a new point to base point, radians.</param>
		/// <returns><see cref="CoordinatePoint" /> at specified distance and direction from the base point.</returns>
		public static CoordinatePoint GetCoordinatePointAtDistanceAndDirectionOnAnEllipsoid(CoordinatePoint basePoint, LinearDimension distance, double azimuth, out double backAzimuth)
		{
			double deltaLatitude;
			double deltaLongitude;
			double deltaAzimuth;
			double newDeltaLatitude = 0;
			double newDeltaLongitude = 0;
			double newDeltaAzimuth = 0;

			do
			{
				deltaLatitude = newDeltaLatitude;
				deltaLongitude = newDeltaLongitude;
				deltaAzimuth = newDeltaAzimuth;

				double iterationLatitude = basePoint.Latitude.ToRadians() + deltaLatitude / 2;
				double iterationAzimuth = azimuth + deltaAzimuth / 2;

				double beta = distance.GetMeters() * Math.Cos(iterationAzimuth) / GetMeridionalForLatitude(Latitude.FromRadians(iterationLatitude));
				double sigma = distance.GetMeters() * Math.Sin(iterationAzimuth) / (GetPrimeVerticalForLatitude(Latitude.FromRadians(iterationLatitude)) * Math.Cos(iterationLatitude));
				double alpha = deltaLongitude * Math.Sin(iterationLatitude);

				newDeltaLatitude = beta * (1 + (2 * Math.Pow(sigma, 2d) + Math.Pow(alpha, 2d)) / 24);
				newDeltaLongitude = sigma * (1 + (Math.Pow(alpha, 2d) - Math.Pow(beta, 2d)) / 24);
				newDeltaAzimuth = alpha * (1 + (3 * Math.Pow(beta, 2d) + 2 * Math.Pow(sigma, 2d) - 2 * Math.Pow(alpha, 2d)) / 24);
			}
			while (Math.Abs(newDeltaLatitude - deltaLatitude) > 0.0000001d || Math.Abs(deltaLongitude - newDeltaLongitude) > 0.0000001d);

			backAzimuth = azimuth + newDeltaAzimuth + Math.PI;
			return new CoordinatePoint(Latitude.FromRadians(basePoint.Latitude.ToRadians() + newDeltaLatitude),
				Longitude.FromRadians(basePoint.Longitude.ToRadians() + newDeltaLongitude));
		}

		/// <summary>
		/// Returns the distance between two <see cref="CoordinatePoint" /> on an ellipsoid. Inverse Geodesics problem.
		/// </summary>
		/// <param name="firstPoint">First point.</param>
		/// <param name="secondPoint">Second point.</param>
		/// <param name="altitude">Point altitude above surface. Zero by default.</param>
		/// <returns>Distance between two <see cref="CoordinatePoint" /> on an ellipsoid.</returns>
		public static LinearDimension GetDistanceBetweenPointsOnAnEllipsoid(CoordinatePoint firstPoint, CoordinatePoint secondPoint, double altitude = 0)
		{
			return GetDistanceBetweenPointsOnAnEllipsoid(firstPoint.Latitude.ToDegrees(),
				firstPoint.Longitude.ToDegrees(), secondPoint.Latitude.ToDegrees(), secondPoint.Longitude.ToDegrees(), altitude);
		}

		/// <summary>
		/// Returns the distance between two <see cref="CoordinatePoint" /> on an ellipsoid. Inverse Geodesics problem.
		/// </summary>
		/// <param name="lat1">First point <see cref="Primitives.Latitude" /> degrees double value.</param>
		/// <param name="long1">First point <see cref="Primitives.Longitude" /> degrees double value.</param>
		/// <param name="lat2">Second point <see cref="Primitives.Latitude" /> degrees double value.</param>
		/// <param name="long2">Second point <see cref="Primitives.Longitude" /> degrees double value.</param>
		/// <param name="altitude">Point altitude above surface(meters). Zero by default.</param>
		/// <returns>Distance between points.</returns>
		/// <seealso cref="http://www.geogr.msu.ru/cafedra/karta/docs/GOK/gok_lecture_4.pdf"/>
		/// <remarks>This method can return two azimuths to points.</remarks>
		public static LinearDimension GetDistanceBetweenPointsOnAnEllipsoid(double lat1, double long1, double lat2, double long2, double altitude = 0)
		{
			//degree(pow) reduction formula
			double SinPow2(double angleRadians) => (1 - Math.Cos(2 * angleRadians)) / 2;

			double deltaLatitudeRadians = (lat2 - lat1) * _d2R;
			double deltaLongitudeRadians = (long2 - long1) * _d2R;
			double middleLongitudeRadians = (lat2 + lat1) * _d2R / 2;
			double meridional = GetMeridionalForLatitude(Latitude.FromRadians(middleLongitudeRadians));
			double primeVertical = GetPrimeVerticalForLatitude(Latitude.FromRadians(middleLongitudeRadians));
			double q = deltaLatitudeRadians * meridional *
				(1 - (2 * Math.Pow(deltaLongitudeRadians, 2d) + Math.Pow(deltaLongitudeRadians, 2d) * SinPow2(meridional)) / 24);
			double p = deltaLongitudeRadians * primeVertical * Math.Cos(middleLongitudeRadians) *
					   (1 + (Math.Pow(deltaLatitudeRadians, 2d) - Math.Pow(deltaLongitudeRadians, 2d) * SinPow2(middleLongitudeRadians)) / 24);

			return LinearDimension.FromMeters(Math.Sqrt(Math.Pow(q, 2d) + Math.Pow(p, 2d)));
		}

		/// <summary>
		/// Returns Meridional value for specified <see cref="Primitives.Latitude"/>.
		/// </summary>
		/// <param name="latitude"><see cref="Primitives.Latitude"/>.</param>
		/// <returns>Meridional value for specified <see cref="Primitives.Latitude"/>.</returns>
		/// <seealso cref="http://en.wikipedia.org/wiki/Earth_radius#Meridional"/>
		private static double GetMeridionalForLatitude(Latitude latitude)
		{
			return Math.Pow(_earthWgs84EquatorialRadius * _earthWgs84PolarRadius, 2d)
				/ Math.Pow(Math.Pow(_earthWgs84EquatorialRadius * Math.Cos(latitude.ToRadians()), 2d) + Math.Pow(_earthWgs84PolarRadius * Math.Sin(latitude.ToRadians()), 2d), 3d / 2d);
		}

		/// <summary>
		/// Returns Prime vertical value for specified <see cref="Primitives.Latitude"/>.
		/// </summary>
		/// <param name="latitude"><see cref="Primitives.Latitude"/>.</param>
		/// <returns>Prime vertical value for specified <see cref="Primitives.Latitude"/>.</returns>
		/// <seealso cref="http://en.wikipedia.org/wiki/Earth_radius#Prime_vertical"/>
		private static double GetPrimeVerticalForLatitude(Latitude latitude)
		{
			return Math.Pow(_earthWgs84EquatorialRadius, 2d)
				/ Math.Sqrt(Math.Pow(_earthWgs84EquatorialRadius * Math.Cos(latitude.ToRadians()), 2d) + Math.Pow(_earthWgs84PolarRadius * Math.Sin(latitude.ToRadians()), 2d));
		}
		#endregion
	}
}