namespace SonarLogAPI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using SonarLogAPI.Primitives;

	/// <summary>
	/// Depth adjuster.
	/// </summary>
	public class DepthAdjuster
	{
		private readonly object _syncRoot = new object();

		/// <summary>
		/// Base points sequence.
		/// </summary>
		public IEnumerable<IDepthPointSource> BasePoints { get; set; }

		/// <summary>
		/// Adjustable points sequence.
		/// </summary>
		public IEnumerable<IDepthPointSource> AdjustablePoints { get; set; }

		/// <summary>
		/// Create instance of <see cref="DepthAdjuster"/>.
		/// </summary>
		/// <param name="basePoints">Base points sequence.</param>
		/// <param name="adjustablePoints">Adjustable points sequence.</param>
		public DepthAdjuster(IEnumerable<IDepthPointSource> basePoints, IEnumerable<IDepthPointSource> adjustablePoints)
		{
			BasePoints = basePoints;
			AdjustablePoints = adjustablePoints;
		}

		/// <summary>
		/// Ajust depth at <see cref="AdjustablePoints"/>.
		/// </summary>
		/// <returns><see cref="AdjustablePoints"/> after depth adjust.</returns>
		public IEnumerable<IDepthPointSource> AdjustDepth()
		{
			var nearestPoint = FindNearestPoint(BasePoints, AdjustablePoints);
			return AdjustDepth(AdjustablePoints, nearestPoint.FirstPoint.Depth - nearestPoint.SecondPoint.Depth);
		}

		/// <summary>
		/// Ajust depth at <see cref="AdjustablePoints"/> async.
		/// </summary>
		/// <returns><see cref="AdjustablePoints"/> after depth adjust.</returns>
		public Task<IEnumerable<IDepthPointSource>> AdjustDepthAsync()
		{
			return Task.Run(() => AdjustDepth());
		}

		/// <summary>
		/// Find nearest points at two sequence.
		/// </summary>
		/// <param name="firstSequence">First points sequence.</param>
		/// <param name="secondSequence">Second points sequence.</param>
		/// <returns><see cref="NearestPointsEventArgs"/>.</returns>
		private NearestPointsEventArgs FindNearestPoint(IEnumerable<IDepthPointSource> firstSequence, IEnumerable<IDepthPointSource> secondSequence)
		{
			var uniqueBasePoints = firstSequence.GroupBy(point => point.Point)
				.Select(g => g.First())
				.ToList();
			var uniqueAdjustablePoints = secondSequence.GroupBy(point => point.Point)
				.Select(g => g.First())
				.ToList();

			var distance = new LinearDimension(double.MaxValue, LinearDimensionUnit.Meter);
			Tuple<IDepthPointSource, IDepthPointSource> points = null;

			Parallel.ForEach(uniqueBasePoints,
			   uniqueBasePoint =>
			   {
				   foreach (var uniqueAdjustablePoint in uniqueAdjustablePoints)
				   {
					   var dim = CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(uniqueBasePoint.Point, uniqueAdjustablePoint.Point);
					   if (dim < distance)
					   {
						   lock (_syncRoot)
						   {
							   distance = dim;
							   points = new Tuple<IDepthPointSource, IDepthPointSource>(uniqueBasePoint, uniqueAdjustablePoint);
						   }
					   }
				   }
			   }
			);

			//foreach (var uniqueBasePoint in uniqueBasePoints)
			//{
			//	foreach (var uniqueAdjustablePoint in uniqueAdjustablePoints)
			//	{
			//		var dim = CoordinatePoint.DistanceBetweenPoints(uniqueBasePoint.Point, uniqueAdjustablePoint.Point);
			//		if (dim < distance)
			//		{
			//			distance = dim;
			//			points = new Tuple<IDepthPointSource, IDepthPointSource>(uniqueBasePoint, uniqueAdjustablePoint);
			//		}
			//	}
			//}

			var e = new NearestPointsEventArgs()
			{
				Distance = distance,
				FirstPoint = points.Item1,
				SecondPoint = points.Item2
			};

			OnNearestPointFound(e);

			return e;
		}

		/// <summary>
		/// Ajust depth at sequence of points.
		/// </summary>
		/// <param name="innerSequence">Sequence of points to adjust depth.</param>
		/// <param name="ajustValue">Value to add to depth.</param>
		/// <returns>Sequence of points after depth adjust.</returns>
		private static IEnumerable<IDepthPointSource> AdjustDepth(IEnumerable<IDepthPointSource> innerSequence, LinearDimension ajustValue)
		{
			var depthPointSources = innerSequence as IDepthPointSource[] ?? innerSequence.ToArray();

			foreach (var depthPointSource in depthPointSources)
			{
				depthPointSource.Depth += ajustValue;
			}

			return depthPointSources;
		}

		public event EventHandler<NearestPointsEventArgs> NearestPointsFound;

		protected virtual void OnNearestPointFound(NearestPointsEventArgs e)
		{
			NearestPointsFound?.Invoke(this, e);
		}

		/// <summary>
		/// EventArgs with information about two nearest points and distance between em.
		/// </summary>
		public class NearestPointsEventArgs : EventArgs
		{
			public IDepthPointSource FirstPoint;
			public IDepthPointSource SecondPoint;
			public LinearDimension Distance;
		}
	}
}