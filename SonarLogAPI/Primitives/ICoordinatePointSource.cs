namespace SonarLogAPI.Primitives
{

	public interface ICoordinatePointSource
	{
		/// <summary>
		/// Represents a geographical location point that is determined by latitude and longitude coordinates.
		/// </summary>
		CoordinatePoint Point { get; set; }
	}
}