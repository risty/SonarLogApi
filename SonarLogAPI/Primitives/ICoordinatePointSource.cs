namespace SonarLogAPI.Primitives
{
	/// <summary>
	/// Interface for source of geographical location point
	/// </summary>
	public interface ICoordinatePointSource
	{
		/// <summary>
		/// Represents a geographical location point that is determined by latitude and longitude coordinates.
		/// </summary>
		CoordinatePoint Point { get; set; }
	}
}