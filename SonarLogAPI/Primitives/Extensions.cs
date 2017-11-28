using System.Collections.Generic;
using System.Linq;

using SonarLogAPI.CSV;

namespace SonarLogAPI.Primitives
{
	

	public static class Extensions
	{

		/// <summary>
		/// Get unique depth points from sequence.
		/// </summary>
		/// <param name="inputDepthPoints">Input sequence of <see cref="IDepthPointSource"/></param>
		/// <returns>Sequence of unique <see cref="IDepthPointSource"/></returns>
		public static IEnumerable<IDepthPointSource> GetUniqueDepthPoints(this IEnumerable<IDepthPointSource> inputDepthPoints)
		{
			return inputDepthPoints
				.GroupBy(point => point.Point)
				.Select(depthPointSourceGroup =>
					new CsvLogEntry(depthPointSourceGroup.Key,
						LinearDimension.FromMeters(depthPointSourceGroup.Average(pnt => pnt.Depth.GetMeters()))));
		}

	}
}
