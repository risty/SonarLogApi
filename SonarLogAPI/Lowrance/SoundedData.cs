namespace SonarLogAPI.Lowrance
{
	using System;
	using System.Linq;

	using SonarLogAPI.Primitives;

	/// <summary>
	/// Sounded data
	/// </summary>
	public class SoundedData
	{
		/// <summary>
		/// Inner byte array of data values
		/// </summary>
		public byte[] Data { get; }

		/// <summary>
		/// Type of channel
		/// </summary>
		public ChannelType ChannelType { get; }

		/// <summary>
		/// Upper limit of <see cref="SoundedData" />
		/// </summary>
		public LinearDimension UpperLimit { get; }

		/// <summary>
		/// Lower limit of <see cref="SoundedData" />
		/// </summary>
		public LinearDimension LowerLimit { get; }

		/// <summary>
		/// Create instance of <see cref="SoundedData"/>.
		/// </summary>
		/// <param name="data">Sounded data</param>
		/// <param name="channelType"><see cref="Frame.ChannelType"/></param>
		/// <param name="upperLimit"><see cref="UpperLimit"/></param>
		/// <param name="lowerLimit"><see cref="LowerLimit"/></param>
		public SoundedData(byte[] data, ChannelType channelType, LinearDimension upperLimit, LinearDimension lowerLimit)
		{
			Data = data;
			ChannelType = channelType;
			UpperLimit = upperLimit;
			LowerLimit = lowerLimit;

		}

		/// <summary>
		/// Flip <see cref="SoundedData"/> for SidescanComposite and ThreeD channels.
		/// </summary>
		/// <returns>Flipped <see cref="SoundedData"/>.</returns>
		public SoundedData FlipSoundedData()
		{
			return FlipSoundedData(this);
		}

		/// <summary>
		/// Flip <see cref="SoundedData"/> for SidescanComposite and ThreeD channels.
		/// </summary>
		/// <param name="soundedData">Inner <see cref="SoundedData"/>.</param>
		/// <returns>Flipped <see cref="SoundedData"/>.</returns>
		public static SoundedData FlipSoundedData(SoundedData soundedData)
		{
			switch (soundedData.ChannelType)
			{
				case ChannelType.Primary:
				case ChannelType.Secondary:
				case ChannelType.DownScan:
				case ChannelType.SidescanLeft:
				case ChannelType.SidescanRight:
					//do nothing with sounded data of this channels
					break;
				case ChannelType.SidescanComposite:

					//invert Data array and change upper and liwer limits values
					return new SoundedData(soundedData.Data.Reverse().ToArray(), soundedData.ChannelType,
						soundedData.LowerLimit * -1, soundedData.UpperLimit * -1);

				case ChannelType.ThreeD:
					throw new NotImplementedException();
				default:
					throw new ArgumentOutOfRangeException();
			}

			return soundedData;
		}

		/// <summary>
		/// Create instance of <see cref="SoundedData"/> with generated <see cref="Data"/>
		/// </summary>
		/// <param name="packetSize"><see cref="Frame.PacketSize"/></param>
		/// <param name="channelType"><see cref="Frame.ChannelType"/></param>
		/// <param name="depth"><see cref="Frame.Depth"/></param>
		/// <param name="upperLimit"><see cref="UpperLimit"/></param>
		/// <param name="lowerLimit"><see cref="LowerLimit"/></param>
		/// <returns>Instance of <see cref="SoundedData"/> with generated <see cref="Data"/></returns>
		public static SoundedData GenerateData(short packetSize, ChannelType channelType,
			LinearDimension depth, LinearDimension upperLimit, LinearDimension lowerLimit)
		{
			if (depth.GetMeters() < 0) throw new ArgumentOutOfRangeException(nameof(depth), channelType, nameof(depth)
				+ "cant be less then zero.");
			if (packetSize < 0) throw new ArgumentOutOfRangeException(nameof(packetSize), packetSize, nameof(packetSize)
				+ "cant be less then zero.");
			if (upperLimit.GetMeters() > lowerLimit.GetMeters()) throw new ArgumentOutOfRangeException(nameof(upperLimit), nameof(upperLimit) + " more then " + nameof(lowerLimit));

			switch (channelType)
			{
				case ChannelType.Primary:
				case ChannelType.Secondary:
				case ChannelType.DownScan:
				case ChannelType.SidescanRight:

					if (upperLimit.GetMeters() < 0) throw new ArgumentOutOfRangeException(nameof(upperLimit), upperLimit, nameof(upperLimit)
				+ "cant be less then zero for " + channelType);

					if (lowerLimit.GetMeters() < 0) throw new ArgumentOutOfRangeException(nameof(lowerLimit), lowerLimit, nameof(lowerLimit)
				+ "cant be less then zero for " + channelType);

					var verticalArrayForSoundedData = GetArrayForNoiseAndBottomSurfaces(packetSize, depth, upperLimit, lowerLimit);
					return new SoundedData(verticalArrayForSoundedData, channelType, upperLimit, lowerLimit);

				case ChannelType.SidescanLeft:

					if (upperLimit.GetMeters() > 0) throw new ArgumentOutOfRangeException(nameof(upperLimit), upperLimit, nameof(upperLimit)
				+ "cant be more then zero for " + channelType);

					if (lowerLimit.GetMeters() > 0) throw new ArgumentOutOfRangeException(nameof(lowerLimit), lowerLimit, nameof(lowerLimit)
				+ "cant be more then zero for " + channelType);

					var sraightArrayForSidescanLeft = GetArrayForNoiseAndBottomSurfaces(packetSize, depth, lowerLimit * -1, upperLimit * -1);
					return new SoundedData(sraightArrayForSidescanLeft.Reverse().ToArray(), channelType, upperLimit, lowerLimit);

				case ChannelType.SidescanComposite:
					if (upperLimit.GetMeters() > 0) throw new ArgumentOutOfRangeException(nameof(upperLimit), upperLimit, nameof(upperLimit)
				+ "cant be more then zero for " + channelType);

					if (lowerLimit.GetMeters() < 0) throw new ArgumentOutOfRangeException(nameof(lowerLimit), lowerLimit, nameof(lowerLimit)
				+ "cant be less then zero for " + channelType);

					var sraightArrayForSidescanComposite = GetArrayForNoiseAndBottomSurfaces((short)(packetSize / 2), depth, LinearDimension.FromMeters(0), lowerLimit);
					var fullSidescan = new byte[packetSize];
					sraightArrayForSidescanComposite.Reverse().ToArray().CopyTo(fullSidescan, 0);
					sraightArrayForSidescanComposite.CopyTo(fullSidescan, packetSize / 2);
					return new SoundedData(fullSidescan, channelType, upperLimit, lowerLimit);

				case ChannelType.ThreeD:
					throw new NotImplementedException();
				default:
					throw new ArgumentOutOfRangeException(nameof(channelType), channelType, null);
			}

		}

		private static byte[] GetDecreasingByteSurface(uint surfacePacketSize)
		{
			var surfaceArray = new byte[surfacePacketSize];
			var intensityStep = (double)byte.MaxValue / (double)surfacePacketSize;
			var randomValue = new Random();
			for (var i = 0; i < surfaceArray.Length; i++)
			{
				surfaceArray[i] = (byte)((byte.MaxValue - i * intensityStep) * (double)randomValue.Next(100) / 200d);
			}

			return surfaceArray;
		}

		private static byte[] GetArrayForNoiseAndBottomSurfaces(short packetSize, LinearDimension depth,
			LinearDimension upperLimit, LinearDimension lowerLimit)
		{
			//calc packet size for all the water column (from water surface to lower limit)
			var allTheWaterColumnPacketSize = (uint)Math.Ceiling(lowerLimit * packetSize / (lowerLimit - upperLimit));

			//create byte array for all the column 
			var allTheWaterColumnBytesArray = new byte[allTheWaterColumnPacketSize];

			//and fill it with patterns for water surface noises and bottom surface
			// lets water surface noises always has 0.5 meter depth. then one meter packetsize is
			var oneMeterPacketSize = (uint)(allTheWaterColumnPacketSize / lowerLimit.GetMeters());

			//then noise array is 
			var noiseArray = GetDecreasingByteSurface(oneMeterPacketSize);

			//copy noise array to begin of allTheWaterColumnBytesArray
			noiseArray.CopyTo(allTheWaterColumnBytesArray, 0);

			//lets fill by bottom surface all the place beyond depth and lower limit
			if (depth < lowerLimit)
			{
				//calc bottom surface packet size
				var bottomSurfacePacketSize = (uint)((lowerLimit - depth) * allTheWaterColumnPacketSize / lowerLimit);

				//then bottom array is 
				var bottomArray = GetDecreasingByteSurface(bottomSurfacePacketSize);

				//and copy bottom array to tne end of allTheWaterColumnBytesArray
				bottomArray.CopyTo(allTheWaterColumnBytesArray, allTheWaterColumnPacketSize - bottomSurfacePacketSize);
			}

			//finally cut allTheWaterColumnBytesArray to array from upper to lower limit and return SoundedData
			var arrayForSoundedData = new byte[packetSize];
			allTheWaterColumnBytesArray.CopyTo(arrayForSoundedData, allTheWaterColumnPacketSize - packetSize);
			return arrayForSoundedData;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"From {LowerLimit:#.#} to {UpperLimit:#.#}, lenght {Data.Length}.";
		}

	}
}