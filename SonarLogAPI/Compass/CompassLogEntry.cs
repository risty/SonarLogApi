namespace SonarLogAPI.Compass
{
	using System;
	using Primitives;

	/// <summary>
	/// The log entry with information from compass.
	/// </summary>
	/// <seealso cref="http://en.wikipedia.org/wiki/Flight_dynamics_(fixed-wing_aircraft)"/>
	public class CompassLogEntry : ICoordinatePointSource
	{
		/// <summary>
		/// Entry DateTimeOffset.
		/// </summary>
		public DateTimeOffset DateTimeOffset { get; set; }

		/// <inheritdoc />
		public CoordinatePoint Point { get; set; }

		/// <summary>
		/// Heading (magnetic azimuth).
		/// </summary>
		public double Heading { get; set; }

		/// <summary>
		/// Pitch in radians. "-" front side turn and "+" back side turn.
		/// </summary>
		public double Pitch { get; set; }

		/// <summary>
		/// Roll in radians. "+" for right side turn and "-" left side turn.
		/// </summary>
		public double Roll { get; set; }
	}
}
