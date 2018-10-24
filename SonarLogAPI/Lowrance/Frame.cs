namespace SonarLogAPI.Lowrance
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Primitives;

    /// <summary>
    /// <see cref="ChannelType"/> from which frame is given.
    /// </summary>
    public enum ChannelType : byte
    {
        /// <summary>
        /// Traditional Sonar
        /// </summary>
        Primary = 0,

        /// <summary>
        /// Traditional Sonar
        /// </summary>
        Secondary = 1,

        /// <summary>
        /// DownScan Imaging
        /// </summary>
        DownScan = 2,

        SidescanLeft = 3,
        SidescanRight = 4,
        SidescanComposite = 5,
        ThreeD = 9,
    }

    /// <summary>
    /// Sonar transducer frequency
    /// </summary>
    public enum Frequency : byte
    {
        Frequency_200KHz = 0,
        Frequency_50KHz = 1,
        Frequency_83KHz = 2,
        Frequency_455KHz = 3,
        Frequency_800KHz = 4,
        Frequency_38KHz = 5,
        Frequency_28KHz = 6,
        Frequency_130KHz210KHz = 7,
        Frequency_90KHz150KHz = 8,
        Frequency_40KHz60KHz = 9,
        Frequency_25KHz45KHz = 10,
    }

    public enum FrameFlags : byte
    {
        TrackValid = 0,
        Preset_0_1 = 1,
        Unknown0_2 = 2,
        PositionValid = 3,
        Unknown_0_4 = 4,
        CourseOrSpeed_0_5 = 5,
        SpeedValid = 6,
        Preset_0_7 = 7,
        Unknown1_0 = 8,
        AltitudeOrCourseOrSpeed_1_1 = 9,
        Unknown1_2 = 10,
        Unknown1_3 = 11,
        Unknown1_4 = 12,
        Unknown1_5 = 13,
        AltitudeValid = 14,
        HeadingValid = 15
    }


    /// <summary>
    /// Map of Sl2 Frame properties offsets
    /// </summary>
    public enum Sl2FramePropertiesOffsets
    {
        FrameOffset = 0,//int, 4 bytes
        LastPrimaryChannelFrameOffset = 4, //int, 4 bytes
        LastSecondaryChannelFrameOffset = 8, //int, 4 bytes
        LastDownScanChannelFrameOffset = 12, //int, 4 bytes
        LastSidescanLeftChannelFrameOffset = 16, //int, 4 bytes
        LastSidescanRightChannelFrameOffset = 20, //int, 4 bytes
        LastSidescanCompositeChannelFrameOffset = 24, //int, 4 bytes
        ThisFrameSize = 28, //short, 2 bytes. Bytes to next frame Start
        PreviousFrameSize = 30, //short, 2 bytes. Bytes to previous frame Start
        ChannelType = 32,//short, 2 bytes
        PacketSize = 34,//short, 2 bytes
        FrameIndex = 36,//int, 4 bytes
        UpperLimit = 40,//float, 4 bytes
        LowerLimit = 44,//float, 4 bytes
        Frequency = 50,//byte

        //undefined offset 53, 1 byte
        CreationDataTime = 60,//int, 4 bytes, value in fist frame = Unix time stamp of file creation.
                              //other frames - time from device boot.

        Depth = 64,//float, 4 bytes
        KeelDepth = 68,//float, 4 bytes

        //undefined offset 72, 1 byte ?
        //undefined offset 82, 2 bytes ?
        //undefined offset 86, 2 bytes ?
        //undefined offset 90, 2 bytes ?
        //undefined offset 94, 2 bytes ?

        SpeedGps = 100,//float, 4 bytes
        Temperature = 104,//float, 4 bytes
        IntLongitude = 108,//int, 4 bytes
        IntLatitude = 112,//int, 4 bytes
        WaterSpeed = 116,//float, 4 bytes
        CourseOverGround = 120,//float, 4 bytes
        Altitude = 124,//float, 4 bytes
        Heading = 128,//float, 4 bytes

        Flags = 132, // two bytes

        //undefined offset 136, 2 bytes
        //undefined offset 138, 2 bytes
        //оr single int 

        TimeOffset = 140,//int, 4 bytes. if creation data(CreationDataTime) in fist frame exist(not -1) - time from device boot?
        SoundedData = 144// bytes array, size of PacketSize
    }

    /// <summary>
    /// Map of Sl3 Frame properties offsets
    /// </summary>
    public enum Sl3FramePropertiesOffsets
    {
        FrameOffset = 0,//int, 4 bytes
        ThisFrameSize = 8,//short, 2 bytes. Bytes to next frame Start
        PreviousFrameSize = 10,//short, 2 bytes. Bytes to previous frame Start
        ChannelType = 12,//short, 2 bytes
        FrameIndex = 16,//int, 4 bytes
        UpperLimit = 20,//float, 4 bytes
        LowerLimit = 24,//float, 4 bytes
        CreationDataTime = 40,//int, 4 bytes, value at fist frame = Unix time stamp of file creation. if GPS cant find position value will be "-1"
                              //other frames - time in milliseconds from device boot.
        PacketSize = 44,//short, 2 bytes
        Depth = 48,//float, 4 bytes
        Frequency = 52,//byte
        SpeedGps = 84,//float, 4 bytes
        Temperature = 88,//float, 4 bytes
        IntLongitude = 92,//int, 4 bytes
        IntLatitude = 96,//int, 4 bytes
        WaterSpeed = 100,//float, 4 bytes
        CourseOverGround = 104,//float, 4 bytes
        Altitude = 108,//float, 4 bytes
        Heading = 112,//float, 4 bytes
        Flags = 116, // two bytes
        TimeOffset = 124,//int, 4 bytes, time in milliseconds from log file creation.
        LastPrimaryChannelFrameOffset = 128, //int, 4 bytes
        LastSecondaryChannelFrameOffset = 132, //int, 4 bytes
        LastDownScanChannelFrameOffset = 136, //int, 4 bytes
        LastSidescanLeftChannelFrameOffset = 140, //int, 4 bytes
        LastSidescanRightChannelFrameOffset = 144, //int, 4 bytes
        LastSidescanCompositeChannelFrameOffset = 148,//int, 4 bytes
        LastThreeDChannelFrameOffset = 164,//int, 4 bytes
        SoundedData = 168// bytes array
    }

    /// <summary>
    /// Sonar log data frame
    /// </summary>
    public class Frame : IDepthPointSource, ITemperaturePointSource, IComparable<Frame>
    {
        private const double _earthWGS84PolarRadius = 6356752.31424518d;
        private const double _radConversion = 180d / Math.PI;

        #region Frame properties

        //Properties based on http://wiki.openstreetmap.org/wiki/SL2

        /// <summary>
        /// Type of channel
        /// </summary>
        public ChannelType ChannelType { get; set; }

        /// <summary>
        /// Size of sounding/bounce data.
        /// </summary>
        public short PacketSize { get; set; }

        /// <summary>
        /// Starts at 0. Used to match frames/block on different channels.
        /// </summary>
        public int FrameIndex { get; set; }

        /// <summary>
        /// Sonar Frequency
        /// </summary>
        public Frequency Frequency { get; set; }

        /// <summary>
        /// Depth under water surface(transponder)
        /// </summary>
        public LinearDimension Depth { get; set; }

        /// <summary>
        /// Depth under keel
        /// </summary>
        public LinearDimension KeelDepth { get; set; }

        /// <summary>
        /// Speed from GPS in meters/second
        /// </summary>
        public float SpeedGps { get; set; }

        /// <summary>
        /// Temperature, in Celsius
        /// </summary>
        public float Temperature { get; set; }

        /// <summary>
        /// CoordinatePoint
        /// </summary>
        public CoordinatePoint Point { get; set; }

        /// <summary>
        /// WaterSpeed in m/s. This value is taken from an actual Water Speed Sensor (such as a paddle-wheel). 
        /// If such a sensor is not present, it takes the value from "Speed" (GPS NMEA data) 
        /// and sets WaterSpeedValid to false.
        /// </summary>
        public float WaterSpeed { get; set; }

        /// <summary>
        /// Track/Course-Over-Ground in radians. Real direction of boat movement. Taken from GPS NMEA data. 
        /// </summary>
        public float CourseOverGround { get; set; }

        /// <summary>
        /// Altitude in meters. Taken from GPS NMEA data. 
        /// </summary>
        public LinearDimension Altitude { get; set; }

        /// <summary>
        /// Heading in radians. Angle in radians between magnetic north and transducer.
        /// </summary>
        public float Heading { get; set; }

        /// <summary>
        /// Bit coded flags
        /// </summary>
        public List<FrameFlags> Flags { get; set; }

        /// <summary>
        /// Frame creation time
        /// </summary>
        public DateTimeOffset DateTimeOffset { get; set; }

        /// <summary>
        /// Contains sounding/bounce data
        /// </summary>
        public SoundedData SoundedData { get; set; }

        #endregion

        /// <summary>
        /// Gets four bytes from <see cref="valueByteOffset"/> position in each frame in <see cref="BinaryReader"/> and represent is as single bytes, shorts, int, float.
        /// </summary>
        /// <param name="reader"><see cref="BinaryReader"/></param>
        /// <param name="firstFrameFirstByteOffset">Offset of first byte of firs frame in file</param>
        /// <param name="valueByteOffset"></param>
        /// <param name="version"><see cref="FileVersion"/></param>
        /// <returns><see cref="IDictionary{TKey,TValue}"/>, where <see cref="TKey"/> is Frame First Byte Offset and <see cref="Value"/> is <see cref="Tuple{T1,T2,T3,T4,T5,T6,T7}"/>.
        /// T1 is byte[] of four bytes. 
        /// T2 is short representation of first two bytes.
        /// T3 is short representation of second two bytes.
        /// T4 is int representation of all four bytes.
        /// T5 is float representation of all four bytes.
        /// T6 is frame index.
        /// T7 is <see cref="ChannelType"/></returns>
        public static IDictionary<int, Tuple<byte[], short, short, int, float, int, ChannelType>> ValuesResearch(BinaryReader reader, int firstFrameFirstByteOffset, int valueByteOffset, FileVersion version)
        {
            var dictionary = new Dictionary<int, Tuple<byte[], short, short, int, float, int, ChannelType>>();
            var slType = GetOffsetsTypeForFileVersion(version);
            var map = GetFramesMap(reader, firstFrameFirstByteOffset, version, out _);

            foreach (var position in map.Select(tuple => tuple.Item1))
            {
                // gets 4 bytes and try to represent 
                var bytesValues = GetBytes(reader, position + valueByteOffset, 4);
                var firstShortValue = GetShort(reader, position + valueByteOffset);
                var secondShortValue = GetShort(reader, position + valueByteOffset + 2);
                var intValue = GetInt(reader, position + valueByteOffset);
                var floatValue = GetFloat(reader, position + valueByteOffset);
                var frameIndex = GetInt(reader, position + GetOffset(slType, "FrameIndex"));
                var channelType = (ChannelType)GetShort(reader, position + GetOffset(slType, "ChannelType"));

                dictionary.Add(position,
                    new Tuple<byte[], short, short, int, float, int, ChannelType>(bytesValues, firstShortValue, secondShortValue, intValue, floatValue, frameIndex, channelType));

            }
            return dictionary;
        }

        /// <summary>
        ///Values, which are specific for sl2 frame type.
        /// </summary>
        private static IEnumerable<int> SpecificSL2FrameValues => new[]
        {
	
			/*Looking for the 
			* ChannelType = 32, - short, 2 bytes
			* PacketSize = 34, - short, 2 bytes
			* together for SL2 files
			*/
		
			// for Primary = 0 channel, size is 1920 = "00 00 80 07"
			125829120,
			// for Primary = 0 channel, size is 3072 = "00 00 00 0C"
			201326592,
			// for Secondary = 1 channel, size is 1920 = "01 00 80 07"
			125829121,
			// for DownScan = 2 channel, size is 1920 = "02 00 80 07"
			125829122,
			// for DownScan = 2 channel, size is 1400 = ""
			91750402,

			// for SidescanLeft = 3 channel is ""
			// for SidescanRight = 4 channel is ""
			// for SidescanComposite = 5 channel, size is 2800 = ""
			183500805
			// no 3D channel in this file type

		};

        /// <summary>
        /// Values, which are specific for sl3 frame type.
        /// </summary>
        private static IEnumerable<int> SpecificSL3FrameValues => new[]
        {	
			/*Looking for the 
			* PacketSize = 44, - short, 2 bytes
			* and two bytes after is "00 00"
			* together for SL3 files
			*/
		
			// for size is 3072 = "00 0C 00 00"
			3072,

			// for size is 2800 = "__ __ 00 00"
			2800,

			// for size is 1400 = "__ __ 00 00"
			1400,
        };

        public static Type FindBeginOfFirstValdFrameAndFileVersion(BinaryReader reader, ref int readPosition)
        {
            Type fileType = null;
            while (readPosition <= reader.BaseStream.Length - 4)
            {
                //if we found specific value
                if (SpecificSL2FrameValues.Contains(GetInt(reader, readPosition)))
                {
                    //sets readPosition to first byte of founded frame
                    //32 - ChannelTypeOffset
                    readPosition = readPosition - 32;
                    fileType = typeof(Sl2FramePropertiesOffsets);
                    break;
                }

                //if we found specific value
                if (SpecificSL3FrameValues.Contains(GetInt(reader, readPosition)))
                {
                    //sets readPosition to first byte of founded frame
                    //44 - ChannelTypeOffset
                    readPosition = readPosition - 44;
                    fileType = typeof(Sl3FramePropertiesOffsets);
                    break;
                }
                readPosition++;
            }

            return fileType;
        }

        /// <summary>
        /// Verify SL log file and create frames map.
        /// </summary>
        /// <param name="reader"><see cref="BinaryReader"/>.</param>
        /// <param name="readPosition">Offset of first frame (in bytes) in <see cref="BinaryReader"/>'s stream.</param>
        /// <param name="version"><see cref="FileVersion"/>.</param>
        /// <param name="fileCreationTime">File creation time.</param>
        /// <returns>Sequence(tuple) of frames offsets at reader and Timespans from logging start.</returns>
        public static IEnumerable<Tuple<int, TimeSpan>> GetFramesMap(BinaryReader reader, int readPosition, FileVersion version, out DateTimeOffset fileCreationTime)
        {
            fileCreationTime = DateTimeOffset.MinValue;

            Type slType;
            try
            {
                slType = GetOffsetsTypeForFileVersion(version);
            }
            catch (Exception e)
            {
                slType = FindBeginOfFirstValdFrameAndFileVersion(reader, ref readPosition);

                if (slType == null)
                    throw new ArgumentNullException(nameof(slType), "Can't define SL file type");
            }

            var framesMap = new List<Tuple<int, TimeSpan>>();

            var thisTypeFrameSizeOffset = GetOffset(slType, "ThisFrameSize");

            ////if stream less then first read position then return empty IEnumerable<int>
            //if (readPosition > reader.BaseStream.Length)
            //	return offsets;

            //size of first frame less then "ThisFrameSize" value position then return empty IEnumerable<int>
            if (readPosition + thisTypeFrameSizeOffset > reader.BaseStream.Length)
                return framesMap;

            //else gets frame size
            var thisFrameSize = GetShort(reader, readPosition + thisTypeFrameSizeOffset);

            //checks that stream more or equal then frame
            while (readPosition + thisFrameSize <= reader.BaseStream.Length)
            {
                //gets frame offset from frame data
                var frameOffset = GetInt(reader, readPosition + GetOffset(slType, "FrameOffset"));

                //if frame is at his place then frame is valid by default
                if (frameOffset == readPosition)
                {
                    //get file creation time from this frame
                    var creationDataTimeInt = GetInt(reader, readPosition + GetOffset(slType, "CreationDataTime"));
                    var creationDataTimeOffset = DateTimeOffset.FromUnixTimeSeconds(creationDataTimeInt);
                    //if time from frame later then previous values then takes them
                    if (creationDataTimeOffset > fileCreationTime)
                        fileCreationTime = creationDataTimeOffset;

                    //get time offset in milliseconds
                    var offset = GetInt(reader, readPosition + GetOffset(slType, "TimeOffset"));
                    //adds offsets to List
                    framesMap.Add(new Tuple<int, TimeSpan>(readPosition, TimeSpan.FromMilliseconds(offset)));
                    //and move readPosition to next frame first byte
                    readPosition += thisFrameSize;

                    if (readPosition + thisTypeFrameSizeOffset <= reader.BaseStream.Length)
                        thisFrameSize = GetShort(reader, readPosition + thisTypeFrameSizeOffset);
                    else
                        break;

                    continue;

                }

                //if size BaseStream less then  readPosition + "ThisFrameSize"value offset then return current IEnumerable<int>
                if (readPosition + thisTypeFrameSizeOffset > reader.BaseStream.Length)
                    return framesMap;
                thisFrameSize = GetShort(reader, readPosition + thisTypeFrameSizeOffset);

                if (readPosition + thisFrameSize + GetOffset(slType, "PreviousFrameSize") > reader.BaseStream.Length)
                    return framesMap;
                var thisFrameSizeFromNextFrame = GetShort(reader, readPosition + thisFrameSize + GetOffset(slType, "PreviousFrameSize"));

                //checks value of frame size from current and next frame. if it's equal then add first frame offset to list
                if (thisFrameSize == thisFrameSizeFromNextFrame)
                {
                    //get file creation time from this frame
                    var creationDataTimeInt = GetInt(reader, readPosition + GetOffset(slType, "CreationDataTime"));
                    var creationDataTimeOffset = DateTimeOffset.FromUnixTimeSeconds(creationDataTimeInt);
                    //if time from frame later then previous values then takes them
                    if (creationDataTimeOffset > fileCreationTime)
                        fileCreationTime = creationDataTimeOffset;

                    //get time offset in milliseconds
                    var offset = GetInt(reader, readPosition + GetOffset(slType, "TimeOffset"));
                    //adds offsets to List
                    framesMap.Add(new Tuple<int, TimeSpan>(readPosition, TimeSpan.FromMilliseconds(offset)));

                    //and move readPosition to next frame first byte
                    readPosition += thisFrameSize;

                    if (readPosition + thisTypeFrameSizeOffset <= reader.BaseStream.Length)
                        thisFrameSize = GetShort(reader, readPosition + thisTypeFrameSizeOffset);
                    else
                        break;

                    continue;
                }
                //and move readPosition to next frame first byte
                readPosition += thisFrameSize;

                //finds specific bytes and set readPosition to first byte of frame
                FindBeginOfFirstValdFrameAndFileVersion(reader, ref readPosition);

            }

            return framesMap;
        }

        /// <summary>
        /// Read frame from Reader.
        /// </summary>
        /// <param name="reader"><see cref="BinaryReader"/>.</param>
        /// <param name="frameStartByteOffset">Frame offset in readers stream.</param>
        /// <param name="version"><see cref="FileVersion"/>.</param>
        /// <returns><see cref="Frame"/> object.</returns>
        public static Frame ReadFrame(BinaryReader reader, int frameStartByteOffset, FileVersion version)
        {
            var slType = GetOffsetsTypeForFileVersion(version);

            var frame = new Frame();

            /*TODO check performance of reading frames set with(details below):
			 * 1.this offset parse realization(GetOffset by property name)
			 * 2.getting offsets directly from specified enum
			 * 3.without offset map. sequence of read-seek-read-seek in one bytes block
			 */

            //frame.FrameOffset = GetInt(reader, frameStartByteOffset + GetOffset(slType, "FrameOffset"));
            var thisFrameSize = GetShort(reader, frameStartByteOffset + GetOffset(slType, "ThisFrameSize"));
            //frame.PreviousFrameSize = GetShort(reader, frameStartByteOffset + GetOffset(slType, "PreviousFrameSize"));
            frame.ChannelType = (ChannelType)GetShort(reader, frameStartByteOffset + GetOffset(slType, "ChannelType"));

            //if ChannelType == ThreeD then correct PacketSize.
            if (frame.ChannelType == ChannelType.ThreeD)
                frame.PacketSize = (short)(thisFrameSize - 168);
            else
                frame.PacketSize = GetShort(reader, frameStartByteOffset + GetOffset(slType, "PacketSize"));

            frame.FrameIndex = GetInt(reader, frameStartByteOffset + GetOffset(slType, "FrameIndex"));
            var upperLimit = new LinearDimension(GetFloat(reader, frameStartByteOffset + GetOffset(slType, "UpperLimit")),
                LinearDimensionUnit.Foot);
            var lowerLimit = new LinearDimension(GetFloat(reader, frameStartByteOffset + GetOffset(slType, "LowerLimit")),
                LinearDimensionUnit.Foot);
            frame.Frequency = (Frequency)GetBytes(reader, frameStartByteOffset + GetOffset(slType, "Frequency"), 1)[0];
            frame.Depth = new LinearDimension(GetFloat(reader, frameStartByteOffset + GetOffset(slType, "Depth")), LinearDimensionUnit.Foot);

            if (version != FileVersion.SL3)
                frame.KeelDepth = new LinearDimension(GetFloat(reader, frameStartByteOffset + (byte)Sl2FramePropertiesOffsets.KeelDepth), LinearDimensionUnit.Foot);

            //SpeedGps knots to meters per second
            frame.SpeedGps = GetFloat(reader, frameStartByteOffset + GetOffset(slType, "SpeedGps")) / 1.94385f;
            frame.Temperature = GetFloat(reader, frameStartByteOffset + GetOffset(slType, "Temperature"));

            var intLongitude = GetInt(reader, frameStartByteOffset + GetOffset(slType, "IntLongitude"));
            var intLatitude = GetInt(reader, frameStartByteOffset + GetOffset(slType, "IntLatitude"));
            frame.Point = new CoordinatePoint(new Latitude(ConvertLatitudeWGS84(intLatitude)),
                new Longitude(ConvertLongitudeToWGS84(intLongitude)));

            //WaterSpeed knots to meters per second
            frame.WaterSpeed = GetFloat(reader, frameStartByteOffset + GetOffset(slType, "WaterSpeed")) / 1.94385f;
            frame.CourseOverGround = GetFloat(reader, frameStartByteOffset + GetOffset(slType, "CourseOverGround"));
            frame.Altitude = new LinearDimension(GetFloat(reader, frameStartByteOffset + GetOffset(slType, "Altitude")), LinearDimensionUnit.Foot);
            frame.Heading = GetFloat(reader, frameStartByteOffset + GetOffset(slType, "Heading"));

            //read two bytes and parse flags
            var flagsBytes = GetBytes(reader, frameStartByteOffset + GetOffset(slType, "Flags"), 2);

            frame.Flags = new List<FrameFlags>();
            if (IsBitSet(flagsBytes[0], 0))
                frame.Flags.Add(FrameFlags.TrackValid);
            if (IsBitSet(flagsBytes[0], 1))
                frame.Flags.Add(FrameFlags.Preset_0_1);
            if (IsBitSet(flagsBytes[0], 2))
                frame.Flags.Add(FrameFlags.Unknown0_2);
            if (IsBitSet(flagsBytes[0], 3))
                frame.Flags.Add(FrameFlags.PositionValid);
            if (IsBitSet(flagsBytes[0], 4))
                frame.Flags.Add(FrameFlags.Unknown_0_4);
            if (IsBitSet(flagsBytes[0], 5))
                frame.Flags.Add(FrameFlags.CourseOrSpeed_0_5);
            if (IsBitSet(flagsBytes[0], 6))
                frame.Flags.Add(FrameFlags.SpeedValid);
            if (IsBitSet(flagsBytes[0], 7))
                frame.Flags.Add(FrameFlags.Preset_0_7);
            if (IsBitSet(flagsBytes[1], 0))
                frame.Flags.Add(FrameFlags.Unknown1_0);
            if (IsBitSet(flagsBytes[1], 1))
                frame.Flags.Add(FrameFlags.AltitudeOrCourseOrSpeed_1_1);
            if (IsBitSet(flagsBytes[1], 2))
                frame.Flags.Add(FrameFlags.Unknown1_2);
            if (IsBitSet(flagsBytes[1], 3))
                frame.Flags.Add(FrameFlags.Unknown1_3);
            if (IsBitSet(flagsBytes[1], 4))
                frame.Flags.Add(FrameFlags.Unknown1_4);
            if (IsBitSet(flagsBytes[1], 5))
                frame.Flags.Add(FrameFlags.Unknown1_5);
            if (IsBitSet(flagsBytes[1], 6))
                frame.Flags.Add(FrameFlags.AltitudeValid);
            if (IsBitSet(flagsBytes[1], 7))
                frame.Flags.Add(FrameFlags.HeadingValid);


            var bytesForSoundedData = GetBytes(reader, frameStartByteOffset + GetOffset(slType, "SoundedData"), frame.PacketSize);
            frame.SoundedData = new SoundedData(bytesForSoundedData, frame.ChannelType, upperLimit, lowerLimit);

            return frame;
        }

        /// <summary>
        /// Write sequence of frames to writer.
        /// </summary>
        /// <param name="writer"><see cref="BinaryWriter"/>.</param>
        /// <param name="framesToWrite">Sequence of frames to write.</param>
        /// <param name="framesSetStartByteOffset">Offset of first frame in Writers stream.</param>
        /// <param name="version">Outer stream version.</param>
        public static void WriteFrames(BinaryWriter writer, List<Frame> framesToWrite, int framesSetStartByteOffset, FileVersion version)
        {
            switch (version)
            {
                case FileVersion.SLG:
                case FileVersion.SL2:
                    WriteSl2Frames(writer, framesToWrite, framesSetStartByteOffset);
                    break;
                case FileVersion.SL3:
                    WriteSl3Frames(writer, framesToWrite, framesSetStartByteOffset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        /// <summary>
        /// Write sequence of SL3 frames to writer.
        /// </summary>
        /// <param name="writer"><see cref="BinaryWriter"/>.</param>
        /// <param name="framesToWrite">Sequence of frames to write.</param>
        /// <param name="framesSetStartByteOffset">Offset of first frame in Writers stream.</param>
        private static void WriteSl3Frames(BinaryWriter writer, List<Frame> framesToWrite, int framesSetStartByteOffset)
        {
            //cut 10 and 11 channels because don't know how to write them
            framesToWrite = framesToWrite.Where(frame => frame.ChannelType != (ChannelType) 7
                                                        && frame.ChannelType != (ChannelType) 8
                                                        && frame.ChannelType != (ChannelType) 10
                                                        && frame.ChannelType != (ChannelType) 11).ToList();

            //sort before writing
            framesToWrite.Sort();

            int frameOffset = framesSetStartByteOffset;

            int lastPrimaryChannelFrameOffset = 0;
            int lastSecondaryChannelFrameOffset = 0;
            int lastDownScanChannelFrameOffset = 0;
            int lastSidescanLeftChannelFrameOffset = 0;
            int lastSidescanRightChannelFrameOffset = 0;
            int lastSidescanCompositeChannelFrameOffset = 0;
            int lastThreeDChannelFrameOffset = 0;

            //take earliest date from frames and start writing from it 
            var firstFrameTime = framesToWrite.Select(frame => frame.DateTimeOffset).Min();
            int lastFrameIndex = 0;
            int timeOffsetMiliseconds = 0;

            short previousFrameSize = 0;

            foreach (var frame in framesToWrite)
            {
                //calculate new frame size
                var newFrameSize = (short)(frame.PacketSize + 168);

                //write frameOffset
                writer.Write(frameOffset);

                //write zero in 4 bytes (offset 4)
                writer.Write(new int());
                writer.Write(newFrameSize);
                writer.Write(previousFrameSize);

                previousFrameSize = newFrameSize;

                writer.Write((short)frame.ChannelType);
                //write zero in 2 bytes (offset 14-16)
                writer.Write(new short());
                writer.Write(frame.FrameIndex);
                writer.Write((float)frame.SoundedData.UpperLimit.GetFoots());
                writer.Write((float)frame.SoundedData.LowerLimit.GetFoots());
                //write zero in 16 bytes (offset 28 to 40)
                writer.Write(new long());
                writer.Write(new int());

                //if it's first frame then write file creation time = earliest time from all the frames
                if (frameOffset == framesSetStartByteOffset)
                    writer.Write((int)firstFrameTime.ToUnixTimeSeconds());
                else
                {
                    //increase offset value
                    if (lastFrameIndex != frame.FrameIndex)
                    {
                        lastFrameIndex = frame.FrameIndex;
                        //add 150ms for the frame time offset
                        timeOffsetMiliseconds += 150;
                    }
                    //write time offset
                    writer.Write(timeOffsetMiliseconds);
                }

                writer.Write(frame.PacketSize);
                //write zero in 2 bytes (offset 46 to 48)
                writer.Write(new short());
                writer.Write((float)frame.Depth.GetFoots());
                writer.Write((byte)frame.Frequency);
                //write zero in 31 bytes (offset 53 to 84)
                writer.Write(new long());
                writer.Write(new long());
                writer.Write(new long());
                writer.Write(new int());
                writer.Write(new short());
                writer.Write(new byte());
                writer.Write(frame.SpeedGps * 1.94385f);
                writer.Write(frame.Temperature);
                writer.Write(ConvertLongitudeToLowranceInt(frame.Point.Longitude.ToDegrees()));
                writer.Write(ConvertLatitudeToLowranceInt(frame.Point.Latitude.ToDegrees()));
                writer.Write(frame.WaterSpeed * 1.94385f);
                writer.Write(frame.CourseOverGround);
                writer.Write((float)frame.Altitude.GetFoots());
                writer.Write(frame.Heading);

                //write flags
                var twoFlagsBytes = TwoFlagsBytes(frame);
                writer.Write(twoFlagsBytes);

                //write zero in 6 bytes (offset 118 to 124)
                writer.Write(new short());
                writer.Write(new int());
                writer.Write(timeOffsetMiliseconds);

                switch (frame.ChannelType)
                {
                    case ChannelType.Primary:
                        lastPrimaryChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.Secondary:
                        lastSecondaryChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.DownScan:
                        lastDownScanChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.SidescanLeft:
                        lastSidescanLeftChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.SidescanRight:
                        lastSidescanRightChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.SidescanComposite:
                        lastSidescanCompositeChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.ThreeD:
                        lastThreeDChannelFrameOffset = frameOffset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                writer.Write(lastPrimaryChannelFrameOffset);
                writer.Write(lastSecondaryChannelFrameOffset);
                writer.Write(lastDownScanChannelFrameOffset);
                writer.Write(lastSidescanLeftChannelFrameOffset);
                writer.Write(lastSidescanRightChannelFrameOffset);
                writer.Write(lastSidescanCompositeChannelFrameOffset);
                //write zero in 12 bytes (offset 152 to 164)
                writer.Write(new long());
                writer.Write(new int());
                writer.Write(lastThreeDChannelFrameOffset);
                writer.Write(frame.SoundedData.Data);

                frameOffset += newFrameSize;
            }

        }

        /// <summary>
        /// Write sequence of SL2 frames to writer.
        /// </summary>
        /// <param name="writer"><see cref="BinaryWriter"/>.</param>
        /// <param name="framesToWrite">Sequence of frames to write.</param>
        /// <param name="framesSetStartByteOffset">Offset of first frame in Writers stream.</param>
        private static void WriteSl2Frames(BinaryWriter writer, List<Frame> framesToWrite, int framesSetStartByteOffset)
        {
            //cut 7 and 8 channels because don't know how to write them
            framesToWrite = framesToWrite.Where(frame => frame.ChannelType != ChannelType.ThreeD 
                                                         && frame.ChannelType != (ChannelType)7
                                                         && frame.ChannelType != (ChannelType)8
                                                         && frame.ChannelType != (ChannelType)10
                                                         && frame.ChannelType != (ChannelType)11).ToList();

            //sort before writing
            framesToWrite.Sort();

            int frameOffset = framesSetStartByteOffset;
            int lastPrimaryChannelFrameOffset = 0;
            int lastSecondaryChannelFrameOffset = 0;
            int lastDownScanChannelFrameOffset = 0;
            int lastSidescanLeftChannelFrameOffset = 0;
            int lastSidescanRightChannelFrameOffset = 0;
            int lastSidescanCompositeChannelFrameOffset = 0;

            //take earlies date from frames and start writing from it 
            var firstFrameTime = framesToWrite.Select(frame => frame.DateTimeOffset).Min();
            int lastFrameIndex = 0;
            int timeOffsetMiliseconds = 0;

            short previousFrameSize = 0;

            foreach (var frame in framesToWrite)
            {
                //calculate new frame size
                var newFrameSize = (short)(frame.PacketSize + 144);

                //write frameOffset
                writer.Write(frameOffset);

                switch (frame.ChannelType)
                {
                    case ChannelType.Primary:
                        lastPrimaryChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.Secondary:
                        lastSecondaryChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.DownScan:
                        lastDownScanChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.SidescanLeft:
                        lastSidescanLeftChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.SidescanRight:
                        lastSidescanRightChannelFrameOffset = frameOffset;
                        break;
                    case ChannelType.SidescanComposite:
                        lastSidescanCompositeChannelFrameOffset = frameOffset;
                        break;
                    //sl2 can't contains ThreeD channel data
                    case ChannelType.ThreeD:
                        throw new ArgumentOutOfRangeException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                //lastchannels frame offsets
                writer.Write(lastPrimaryChannelFrameOffset);
                writer.Write(lastSecondaryChannelFrameOffset);
                writer.Write(lastDownScanChannelFrameOffset);
                writer.Write(lastSidescanLeftChannelFrameOffset);
                writer.Write(lastSidescanRightChannelFrameOffset);
                writer.Write(lastSidescanCompositeChannelFrameOffset);
                //write frame size
                writer.Write(newFrameSize);
                writer.Write(previousFrameSize);

                previousFrameSize = newFrameSize;

                writer.Write((short)frame.ChannelType);
                writer.Write(frame.PacketSize);
                writer.Write(frame.FrameIndex);
                writer.Write((float)frame.SoundedData.UpperLimit.GetFoots());
                writer.Write((float)frame.SoundedData.LowerLimit.GetFoots());
                //write zero in two bytes (offset 48)
                writer.Write(new short());
                writer.Write((byte)frame.Frequency);
                //write zero in 13 bytes (offset 51)
                writer.Write(new long());
                writer.Write(new byte());

                //if it's first frame then write file creation time = earliest time from all the frames
                if (frameOffset == framesSetStartByteOffset)
                    writer.Write((int)firstFrameTime.ToUnixTimeSeconds());
                else
                {
                    //increase offset value
                    if (lastFrameIndex != frame.FrameIndex)
                    {
                        lastFrameIndex = frame.FrameIndex;
                        //add 150ms for the frame time offset
                        timeOffsetMiliseconds += 150;
                    }
                    //write time offset
                    writer.Write(timeOffsetMiliseconds);
                }

                writer.Write((float)frame.Depth.GetFoots());

                if (frame.KeelDepth != null)
                    writer.Write((float)frame.KeelDepth.GetFoots());
                else
                    writer.Write((float)0);

                //write zeros in 28 bytes (from offset 72 to 100)
                writer.Write(new long());
                writer.Write(new long());
                writer.Write(new long());
                writer.Write(new int());
                writer.Write(frame.SpeedGps * 1.94385f);
                writer.Write(frame.Temperature);
                writer.Write(ConvertLongitudeToLowranceInt(frame.Point.Longitude.ToDegrees()));
                writer.Write(ConvertLatitudeToLowranceInt(frame.Point.Latitude.ToDegrees()));
                writer.Write(frame.WaterSpeed * 1.94385f);
                writer.Write(frame.CourseOverGround);
                writer.Write((float)frame.Altitude.GetFoots());
                writer.Write(frame.Heading);

                //write flags
                var twoFlagsBytes = TwoFlagsBytes(frame);
                writer.Write(twoFlagsBytes);

                //write zeros in 6 bytes (from offset 134 to 140)
                writer.Write(new short());
                writer.Write(new short());
                writer.Write(new short());

                //write time offset
                writer.Write(timeOffsetMiliseconds);

                //write Sounded Data
                writer.Write(frame.SoundedData.Data);

                //add newFrameSize to frameOffset at the end
                frameOffset += newFrameSize;
            }
        }

        /// <summary>
        /// Converts Frames flags to two bytes.
        /// </summary>
        /// <param name="frame"><see cref="Frame"/>.</param>
        /// <returns>Array of two bytes.</returns>
        private static byte[] TwoFlagsBytes(Frame frame)
        {
            var twoFlagsBytes = new byte[2];
            //sets bits in two bytes
            if (frame.Flags != null)
            {
                foreach (var frameFlag in frame.Flags)
                {
                    switch (frameFlag)
                    {
                        case FrameFlags.TrackValid:
                            SetBitInByte(ref twoFlagsBytes[0], 0);
                            break;
                        case FrameFlags.Preset_0_1:
                            SetBitInByte(ref twoFlagsBytes[0], 1);
                            break;
                        case FrameFlags.Unknown0_2:
                            SetBitInByte(ref twoFlagsBytes[0], 2);
                            break;
                        case FrameFlags.PositionValid:
                            SetBitInByte(ref twoFlagsBytes[0], 3);
                            break;
                        case FrameFlags.Unknown_0_4:
                            SetBitInByte(ref twoFlagsBytes[0], 4);
                            break;
                        case FrameFlags.CourseOrSpeed_0_5:
                            SetBitInByte(ref twoFlagsBytes[0], 5);
                            break;
                        case FrameFlags.SpeedValid:
                            SetBitInByte(ref twoFlagsBytes[0], 6);
                            break;
                        case FrameFlags.Preset_0_7:
                            SetBitInByte(ref twoFlagsBytes[0], 7);
                            break;
                        case FrameFlags.Unknown1_0:
                            SetBitInByte(ref twoFlagsBytes[1], 0);
                            break;
                        case FrameFlags.AltitudeOrCourseOrSpeed_1_1:
                            SetBitInByte(ref twoFlagsBytes[1], 1);
                            break;
                        case FrameFlags.Unknown1_2:
                            SetBitInByte(ref twoFlagsBytes[1], 2);
                            break;
                        case FrameFlags.Unknown1_3:
                            SetBitInByte(ref twoFlagsBytes[1], 3);
                            break;
                        case FrameFlags.Unknown1_4:
                            SetBitInByte(ref twoFlagsBytes[1], 4);
                            break;
                        case FrameFlags.Unknown1_5:
                            SetBitInByte(ref twoFlagsBytes[1], 5);
                            break;
                        case FrameFlags.AltitudeValid:
                            SetBitInByte(ref twoFlagsBytes[1], 6);
                            break;
                        case FrameFlags.HeadingValid:
                            SetBitInByte(ref twoFlagsBytes[1], 7);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            return twoFlagsBytes;
        }

        /// <summary>
        /// Gets offset map type for file version.
        /// </summary>
        /// <param name="version"><see cref="SonarLogAPI.Lowrance.FileVersion"/>.</param>
        /// <returns>Type of offset map.</returns>
        private static Type GetOffsetsTypeForFileVersion(FileVersion version)
        {
            Type slType;

            switch (version)
            {
                case FileVersion.SLG:
                    throw new NotImplementedException();
                //break;
                case FileVersion.SL2:
                    slType = typeof(Sl2FramePropertiesOffsets);
                    break;
                case FileVersion.SL3:
                    slType = typeof(Sl3FramePropertiesOffsets);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }

            return slType;
        }

        /// <summary>
        /// Parse property to enum and convert it to byte.
        /// </summary>
        /// <param name="slType">Type of enum.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Property byte representation.</returns>
        private static byte GetOffset(Type slType, string propertyName)
        {
            return Convert.ToByte(Enum.Parse(slType, propertyName));
        }

        /// <summary>
        /// Converts Longitude double degrees value in WGS84 to int value in Lowrance format.
        /// </summary>
        /// <param name="longitudeWGS84">Longitude double degrees value in WGS84.</param>
        /// <returns>Longitude int value in Lowrance format.</returns>
        private static int ConvertLongitudeToLowranceInt(double longitudeWGS84)
        {
            return Convert.ToInt32(longitudeWGS84 * _earthWGS84PolarRadius / _radConversion);
        }

        /// <summary>
        /// Converts Longitude int value in Lowrance format to double degrees value in WGS84 format.
        /// </summary>
        /// <param name="lowranceIntValue">Longitude int value in Lowrance format.</param>
        /// <returns>Double degrees value in WGS84 format.</returns>
        private static double ConvertLongitudeToWGS84(int lowranceIntValue)
        {
            return lowranceIntValue / _earthWGS84PolarRadius * _radConversion;
        }

        /// <summary>
        /// Converts <see cref="Latitude"/> double degrees value in WGS84 to int value in Lowrance(Spherical Mercator Projection) format.
        /// </summary>
        /// <param name="latitudeWGS84">Latitude double degrees value in WGS84.</param>
        /// <returns>Latitude int value in Lowrance format.</returns>
        private static int ConvertLatitudeToLowranceInt(double latitudeWGS84)
        {
            var temp = latitudeWGS84 / _radConversion;
            temp = Math.Log(Math.Tan(temp / 2 + Math.PI / 4));
            return Convert.ToInt32(temp * _earthWGS84PolarRadius);
        }

        private static double ConvertLatitudeWGS84(int lowranceIntValue)
        {
            var temp = lowranceIntValue / _earthWGS84PolarRadius;
            temp = Math.Exp(temp);
            temp = 2 * Math.Atan(temp) - Math.PI / 2;
            return temp * _radConversion;
        }

        /// <summary>
        /// Sets specific bit in ref byte.
        /// </summary>
        /// <param name="refByte">Byte ref.</param>
        /// <param name="position">Bit position.</param>
        private static void SetBitInByte(ref byte refByte, int position)
        {
            if (position > 7) throw new ArgumentOutOfRangeException(nameof(position));

            var mask = (byte)(1 << position);
            // set to 1
            refByte |= mask;
        }

        /// <summary>
        /// Checks specific bit in byte set.
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="position">Bit position.</param>
        /// <returns>Is bit set or not.</returns>
        private static bool IsBitSet(byte b, int position)
        {
            if (position > 7) throw new ArgumentOutOfRangeException(nameof(position));
            return (b & (1 << position)) != 0;
        }

        private static byte[] GetBytes(BinaryReader reader, long offset, int count)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return reader.ReadBytes(count);
        }

        private static short GetShort(BinaryReader reader, long offset)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return reader.ReadInt16();
        }

        private static int GetInt(BinaryReader reader, long offset)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return reader.ReadInt32();
        }

        private static float GetFloat(BinaryReader reader, long offset)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return GetFloat(reader.ReadBytes(4));
        }

        private static long GetLong(BinaryReader reader, long offset)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return reader.ReadInt64();
        }

        private static float GetFloat(byte[] bytes)
        {
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Generate frame based on frame from the base channel.
        /// </summary>
        /// <param name="requiredChannelType"><see cref="ChannelType"/> for a new(generated) frame.</param>
        /// <param name="sourceFrame"></param>
        /// <param name="generateSoundedData">Generate sounded data. By default:false(take from source frame).</param>
        /// <returns>New(generated) frame.</returns>
        public static Frame GenerateFromOtherChannelFrame(ChannelType requiredChannelType, Frame sourceFrame, bool generateSoundedData = false)
        {
            if (requiredChannelType == sourceFrame.ChannelType)
                throw new ArgumentException("requiredChannelType the same as source frame channel", nameof(requiredChannelType));

            if (requiredChannelType == ChannelType.ThreeD)
                throw new NotImplementedException("Cant generate 3D channel frame.");

            var frame = new Frame
            {
                Altitude = sourceFrame.Altitude,
                ChannelType = requiredChannelType,
                CourseOverGround = sourceFrame.CourseOverGround,
                Depth = sourceFrame.Depth,
                Flags = sourceFrame.Flags,
                FrameIndex = sourceFrame.FrameIndex,
                Frequency = Frequency.Frequency_200KHz,
                SpeedGps = sourceFrame.SpeedGps,
                Heading = sourceFrame.Heading,
                KeelDepth = sourceFrame.KeelDepth,
                Point = sourceFrame.Point,
                Temperature = sourceFrame.Temperature,
                DateTimeOffset = sourceFrame.DateTimeOffset,
                WaterSpeed = sourceFrame.WaterSpeed,

            };

            switch (requiredChannelType)
            {
                case ChannelType.Primary:
                case ChannelType.Secondary:
                case ChannelType.DownScan:
                case ChannelType.SidescanRight:
                    #region source channel switch
                    //if sounded data needs to be generated
                    if (generateSoundedData)
                    {
                        frame.PacketSize = 1440;

                        //lower limit calc from depth value
                        var dptInMeters = frame.Depth.GetMeters();
                        var lowerLimit = LinearDimension.FromMeters(Math.Ceiling(dptInMeters / 5) * 5 + 1);

                        frame.SoundedData = SoundedData.GenerateData(frame.PacketSize, requiredChannelType,
                            frame.Depth, LinearDimension.FromMeters(0), lowerLimit);
                    }

                    switch (sourceFrame.ChannelType)
                    {
                        case ChannelType.Primary:
                        case ChannelType.Secondary:
                        case ChannelType.DownScan:
                        case ChannelType.SidescanRight:

                            if (!generateSoundedData)
                            {
                                frame.PacketSize = sourceFrame.PacketSize;
                                frame.SoundedData = sourceFrame.SoundedData;
                            }

                            break;
                        case ChannelType.SidescanLeft:

                            if (!generateSoundedData)
                            {
                                frame.PacketSize = sourceFrame.PacketSize;
                                var upperLimit = LinearDimension.FromMeters(0);
                                var lowerLimit = sourceFrame.SoundedData.UpperLimit * -1;
                                frame.SoundedData = new SoundedData(sourceFrame.SoundedData.Data.Reverse().ToArray(), requiredChannelType,
                                    upperLimit, lowerLimit);
                            }

                            break;

                        case ChannelType.SidescanComposite:

                            if (!generateSoundedData)
                            {
                                frame.PacketSize = (short)(sourceFrame.PacketSize / 2);
                                var upperLimit = LinearDimension.FromMeters(0);
                                var lowerLimit = sourceFrame.SoundedData.LowerLimit;

                                //array for right side
                                var sidescanRight = new byte[frame.PacketSize];

                                //copy right side to array
                                sourceFrame.SoundedData.Data.Where((b, i) => i >= frame.PacketSize).ToArray().CopyTo(sidescanRight, 0);

                                //create SoundedData
                                frame.SoundedData = new SoundedData(sidescanRight, requiredChannelType,
                                    upperLimit, lowerLimit);
                            }

                            break;
                        case ChannelType.ThreeD:

                            if (!generateSoundedData)
                                throw new NotImplementedException("Can't take sounded data from 3D channel");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    #endregion
                    break;
                case ChannelType.SidescanLeft:
                    #region source channel switch

                    //if sounded data needs to be generated
                    if (generateSoundedData)
                    {
                        frame.PacketSize = 1440;
                        var lowerLimit = LinearDimension.FromMeters(0);

                        //UpperLimit calc from depth value
                        var dptInMeters = frame.Depth.GetMeters();
                        var upperLimit = LinearDimension.FromMeters(Math.Ceiling(dptInMeters / 5) * 5 + 1) * -1;

                        frame.SoundedData = SoundedData.GenerateData(frame.PacketSize, requiredChannelType,
                            frame.Depth, upperLimit, lowerLimit);

                    }

                    switch (sourceFrame.ChannelType)
                    {
                        case ChannelType.Primary:
                        case ChannelType.Secondary:
                        case ChannelType.DownScan:
                        case ChannelType.SidescanRight:
                            //if sounded data needs to be generated
                            if (!generateSoundedData)
                            {
                                frame.PacketSize = sourceFrame.PacketSize;
                                var upperLimit = sourceFrame.SoundedData.LowerLimit * -1;
                                var lowerLimit = LinearDimension.FromMeters(0);
                                frame.SoundedData = new SoundedData(sourceFrame.SoundedData.Data.Reverse().ToArray(), requiredChannelType,
                                    upperLimit, lowerLimit);
                            }
                            break;
                        case ChannelType.SidescanLeft:
                            if (!generateSoundedData)
                            {
                                frame.PacketSize = sourceFrame.PacketSize;
                                frame.SoundedData = sourceFrame.SoundedData;
                            }
                            break;

                        case ChannelType.SidescanComposite:

                            if (!generateSoundedData)
                            {
                                frame.PacketSize = (short)(sourceFrame.PacketSize / 2);
                                var upperLimit = sourceFrame.SoundedData.UpperLimit;
                                var lowerLimit = LinearDimension.FromMeters(0);

                                //array for left side
                                var sidescanLeft = new byte[frame.PacketSize];

                                //copy left side to array
                                sourceFrame.SoundedData.Data.Where((b, i) => i < frame.PacketSize).ToArray().CopyTo(sidescanLeft, 0);

                                //create SoundedData
                                frame.SoundedData = new SoundedData(sidescanLeft, requiredChannelType,
                                    upperLimit, lowerLimit);
                            }

                            break;
                        case ChannelType.ThreeD:
                            if (!generateSoundedData)
                                throw new NotImplementedException("Can't take sounded data from 3D channel");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    #endregion
                    break;
                case ChannelType.SidescanComposite:
                    #region source channel switch

                    //if sounded data needs to be generated
                    if (generateSoundedData)
                    {
                        frame.PacketSize = 2880;

                        frame.SoundedData = SoundedData.GenerateData(frame.PacketSize, requiredChannelType,
                            frame.Depth, LinearDimension.FromMeters(-48), LinearDimension.FromMeters(48));
                    }

                    switch (sourceFrame.ChannelType)
                    {
                        case ChannelType.Primary:
                        case ChannelType.Secondary:
                        case ChannelType.DownScan:
                        case ChannelType.SidescanRight:
                            if (!generateSoundedData)
                            {
                                frame.PacketSize = (short)(sourceFrame.PacketSize * 2);
                                var sidescanCompositeArray = new byte[frame.PacketSize];
                                sourceFrame.SoundedData.Data.Reverse().ToArray().CopyTo(sidescanCompositeArray, 0);
                                sourceFrame.SoundedData.Data.CopyTo(sidescanCompositeArray, sourceFrame.PacketSize);
                                frame.SoundedData = new SoundedData(sidescanCompositeArray, requiredChannelType,
                                    sourceFrame.SoundedData.LowerLimit * -1, sourceFrame.SoundedData.LowerLimit);

                            }
                            break;

                        case ChannelType.SidescanLeft:
                            if (!generateSoundedData)
                            {
                                frame.PacketSize = (short)(sourceFrame.PacketSize * 2);
                                var sidescanCompositeArray = new byte[frame.PacketSize];

                                sourceFrame.SoundedData.Data.CopyTo(sidescanCompositeArray, 0);
                                sourceFrame.SoundedData.Data.Reverse().ToArray().CopyTo(sidescanCompositeArray, sourceFrame.PacketSize);

                                frame.SoundedData = new SoundedData(sidescanCompositeArray, requiredChannelType,
                                    sourceFrame.SoundedData.UpperLimit, sourceFrame.SoundedData.UpperLimit * -1);
                            }

                            break;
                        case ChannelType.SidescanComposite:
                            if (!generateSoundedData)
                            {
                                frame.PacketSize = sourceFrame.PacketSize;
                                frame.SoundedData = sourceFrame.SoundedData;
                            }
                            break;
                        case ChannelType.ThreeD:
                            if (!generateSoundedData)
                                throw new NotImplementedException("Can't take sounded data from 3D channel");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    #endregion
                    break;
                case ChannelType.ThreeD:
                    throw new NotImplementedException("Can't generate " + requiredChannelType + " frame.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(requiredChannelType), requiredChannelType, null);
            }
            return frame;
        }

        public override string ToString()
        {
            return $"#{FrameIndex}, Dpt. {Depth}, {ChannelType}";
        }

        public int CompareTo(Frame otherFrame)
        {
            if (FrameIndex > otherFrame.FrameIndex) return 1;
            if (FrameIndex < otherFrame.FrameIndex) return -1;

            //if FrameIndex are equal, lets compare ChannelType
            if (ChannelType > otherFrame.ChannelType) return 1;
            if (ChannelType < otherFrame.ChannelType) return -1;

            return 0;
        }
    }
}
