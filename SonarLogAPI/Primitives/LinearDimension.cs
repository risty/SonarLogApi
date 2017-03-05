namespace SonarLogAPI.Primitives
{
	using System;
	using System.Globalization;

	public enum LinearDimensionUnit : byte
	{
		Meter,
		
		/// <summary>
		/// US/UK foot value
		/// </summary>
		Foot
	}

	/// <summary>
	/// LinearDimension
	/// </summary>
	public class LinearDimension : IEquatable<LinearDimension>
	{
		//https://en.wikipedia.org/wiki/United_States_customary_units
		private const double _metersInOneFoot = 0.3048;

		/// <summary>
		/// LinearDimension value
		/// </summary>
		public double Value;

		/// <summary>
		/// LinearDimension value unit
		/// </summary>
		public LinearDimensionUnit Unit;

		/// <summary>
		/// Create instance of <see cref="LinearDimension"/>
		/// </summary>
		/// <param name="value"><see cref="LinearDimension"/> value</param>
		/// <param name="unit"><see cref="LinearDimension"/> unit</param>
		public LinearDimension(double value, LinearDimensionUnit unit )
		{
			Value = value;
			Unit = unit;
		}

		/// <summary>
		/// Get <see cref="LinearDimension"/> value in meters
		/// </summary>
		/// <param name="depth"><see cref="LinearDimension"/></param>
		/// <returns><see cref="LinearDimension"/> value in meters</returns>
		public static double GetMeters(LinearDimension depth)
		{
			return depth.Unit == LinearDimensionUnit.Meter
				? depth.Value 
				: depth.Value * _metersInOneFoot;
		}

		/// <summary>
		/// Get <see cref="LinearDimension"/> value in foots
		/// </summary>
		/// <param name="depth"><see cref="LinearDimension"/></param>
		/// <returns><see cref="LinearDimension"/> value in foots</returns>
		public static double GetFoots(LinearDimension depth)
		{
			return depth.Unit == LinearDimensionUnit.Foot
				? depth.Value
				: depth.Value / _metersInOneFoot;
		}

		/// <summary>
		/// Get <see cref="LinearDimension"/> value in foots
		/// </summary>
		/// <returns><see cref="LinearDimension"/> value in foots</returns>
		public double GetFoots()
		{
			return GetFoots(this);
		}

		/// <summary>
		/// Get <see cref="LinearDimension"/> value in meters
		/// </summary>
		/// <returns><see cref="LinearDimension"/>value in meters</returns>
		public double GetMeters()
		{
			return GetMeters(this);
		}

		/// <summary>
		/// Convert <see cref="LinearDimension"/> object to depth object with value in meters
		/// </summary>
		/// <param name="depth"><see cref="LinearDimension"/> object</param>
		/// <returns><see cref="LinearDimension"/> object with value in meters</returns>
		public static LinearDimension ToMeters(LinearDimension depth)
		{
			return depth.Unit == LinearDimensionUnit.Meter 
				? depth 
				: new LinearDimension(GetMeters(depth),LinearDimensionUnit.Meter);
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
			double result;
			var isSuccessParse = double.TryParse(stringvalue, out result);

			depth = null;
			if (isSuccessParse)
				depth = new LinearDimension(result, depthUnit);

			return isSuccessParse;
		}

		public bool Equals(LinearDimension other)
		{
			return other != null && GetMeters().Equals(other.GetMeters());
		}

		public static bool operator ==(LinearDimension left, LinearDimension right)
		{
			return (object)left != null && (object)right != null && left.Equals(right);
		}

		public static bool operator !=(LinearDimension left, LinearDimension right)
		{
			return !(left == right);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:0.000 meters}", GetMeters());
			//return $"{GetMeters():#.###}";
		}
	}
}