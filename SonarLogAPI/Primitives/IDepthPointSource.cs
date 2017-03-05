namespace SonarLogAPI.Primitives
{

	public interface IDepthPointSource : ICoordinatePointSource
	{
		/// <summary>
		/// Water depth
		/// </summary>
		LinearDimension Depth { get; set; }
	}
}