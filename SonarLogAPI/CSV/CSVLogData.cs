namespace SonarLogAPI.CSV
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

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
			if (!stream.CanWrite)
				throw new InvalidOperationException("Can't write to stream");

			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			{
				foreach (var point in data.Points)
				{
					writer.WriteLine(point);
				}
			}
		}

		/// <summary>
		/// Reads <see cref="CsvLogData"/> from <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/>.</param>
		/// <returns><see cref="CsvLogData"/> object.</returns>
		public static CsvLogData ReadFromStream(Stream stream)
		{
			if (!stream.CanRead)
				throw new InvalidOperationException("Can't read from stream");

			var listofEntrys = new List<CsvLogEntry>();
			var valuesOrder = new Dictionary<int, string> { { 0, "Latitude" }, { 1, "Longitude" }, { 2, "Depth" } };

			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				while (!reader.EndOfStream)
				{
					if (CsvLogEntry.TryParse(reader.ReadLine(), ',', LinearDimensionUnit.Meter, valuesOrder, out var result))
					listofEntrys.Add(result);
				}
			}

			return new CsvLogData()
			{
				CreationDateTime = DateTimeOffset.Now,
				Points = listofEntrys,
				Name = $"{listofEntrys.Count} points with depths from {listofEntrys.Select(entry=> entry.Depth.GetMeters()).Min()} to {listofEntrys.Select(entry => entry.Depth.GetMeters()).Max()}."
			};
		}
	}
}