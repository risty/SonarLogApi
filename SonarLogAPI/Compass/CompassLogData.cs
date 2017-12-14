using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SonarLogAPI.Compass
{

	/// <summary>
	///Log data that consists of <see cref="CompassLogEntry"/>s.
	/// </summary>
	public class CompassLogData
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
		/// <see cref="CompassLogEntry"/>s.
		/// </summary>
		public IEnumerable<CompassLogEntry> CompassLogEntrys { get; set; }

		/// <summary>
		/// Writes <see cref="CompassLogData"/> object to <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/>.</param>
		/// <param name="data"><see cref="CompassLogData"/></param>
		public static void WriteToStream(Stream stream, CompassLogData data)
		{
			if (!stream.CanWrite)
				throw new InvalidOperationException("Can't write to stream");

			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			{
				foreach (var entry in data.CompassLogEntrys)
				{
					writer.WriteLine(entry);
				}
			}
		}

		/// <summary>
		/// Reads <see cref="CompassLogData"/> from <see cref="Stream"/> with default values order. Default order is { 0, "Time" }, { 1, "Latitude" }, { 2, "Longitude" }, { 3, "Heading" }, { 4, "Pitch" }, { 5, "Roll" }.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/>.</param>
		/// <returns><see cref="CompassLogData"/> object.</returns>
		public static CompassLogData ReadFromStream(Stream stream)
		{
			return ReadFromStream(stream, new Dictionary<int, string>() { { 0, "Time" }, { 1, "Latitude" },
				{ 2, "Longitude" }, { 3, "Heading" }, { 4, "Pitch" }, { 5, "Roll" } });
		}

		/// <summary>
		/// Reads <see cref="CompassLogData"/> from <see cref="Stream"/> with specified values order.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/>.</param>
		/// <param name="valuesOrder">Order of values at strings. For example: { 0, "Time" }, { 1, "Latitude" }, { 2, "Longitude" }, { 3, "Heading" }, { 4, "Pitch" }, { 5, "Roll" }.</param>
		/// <returns><see cref="CompassLogData"/> object.</returns>
		public static CompassLogData ReadFromStream(Stream stream, IDictionary<int, string> valuesOrder)
		{
			if (!stream.CanRead)
				throw new InvalidOperationException("Can't read from stream");

			var listofEntrys = new List<CompassLogEntry>();

			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				while (!reader.EndOfStream)
				{
					if (CompassLogEntry.TryParse(reader.ReadLine(), ',', valuesOrder, out var result))
						listofEntrys.Add(result);
				}
			}

			return new CompassLogData()
			{
				CreationDateTime = DateTimeOffset.Now,
				CompassLogEntrys = listofEntrys,
				Name = $"{listofEntrys.Count} compass entrys."
			};
		}
	}
}
