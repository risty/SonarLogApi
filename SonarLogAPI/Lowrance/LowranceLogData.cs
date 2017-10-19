namespace SonarLogAPI.Lowrance
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	/// <summary>
	/// Lowrance Log Data
	/// </summary>
	public class LowranceLogData
	{
		/// <summary>
		/// <see cref="LowranceLogData"/> name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// <see cref="LowranceLogData"/> creation time.
		/// </summary>
		public DateTimeOffset CreationDateTime { get; set; }

		/// <summary>
		/// Lowrance log file <see cref="SonarLogAPI.Lowrance.Header"/>
		/// </summary>
		public Header Header { get; set; }

		/// <summary>
		/// Frames
		/// </summary>
		public List<Frame> Frames { get; set; }

		/// <summary>
		/// Reads <see cref="LowranceLogData"/> from <see cref="Stream"/>
		/// </summary>
		/// <param name="stream"><see cref="Stream"/></param>
		/// <returns><see cref="LowranceLogData"/></returns>
		public static LowranceLogData ReadFromStream(Stream stream)
		{
			if (!stream.CanRead)
				throw new InvalidOperationException("Can't read stream");
			if (!stream.CanSeek)
				throw new InvalidOperationException("Can't seek stream");

			//first of all read header
			if (stream.Length < Header.Lenght)
				throw new ArgumentException(nameof(stream.Length) + " less then " + nameof(Header.Lenght), nameof(stream.Length));

			var data = new LowranceLogData
			{
				Frames = new List<Frame>()
			};

			using (var reader = new BinaryReader(stream))
			{
				data.Header = Header.ReadHeader(reader, 0);

				var framesMap = Frame.GetFramesMap(reader, Header.Lenght, data.Header.FileVersion, out var fileCreationDateTime);

				data.CreationDateTime = fileCreationDateTime;

				foreach (var frameRecord in framesMap)
				{
					var frame = Frame.ReadFrame(reader, frameRecord.Item1, data.Header.FileVersion);
					frame.DateTimeOffset = data.CreationDateTime + frameRecord.Item2;
					data.Frames.Add(frame);
				}
			}

			return data;
		}

		/// <summary>
		/// Writes <see cref="LowranceLogData"/> to <see cref="Stream"/>
		/// </summary>
		/// <param name="stream"><see cref="Stream"/></param>
		/// <param name="data"><see cref="LowranceLogData"/></param>
		public static void WriteToStream(Stream stream, LowranceLogData data)
		{
			using (var writer = new BinaryWriter(stream))
			{
				Header.WriteHeader(writer, data.Header, 0);
				Frame.WriteFrames(writer, data.Frames, Header.Lenght, data.Header.FileVersion);
			}
		}

		public override string ToString()
		{
			return $"{Header.FileVersion}, Frames count = {Frames.Count}";
		}
	}
}