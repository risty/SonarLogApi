namespace SonarLogAPI.Primitives
{
	public interface ITemperaturePointSource : ICoordinatePointSource
	{
		float Temperature { get; set; }
	}
}