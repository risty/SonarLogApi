namespace SonarLogAPI.CVS
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class CVSLogData
	{
		/// <summary>
		/// Data Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Data creation time
		/// </summary>
		public DateTimeOffset CreationDateTime { get; set; }

		/// <summary>
		/// Depth points
		/// </summary>
		public List<CvsLogEntry> Points { get; set; }

		/// <summary>
		/// Writes <see cref="CVSLogData"/> object to <see cref="Stream"/>
		/// </summary>
		/// <param name="stream"><see cref="Stream"/></param>
		/// <param name="data"><see cref="CVSLogData"/> object</param>
		public static void WriteToStream(Stream stream, CVSLogData data)
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