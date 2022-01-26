namespace SonarLogAPI.Primitives
{
	/// <inheritdoc />
	/// <summary>
	/// Interface for source of geographical location point with water depth value.
	/// </summary>
	public interface IDepthPointSource : ICoordinatePointSource
	{
		/// <summary>
		/// Water depth
		/// </summary>
		LinearDimension Depth { get; set; }
	}
}