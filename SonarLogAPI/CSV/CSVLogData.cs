namespace SonarLogAPI.CSV
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	using SonarLogAPI.Primitives;

	/// <summary>
	///Log data that consists of CSV entries.
	/// </summary>
	public class CsvLogData
	{
		/// <summary>
		/// Data Name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Data creation time.
		/// </summary>
		public DateTimeOffset CreationDateTime { get; set; }

		/// <summary>
		/// Depth points.
		/// </summary>
		public IEnumerable<IDepthPointSource> Points { get; set; }

		/// <summary>
		/// Writes <see cref="CsvLogData"/> object to <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/>.</param>
		/// <param name="data"><see cref="CsvLogData"/> object.</param>
		public static void WriteToStream(Stream stream, CsvLogData data)
		{
			using (var writer = new StreamWriter(stream))
			{
				foreach (var point in data.Points)
				{
					writer.WriteLine(point);
				}
			}
		}
	}
}