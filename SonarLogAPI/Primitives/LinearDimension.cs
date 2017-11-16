namespace SonarLogAPI.Primitives
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Linear dimension unit.
	/// </summary>
	public enum LinearDimensionUnit : byte
	{
		Meter,

		/// <summary>
		/// US/UK foot value
		/// </summary>
		Foot
	}

	/// <summary>
	/// Linear dimension.
	/// </summary>
	public class LinearDimension : IEquatable<LinearDimension>
	{
		//https://en.wikipedia.org/wiki/United_States_customary_units
		private const double _metersInOneFoot = 0.3048;

		/// <summary>
		/// LinearDimension value.
		/// </summary>
		public double Value;

		/// <summary>
		/// LinearDimension value unit.
		/// </summary>
		public LinearDimensionUnit Unit;

		/// <summary>
		/// Create instance of <see cref="LinearDimension"/>.
		/// </summary>
		/// <param name="value"><see cref="LinearDimension"/> value.</param>
		/// <param name="unit"><see cref="LinearDimension"/> unit.</param>
		public LinearDimension(double value, LinearDimensionUnit unit)
		{
			Value = value;
			Unit = unit;
		}

		/// <summary>
		/// Get <see cref="LinearDimension"/> value in meters.
		/// </summary>
		/// <param name="depth"><see cref="LinearDimension"/>.</param>
		/// <returns><see cref="LinearDimension"/> value in meters.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static double GetMeters(LinearDimension depth)
		{
			if (depth == null) throw new ArgumentNullException(nameof(depth));

			return depth.Unit == LinearDimensionUnit.Meter
				? depth.Value
				: depth.Value * _metersInOneFoot;
		}

		/// <summary>
		/// Get <see cref="LinearDimension"/> value in foots.
		/// </summary>
		/// <param name="depth"><see cref="LinearDimension"/>.</param>
		/// <returns><see cref="LinearDimension"/> value in foots.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static double GetFoots(LinearDimension depth)
		{
			if (depth == null) throw new ArgumentNullException(nameof(depth));

			return depth.Unit == LinearDimensionUnit.Foot
				? depth.Value
				: depth.Value / _metersInOneFoot;
		}

		/// <summary>
		/// Get <see cref="LinearDimension"/> value in foots.
		/// </summary>
		/// <returns><see cref="LinearDimension"/> value in foots.</returns>
		public double GetFoots()
		{
			return GetFoots(this);
		}

		/// <summary>
		/// Get <see cref="LinearDimension"/> value in meters.
		/// </summary>
		/// <returns><see cref="LinearDimension"/>value in meters.</returns>
		public double GetMeters()
		{
			return GetMeters(this);
		}

		/// <summary>
		/// Convert <see cref="LinearDimension"/> object to depth object with value in meters.
		/// </summary>
		/// <param name="depth"><see cref="LinearDimension"/> object.</param>
		/// <returns><see cref="LinearDimension"/> object with value in meters.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static LinearDimension ToMeters(LinearDimension depth)
		{
			if (depth == null) throw new ArgumentNullException(nameof(depth));

			return depth.Unit == LinearDimensionUnit.Meter
				? depth
				: new LinearDimension(GetMeters(depth), LinearDimensionUnit.Meter);
		}

		/// <summary>
		/// Create <see cref="LinearDimension"/> object from value in meters
		/// </summary>
		/// <param name="meters">Value in meters</param>
		/// <returns><see cref="LinearDimension"/> object</returns>
		public static LinearDimension FromMeters(double meters)
		{
			return new LinearDimension(meters, LinearDimensionUnit.Meter);
		}

		/// <summary>
		/// Create <see cref="LinearDimension"/> object from value in foots
		/// </summary>
		/// <param name="foots">Value in foots</param>
		/// <returns><see cref="LinearDimension"/> object</returns>
		public static LinearDimension FromFoots(double foots)
		{
			return new LinearDimension(foots, LinearDimensionUnit.Foot);
		}


		/// <summary>
		/// Converts the string representation of LinearDimension value to <see cref="LinearDimension"/> object
		/// </summary>
		/// <param name="stringvalue">String representation of <see cref="LinearDimension"/> value</param>
		/// <param name="depthUnit"><see cref="LinearDimension"/> unit</param>
		/// <param name="depth">Conversion successed or failed</param>
		/// <returns></returns>
		public static bool TryParse(string stringvalue, LinearDimensionUnit depthUnit, out LinearDimension depth)
		{
			var isSuccessParse = double.TryParse(stringvalue, out var result);

			depth = null;
			if (isSuccessParse)
				depth = new LinearDimension(result, depthUnit);

			return isSuccessParse;
		}

		public override bool Equals(object obj)
		{
			var item = obj as LinearDimension;

			return item != null && Equals(item);
		}

		/// <summary>
		/// Determines if a specified <see cref="LinearDimension" /> 
		/// is equal to the current <see cref="LinearDimension" />.
		/// </summary>
		/// <param name="other">The object to compare the <see cref="LinearDimension" /> to.</param>
		/// <returns>True, if the <see cref="LinearDimension" /> objects are equal; otherwise, false.</returns>
		public bool Equals(LinearDimension other)
		{
			//Check whether the compared object is null. 
			if (ReferenceEquals(other, null)) return false;

			//Check whether the compared object references the same data. 
			if (ReferenceEquals(this, other)) return true;

			return GetMeters().Equals(other.GetMeters());
		}

		public override int GetHashCode()
		{
			return GetMeters().GetHashCode();
		}

		public static bool operator ==(LinearDimension left, LinearDimension right)
		{
			return ReferenceEquals(left, null) && ReferenceEquals(right, null) || !ReferenceEquals(left, null) && left.Equals(right);
		}

		public static bool operator !=(LinearDimension left, LinearDimension right)
		{
			return !(left == right);
		}

		public static bool operator <(LinearDimension left, LinearDimension right)
		{
			return left.GetMeters() < right.GetMeters();
		}

		public static bool operator >(LinearDimension left, LinearDimension right)
		{
			return left.GetMeters() > right.GetMeters();
		}

		public static LinearDimension operator +(LinearDimension left, LinearDimension right)
		{
			return new LinearDimension(left.GetMeters() + right.GetMeters(), LinearDimensionUnit.Meter);
		}

		public static LinearDimension operator -(LinearDimension left, LinearDimension right)
		{
			return new LinearDimension(left.GetMeters() - right.GetMeters(), LinearDimensionUnit.Meter);
		}

		public static LinearDimension operator *(LinearDimension left, double right)
		{
			return new LinearDimension(left.GetMeters() * right, LinearDimensionUnit.Meter);
		}

		public static LinearDimension operator /(LinearDimension left, double right)
		{
			return new LinearDimension(left.GetMeters() / right, LinearDimensionUnit.Meter);
		}

		public static double operator /(LinearDimension left, LinearDimension right)
		{
			return left.GetMeters() / right.GetMeters();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:0.000 meters}", GetMeters());
			//return $"{GetMeters():#.###}";
		}
	}
}