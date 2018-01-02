namespace SonarLogAPI.Primitives
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Latitude position. South or North.
	/// </summary>
	public enum LatitudePosition : byte
	{
		South, North
	}

	/// <summary>
	/// <see cref="Latitude" /> coordinate. Between −90°(South/low) and +90°(North/high)
	/// </summary>
	public class Latitude : Coordinate, IEquatable<Latitude>
	{
		/// <summary>
		/// Position of Latitude 
		/// </summary>
		public LatitudePosition Position { get; set; }

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="Latitude" /> class from latitude data.
		/// </summary>
		/// <param name="degrees">The latitude of the location in degrees. May range from -90.0 to 90.0.</param>
		/// <exception cref="ArgumentOutOfRangeException" />
		public Latitude(double degrees) : base(degrees)
		{
			if (degrees < -90 || degrees > 90)
				throw new ArgumentOutOfRangeException(nameof(degrees), "can be between -90 and 90");

			PositionSet(degrees);
		}

		/// <inheritdoc />
		///  <summary>
		///  Initializes a new instance of the <see cref="Latitude" /> class from latitude data.
		///  </summary>
		///  <param name="degrees">Degrees part of latitude. May range from -90.0 to 90.0.</param>
		/// <param name="minutes">Minutes part of latitude. May range from 0 to 60.0.</param>
		/// <exception cref="ArgumentOutOfRangeException" />
		public Latitude(double degrees, double minutes) : base(degrees, minutes)
		{
			PositionSet(degrees);
		}

		/// <inheritdoc />
		///  <summary>
		///  Initializes a new instance of the <see cref="Latitude" /> class from latitude data.
		///  </summary>
		///  <param name="degrees">Degrees part of <see cref="Latitude" />. May range from -90.0 to 90.0.</param>
		/// <param name="minutes">Minutes part of <see cref="Latitude" />. May range from 0 to 60.0.</param>
		/// <param name="seconds">Seconds part of <see cref="Latitude" />. May range from 0 to 60.0.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException" />
		public Latitude(double degrees, double minutes, double seconds) : base(degrees, minutes, seconds)
		{
			PositionSet(degrees);
		}

		/// <summary>
		/// Returns <see cref="Latitude" /> coordinate with Position, converted to Degrees double value.
		/// </summary>
		/// <param name="degrees">Latitude degrees.</param>
		/// <param name="minutes">Latitude minutes.</param>
		/// <param name="seconds">Latitude seconds.</param>
		/// <param name="position">Latitude position.</param>
		/// <returns>Degrees double value.</returns>
		public static double ToDegrees(double degrees, double minutes, double seconds, LatitudePosition position)
		{
			var toDouble = Math.Abs(degrees) + minutes / 60 + seconds / 3600;
			return position == LatitudePosition.North
				? toDouble
				: toDouble * -1;
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns <see cref="T:SonarLogAPI.Primitives.Latitude" /> coordinate, converted to Degrees double value
		/// </summary>
		/// <returns>Degrees double value.</returns>
		public override double ToDegrees()
		{
			return Position == LatitudePosition.North
				? base.ToDegrees()
				: base.ToDegrees() * -1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Latitude" /> class from degrees value.
		/// </summary>
		/// <param name="degreesValue">Degrees value.</param>
		/// <returns>Instance of the <see cref="Latitude" />.</returns>
		public static Latitude FromDegrees(double degreesValue)
		{
			return new Latitude(degreesValue);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Latitude" /> class from radians value.
		/// </summary>
		/// <param name="radiansValue">Radians value.</param>
		/// <returns>Instance of the <see cref="Latitude" />.</returns>
		public static Latitude FromRadians(double radiansValue)
		{
			return new Latitude(radiansValue * 180d / Math.PI);
		}

		/// <summary>
		/// Converts the string representation of <see cref="Latitude" /> degrees value to Latitude object.
		/// </summary>
		/// <param name="stringvalue">String representation of Latitude degrees value.</param>
		/// <param name="latitude"><see cref="Latitude" /> object.</param>
		/// <returns>Conversion succeeded or failed.</returns>
		public static bool TryParse(string stringvalue, out Latitude latitude)
		{
			var isSuccessParse = double.TryParse(stringvalue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);

			latitude = null;
			if (isSuccessParse)
				latitude = new Latitude(result);

			return isSuccessParse;
		}

		public override string ToString()
		{
			return $"{base.ToString()} {Position}";
		}

		public bool Equals(Latitude other)
		{
			return other != null && base.Equals(other) && Position == other.Position;
		}

		public override bool Equals(object obj)
		{
			return obj is Latitude item && Equals(item);
		}

		public override int GetHashCode()
		{
			return ToDegrees().GetHashCode();
		}

		private void PositionSet(double degrees)
		{
			if (degrees > 0) Position = LatitudePosition.North;
			if (degrees < 0) Position = LatitudePosition.South;
		}

	}
}