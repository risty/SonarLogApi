namespace SonarLogAPI.Primitives
{
	/// <inheritdoc />
	/// <summary>
	/// Interface for source of geographical location point with temperature value.
	/// </summary>
	public interface ITemperaturePointSource : ICoordinatePointSource
	{
		/// <summary>
		/// Temperature
		/// </summary>
		float Temperature { get; set; }
	}
}