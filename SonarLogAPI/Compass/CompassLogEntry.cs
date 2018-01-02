namespace SonarLogAPI.Compass
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	using Primitives;

	/// <summary>
	/// The log entry with information from compass.
	/// </summary>
	/// <seealso cref="http://en.wikipedia.org/wiki/Flight_dynamics_(fixed-wing_aircraft)" />
	public class CompassLogEntry : ICoordinatePointSource, IEquatable<CompassLogEntry>
	{
		/// <summary>
		/// Entry EntryDateTimeOffset.
		/// </summary>
		public DateTimeOffset EntryDateTimeOffset { get; set; }

		/// <inheritdoc />
		public CoordinatePoint Point { get; set; }

		/// <summary>
		/// Heading (magnetic azimuth), degrees.
		/// </summary>
		public double Heading { get; set; }

		/// <summary>
		/// Pitch in radians. "-" front side turn and "+" back side turn, degrees.
		/// </summary>
		public double Pitch { get; set; }

		/// <summary>
		/// Roll in radians. "+" for right side turn and "-" left side turn, degrees.
		/// </summary>
		public double Roll { get; set; }

		/// <summary>
		/// Create instance of <see cref="CompassLogEntry"/> with <see cref="EntryDateTimeOffset"/> and MRU data.
		/// </summary>
		/// <param name="entryDateTimeOffset"></param>
		/// <param name="heading"><see cref="Heading"/></param>
		/// <param name="pitch"><see cref="Pitch"/></param>
		/// <param name="roll"><see cref="Roll"/></param>
		public CompassLogEntry(DateTimeOffset entryDateTimeOffset, double heading, double pitch, double roll)
		{
			EntryDateTimeOffset = entryDateTimeOffset;
			Heading = heading;
			Pitch = pitch;
			Roll = roll;
		}

		/// <summary>
		/// Create instance of <see cref="CompassLogEntry"/> with <see cref="CoordinatePoint"/> and MRU data.
		/// </summary>
		/// <param name="point"><see cref="CoordinatePoint"/></param>
		/// <param name="heading"><see cref="Heading"/></param>
		/// <param name="pitch"><see cref="Pitch"/></param>
		/// <param name="roll"><see cref="Roll"/></param>
		public CompassLogEntry(CoordinatePoint point, double heading, double pitch, double roll)
		{
			Point = point;
			Heading = heading;
			Pitch = pitch;
			Roll = roll;
		}

		public static bool TryParse(string cvsLogEntryString, char charForSplit, IDictionary<int, string> valuesOrder, out CompassLogEntry result)
		{
			result = null;

			long dateParseResult = 0;
			Latitude lat = null;
			Longitude lon = null;
			double heading = 0;
			double pitch = 0;
			double roll = 0;
			var values = cvsLogEntryString.Split(charForSplit);

			// if string doesn't contains values or values count less then expect return false
			if (values.Length == 0 || values.Length < valuesOrder.Count)
				return false;

			for (var i = 0; i < values.Length; i++)
			{
				if (valuesOrder.ContainsKey(i))
				{
					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(valuesOrder[i],
							"Time", CompareOptions.IgnoreCase) >= 0)
					{
						var parceresult = long.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture,
							out dateParseResult);

						if (parceresult)
							continue;
						return false;
					}

					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(valuesOrder[i],
							"Latitude", CompareOptions.IgnoreCase) >= 0)
					{
						var parceresult = Latitude.TryParse(values[i], out lat);

						if (parceresult)
							continue;
						return false;
					}

					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(valuesOrder[i],
							"Longitude", CompareOptions.IgnoreCase) >= 0)
					{
						var parceresult = Longitude.TryParse(values[i], out lon);

						if (parceresult)
							continue;
						return false;
					}

					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(valuesOrder[i],
							"Heading", CompareOptions.IgnoreCase) >= 0)
					{
						var parceresult = double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture,
							out heading);

						if (parceresult)
							continue;
						return false;
					}

					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(valuesOrder[i],
							"Pitch", CompareOptions.IgnoreCase) >= 0)
					{
						var parceresult = double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture,
							out pitch);

						if (parceresult)
							continue;
						return false;
					}

					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(valuesOrder[i],
							"Roll", CompareOptions.IgnoreCase) >= 0)
					{
						var parceresult = double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture,
							out roll);

						if (parceresult)
							continue;
						return false;
					}
				}

			}

			if (lat != null && lon != null)
			{
				result = new CompassLogEntry(new CoordinatePoint(lat, lon), heading, pitch, roll);
				if (dateParseResult != 0)
					result.EntryDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(dateParseResult);

				return true;
			}

			if (dateParseResult != 0)
			{
				result = new CompassLogEntry(DateTimeOffset.FromUnixTimeMilliseconds(dateParseResult), heading, pitch, roll);
				return true;
			}

			return false;
		}


		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5}",
				EntryDateTimeOffset.ToUnixTimeMilliseconds(), Point?.Latitude.ToDegrees(), Point?.Longitude.ToDegrees(), Heading, Pitch, Roll);
		}

		public bool Equals(CompassLogEntry other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return EntryDateTimeOffset.Equals(other.EntryDateTimeOffset) 
				&& Equals(Point, other.Point) 
				&& Heading.Equals(other.Heading) 
				&& Pitch.Equals(other.Pitch) 
				&& Roll.Equals(other.Roll);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((CompassLogEntry)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = EntryDateTimeOffset.GetHashCode();
				hashCode = (hashCode * 397) ^ (Point != null ? Point.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Heading.GetHashCode();
				hashCode = (hashCode * 397) ^ Pitch.GetHashCode();
				hashCode = (hashCode * 397) ^ Roll.GetHashCode();
				return hashCode;
			}
		}
	}
}
