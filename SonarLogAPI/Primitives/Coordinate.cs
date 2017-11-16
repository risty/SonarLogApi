//based on http://stackoverflow.com/questions/4504956/formatting-double-to-latitude-longitude-human-readable-format

namespace SonarLogAPI.Primitives
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Abstract class, representing geographical <see cref="Coordinate" />.
	/// </summary>
	public abstract class Coordinate : IEquatable<Coordinate>
	{
		private double _minutes;
		private double _seconds;

		/// <summary>
		/// Degrees part of geographical <see cref="Coordinate" />.
		/// </summary>
		public double Degrees { get; set; }

		/// <summary>
		/// Minutes part of geographical <see cref="Coordinate" />.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public double Minutes
		{
			get => _minutes;
			set
			{
				if (value < 0 || value > 60)
					throw new ArgumentOutOfRangeException(nameof(Minutes), "can be between 0 and 60");

				_minutes = value;
			}
		}

		/// <summary>
		/// Seconds part of geographical <see cref="Coordinate" />.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public double Seconds
		{
			get => _seconds;
			set
			{
				if (value < 0 || value > 60)
					throw new ArgumentOutOfRangeException(nameof(Seconds), "can be between 0 and 60");

				_seconds = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Coordinate" /> class from <see cref="Degrees" /> value.
		/// </summary>
		/// <param name="value"><see cref="Degrees" /> value.</param>
		protected Coordinate(double value)
		{
			var absValue = Math.Abs(value);

			Degrees = FromDoubleToIntAndFractionIn60ThSystem(absValue, out var fraction);
			Minutes = FromDoubleToIntAndFractionIn60ThSystem(fraction, out fraction);
			Seconds = fraction;

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Coordinate" /> class from <see cref="Degrees" /> and <see cref="Minutes" /> value.
		/// </summary>
		/// <param name="degrees"><see cref="Degrees" /> value.</param>
		/// <param name="minutes"><see cref="Minutes" /> value.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		protected Coordinate(double degrees, double minutes)
		{
			Degrees = Math.Abs(degrees);
			if (minutes < 0)
				throw new ArgumentOutOfRangeException(nameof(Minutes), "can't be less zero");

			Minutes = FromDoubleToIntAndFractionIn60ThSystem(minutes, out var fraction);
			Seconds = fraction;

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Coordinate" /> class from <see cref="Degrees" />, <see cref="Minutes" /> and <see cref="Seconds" /> value.
		/// </summary>
		/// <param name="degrees"><see cref="Degrees" /> value.</param>
		/// <param name="minutes"><see cref="Minutes" /> value.</param>
		/// <param name="seconds"><see cref="Seconds" /></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		protected Coordinate(double degrees, double minutes, double seconds)
		{
			Degrees = Math.Abs(degrees);

			if (minutes < 0)
				throw new ArgumentOutOfRangeException(nameof(Minutes), "can't be less zero");

			Minutes = minutes;

			if (seconds < 0)
				throw new ArgumentOutOfRangeException(nameof(Seconds), "can't be less zero");

			Seconds = seconds;
		}

		/// <summary>
		/// Split double value to integer part and fraction part, converted to 60 decimal calculus.
		/// </summary>
		/// <param name="value">Double value of Degrees or Minutes.</param>
		/// <param name="fraction">Fraction part, converted to 60 decimal calculus.</param>
		/// <returns>Integer part if value</returns>
		private static int FromDoubleToIntAndFractionIn60ThSystem(double value, out double fraction)
		{
			var integerPart = (int)value;
			fraction = (value - integerPart) * 60;
			return integerPart;
		}

		/// <summary>
		/// Represent coordinate as degrees double value.
		/// </summary>
		/// <returns>Degrees double value.</returns>
		public virtual double ToDegrees()
		{
			return Degrees + Minutes / 60 + Seconds / 3600;
		}

		/// <summary>
		/// Represent coordinate as radians double value.
		/// </summary>
		/// <returns>Radians double value.</returns>
		public double ToRadians()
		{
			return ToDegrees() * Math.PI / 180d;
		}

		public bool Equals(Coordinate other)
		{
			//Check whether the compared object is null. 
			if (ReferenceEquals(other, null)) return false;

			//Check whether the compared object references the same data. 
			if (ReferenceEquals(this, other)) return true;

			//Check whether the products' properties are equal. 
			return Degrees.Equals(other.Degrees)
				&& Minutes.Equals(other.Minutes) && Seconds.Equals(other.Seconds);
		}

		public override bool Equals(object obj)
		{
			return obj is Coordinate item && Equals(item);
		}

		public override int GetHashCode()
		{
			return ToDegrees().GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}º {1}' {2}\"", Degrees, Minutes, Seconds);
			//return $"{Degrees}º {Minutes}' {Seconds}\"";
		}
	}

}
