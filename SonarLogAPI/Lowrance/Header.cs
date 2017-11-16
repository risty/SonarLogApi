namespace SonarLogAPI.Lowrance
{
	using System;
	using System.IO;

	/// <summary>
	/// Supported Lowrance log file versions
	/// </summary>
	public enum FileVersion : byte
	{
		SLG = 1,
		SL2 = 2,
		SL3 = 3,
	}

	/// <summary>
	/// SlG/SL2/SL3 binary file header, 8 bytes lenght
	/// </summary>
	public struct Header
	{
		/// <summary>
		/// Header lenght in bytes
		/// </summary>
		public static byte Lenght { get; } = 8;

		/// <summary>
		/// Standart SL2 Header
		/// </summary>
		public static Header sl2
		{
			get
			{
				var header = new Header
				{
					FileVersion = FileVersion.SL2,
					HardwareVersion = 1,
					BlockSize = 1970
				};

				return header;
			}
		}

		/// <summary>
		/// Standart SL3 Header
		/// </summary>
		public static Header sl3
		{
			get
			{
				var header = new Header
				{
					FileVersion = FileVersion.SL3,
					HardwareVersion = 1,
					BlockSize = 3200
				};

				return header;
			}
		}



		/// <summary>
		/// FileVersion
		/// </summary>
		public FileVersion FileVersion { get; set; }

		/// <summary>
		/// Lowrance product hardware version. 
		/// </summary>
		public short HardwareVersion { get; set; }

		/// <summary>
		/// SonarType or BlockSize. 1970/Downscan #b207, 3200/Sidescan #800c
		/// </summary>
		public short BlockSize { get; set; }

		/// <summary>
		/// Reads <see cref="SonarLogAPI.Lowrance.Header"/> from <see cref="BinaryReader"/>.
		/// </summary>
		/// <param name="reader"><see cref="BinaryReader"/>.</param>
		/// <param name="headerFirstByteOffset">Offset of <see cref="SonarLogAPI.Lowrance.Header"/> first byte</param>
		/// <returns><see cref="SonarLogAPI.Lowrance.Header"/> object.</returns>
		public static Header ReadHeader(BinaryReader reader, long headerFirstByteOffset)
		{
			if (reader.BaseStream.Length < headerFirstByteOffset + Lenght)
				throw new ArgumentException("Stream length less then "
											+ nameof(headerFirstByteOffset) + "+ " + nameof(Header) + " " + nameof(Lenght));

			//seeking to header first byte
			reader.BaseStream.Seek(headerFirstByteOffset, SeekOrigin.Begin);

			var header = new Header
			{
				FileVersion = (FileVersion)reader.ReadInt16(),
				HardwareVersion = reader.ReadInt16(),
				BlockSize = reader.ReadInt16()
			};

			return header;
		}

		/// <summary>
		/// Writes <see cref="SonarLogAPI.Lowrance.Header"/> to <see cref="BinaryWriter"/>.
		/// </summary>
		/// <param name="writer"><see cref="BinaryWriter"/>.</param>
		/// <param name="headerToWrite"><see cref="SonarLogAPI.Lowrance.Header"/> object to write.</param>
		/// <param name="headerFirstByteOffset">Offset of <see cref="SonarLogAPI.Lowrance.Header"/> first byte.</param>
		public static void WriteHeader(BinaryWriter writer, Header headerToWrite, long headerFirstByteOffset)
		{
			//seeking to header first byte
			writer.BaseStream.Seek(headerFirstByteOffset, SeekOrigin.Begin);
			//writing bytes
			writer.Write((short)headerToWrite.FileVersion);
			writer.Write(headerToWrite.HardwareVersion);
			writer.Write(headerToWrite.BlockSize);
			//write zero from current position to lenght position
			while (writer.BaseStream.Position < headerFirstByteOffset + Lenght)
				writer.Write(new byte());
		}

		public override string ToString()
		{
			return $"{FileVersion}, {HardwareVersion}, {BlockSize}";
		}

	}

}