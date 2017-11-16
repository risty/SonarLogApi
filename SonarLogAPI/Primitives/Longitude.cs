namespace SonarLogAPI.Primitives
{
	using System;

	/// <summary>
	/// Longitude position. West or East.
	/// </summary>
	public enum LongitudePosition : byte
	{
		West, East
	}

	/// <summary>
	/// Longitude coordinate. Between −180°(West) and +180°(East).
	/// </summary>
	public class Longitude : Coordinate, IEquatable<Longitude>
	{
		/// <summary>
		/// Position of Longitude.
		/// </summary>
		public LongitudePosition Position { get; set; }

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SonarLogAPI.Primitives.Longitude" /> class from longitude data.
		/// </summary>
		/// <param name="degrees">The longitude of the location in deegrees. May range from -180.0 to 180.0.</param>
		/// <exception cref="ArgumentOutOfRangeException" />
		public Longitude(double degrees)
			: base(degrees)
		{
			if (degrees < -180 || degrees > 180)
				throw new ArgumentOutOfRangeException(nameof(degrees), "can be between -180 and 180");

			PositionSet(degrees);
		}

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SonarLogAPI.Primitives.Longitude" /> class from longitude data.
		/// </summary>
		/// <param name="degrees">The longitude of the location in deegrees. May range from -180.0 to 180.0.</param>
		/// <param name="minutes">Minutes part of latitude. May range from 0 to 60.0.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException" />
		public Longitude(double degrees, double minutes)
			: base(degrees, minutes)
		{
			PositionSet(degrees);
		}

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="Longitude" /> class from longitude data.
		/// </summary>
		/// <param name="degrees">The <see cref="Longitude" /> of the location in deegrees. May range from -180.0 to 180.0.</param>
		/// <param name="minutes">Minutes part of <see cref="Longitude" />. May range from 0 to 60.0.</param>
		/// <param name="seconds">Seconds part of <see cref="Longitude" />. May range from 0 to 60.0.</param>
		public Longitude(double degrees, double minutes, double seconds)
			: base(degrees, minutes, seconds)
		{
			PositionSet(degrees);
		}

		/// <summary>
		/// Returns <see cref="Longitude" /> coordinate with Position, converted to Degrees double value.
		/// </summary>
		/// <param name="degrees">Longitude degrees.</param>
		/// <param name="minutes">Longitude minutes.</param>
		/// <param name="seconds">Longitude seconds.</param>
		/// <param name="position">Longitude position.</param>
		/// <returns>Degrees double value.</returns>
		public static double ToDegrees(double degrees, double minutes, double seconds, LongitudePosition position)
		{
			var toDouble = Math.Abs(degrees) + minutes / 60 + seconds / 3600;
			return position == LongitudePosition.East
				? toDouble
				: toDouble * -1;
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns <see cref="Longitude" /> coordinate, converted to Degrees double value.
		/// </summary>
		/// <returns>Degrees double value.</returns>
		public override double ToDegrees()
		{
			return Position == LongitudePosition.East
				? base.ToDegrees()
				: base.ToDegrees() * -1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Longitude"/> class from degrees value.
		/// </summary>
		/// <param name="degreesValue">Degrees value.</param>
		/// <returns>Instance of the <see cref="Longitude"/>.</returns>
		public static Longitude FromDegrees(double degreesValue)
		{
			return new Longitude(degreesValue);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Longitude"/> class from radians value.
		/// </summary>
		/// <param name="radiansValue">Radians value.</param>
		/// <returns>Instance of the <see cref="Longitude"/>.</returns>
		public static Longitude FromRadians(double radiansValue)
		{
			return new Longitude(radiansValue * 180d / Math.PI);
		}

		/// <summary>
		/// Converts the string representation of Longitude degrees value to <see cref="Longitude" /> object.
		/// </summary>
		/// <param name="stringvalue">String representation of Longitude degrees value.</param>
		/// <param name="latitude">Longitude object.</param>
		/// <returns>Conversion successed or failed.</returns>
		public static bool TryParse(string stringvalue, out Longitude latitude)
		{
			var isSuccessParse = double.TryParse(stringvalue, out var result);

			latitude = null;
			if (isSuccessParse)
				latitude = new Longitude(result);

			return isSuccessParse;
		}

		public override string ToString()
		{
			return $"{base.ToString()} {Position}";
		}

		public bool Equals(Longitude other)
		{
			return other != null && base.Equals(other) && Position == other.Position;
		}

		public override bool Equals(object obj)
		{
			return obj is Longitude item && Equals(item);
		}

		public override int GetHashCode()
		{
			return ToDegrees().GetHashCode();
		}

		private void PositionSet(double degrees)
		{
			if (degrees > 0) Position = LongitudePosition.East;
			if (degrees < 0) Position = LongitudePosition.West;
		}
	}
}