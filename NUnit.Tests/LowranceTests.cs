namespace NUnit.Tests
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using NUnit.Framework;

	using SonarLogAPI.Localization;
	using SonarLogAPI.Lowrance;

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class HeaderTests
	{
		[Test(TestOf = typeof(Header))]

		//read header from first file, write it to second, read from second and compare with readed from first file
		public void ReadandWriteHeader()
		{
			Header firstHeader;
			Header secondHeader;

			var testDir = TestContext.CurrentContext.TestDirectory;
			var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory));

			var fromStream = new FileStream
				(solutionDir + @"\input.sl2", FileMode.Open, FileAccess.Read);

			using (var reader = new BinaryReader(fromStream))
			{
				firstHeader = Header.ReadHeader(reader, 0);
			}

			fromStream.Close();
			fromStream.Dispose();

			var toStream = new FileStream
				(testDir + @"\toHeader.sl2", FileMode.Create, FileAccess.Write);

			using (var writer = new BinaryWriter(toStream))
			{
				Header.WriteHeader(writer, firstHeader,0);
			}

			toStream.Close();
			toStream.Dispose();

			var fromStream2 = new FileStream
				(testDir + @"\toHeader.sl2", FileMode.Open, FileAccess.Read);

			using (var reader = new BinaryReader(fromStream2))
			{
				secondHeader = Header.ReadHeader(reader, 0);
			}

			fromStream2.Close();
			fromStream2.Dispose();

			Assert.AreEqual(firstHeader.FileVersion, secondHeader.FileVersion);
			Assert.AreEqual(firstHeader.HardwareVersion, secondHeader.HardwareVersion);
			Assert.AreEqual(firstHeader.BlockSize, secondHeader.BlockSize);
		}
		
	}

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class FramesTests
	{
		[Test(TestOf = typeof(Frame))]
		public void ReadAndWriteSL2Frames()
		{
			//read frames from first file, write it to second, read from second and compare with readed from first file
			var firstFrameList = new List<Frame>();
			var secondFrameList = new List<Frame>();

			var testDir = TestContext.CurrentContext.TestDirectory;
			var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory));

			var fromStream = new FileStream
				(solutionDir + @"\input.sl2", FileMode.Open, FileAccess.Read);
			
			using (var reader = new BinaryReader(fromStream))
			{
				var map = Frame.GetFrameMap(reader, Header.Lenght, FileVersion.SL2).ToArray();
				firstFrameList.Add(Frame.ReadFrame(reader, map[0], FileVersion.SL2));
				firstFrameList.Add(Frame.ReadFrame(reader, map[1], FileVersion.SL2));
				firstFrameList.Add(Frame.ReadFrame(reader, map[2], FileVersion.SL2));
			}

			fromStream.Close();
			fromStream.Dispose();

			var toStream = new FileStream
				(testDir + @"\toFrame.sl2", FileMode.Create, FileAccess.Write);

			using (var writer = new BinaryWriter(toStream))
			{
				//header
				writer.Write(new double());
				Frame.WriteFrames(writer, firstFrameList, Header.Lenght, FileVersion.SL2);
			}

			toStream.Close();
			toStream.Dispose();

			var fromStream2 = new FileStream
				(testDir + @"\toFrame.sl2", FileMode.Open, FileAccess.Read);

			using (var reader = new BinaryReader(fromStream2))
			{
				var map = Frame.GetFrameMap(reader, Header.Lenght, FileVersion.SL2).ToArray();
				secondFrameList.Add(Frame.ReadFrame(reader, map[0], FileVersion.SL2));
				secondFrameList.Add(Frame.ReadFrame(reader, map[1], FileVersion.SL2));
				secondFrameList.Add(Frame.ReadFrame(reader, map[2], FileVersion.SL2));
			}

			fromStream2.Close();
			fromStream2.Dispose();

			//check Lists
			Assert.IsTrue(firstFrameList.Count == 3);
			Assert.IsTrue(secondFrameList.Count == 3);

			//check properties of firs frame
			Assert.AreEqual(firstFrameList[0].ChannelType, secondFrameList[0].ChannelType);
			Assert.AreEqual(firstFrameList[0].PacketSize, secondFrameList[0].PacketSize);
			Assert.AreEqual(firstFrameList[0].FrameIndex, secondFrameList[0].FrameIndex);
			Assert.AreEqual(firstFrameList[0].UpperLimit, secondFrameList[0].UpperLimit);
			Assert.AreEqual(firstFrameList[0].LowerLimit, secondFrameList[0].LowerLimit);
			Assert.AreEqual(firstFrameList[0].Frequency, secondFrameList[0].Frequency);
			Assert.AreEqual(firstFrameList[0].Frequency, secondFrameList[0].Frequency);
			Assert.AreEqual(firstFrameList[0].Depth, secondFrameList[0].Depth);
			Assert.AreEqual(firstFrameList[0].KeelDepth, secondFrameList[0].KeelDepth);
			Assert.AreEqual(firstFrameList[0].SpeedGps, secondFrameList[0].SpeedGps);
			Assert.AreEqual(firstFrameList[0].Temperature, secondFrameList[0].Temperature);
			Assert.AreEqual(firstFrameList[0].Temperature, secondFrameList[0].Temperature);
			Assert.AreEqual(firstFrameList[0].Point, secondFrameList[0].Point);
			Assert.AreEqual(firstFrameList[0].WaterSpeed, secondFrameList[0].WaterSpeed);
			Assert.AreEqual(firstFrameList[0].CourseOverGround, secondFrameList[0].CourseOverGround);
			Assert.AreEqual(firstFrameList[0].Altitude, secondFrameList[0].Altitude);
			Assert.AreEqual(firstFrameList[0].Heading, secondFrameList[0].Heading);
			Assert.AreEqual(firstFrameList[0].Flags.Count, secondFrameList[0].Flags.Count);
			//Assert.AreEqual(firstFrameList[0].TimeOffset.TotalMilliseconds, secondFrameList[0].TimeOffset.TotalMilliseconds);
			Assert.AreEqual(firstFrameList[0].SoundedData, secondFrameList[0].SoundedData);

			//check properties of second frame
			Assert.AreEqual(firstFrameList[1].ChannelType, secondFrameList[1].ChannelType);
			Assert.AreEqual(firstFrameList[1].PacketSize, secondFrameList[1].PacketSize);
			Assert.AreEqual(firstFrameList[1].FrameIndex, secondFrameList[1].FrameIndex);
			Assert.AreEqual(firstFrameList[1].UpperLimit, secondFrameList[1].UpperLimit);
			Assert.AreEqual(firstFrameList[1].LowerLimit, secondFrameList[1].LowerLimit);
			Assert.AreEqual(firstFrameList[1].Frequency, secondFrameList[1].Frequency);
			Assert.AreEqual(firstFrameList[1].Frequency, secondFrameList[1].Frequency);
			Assert.AreEqual(firstFrameList[1].Depth, secondFrameList[1].Depth);
			Assert.AreEqual(firstFrameList[1].KeelDepth, secondFrameList[1].KeelDepth);
			Assert.AreEqual(firstFrameList[1].SpeedGps, secondFrameList[1].SpeedGps);
			Assert.AreEqual(firstFrameList[1].Temperature, secondFrameList[1].Temperature);
			Assert.AreEqual(firstFrameList[1].Temperature, secondFrameList[1].Temperature);
			Assert.AreEqual(firstFrameList[1].Point, secondFrameList[1].Point);
			Assert.AreEqual(firstFrameList[1].WaterSpeed, secondFrameList[1].WaterSpeed);
			Assert.AreEqual(firstFrameList[1].CourseOverGround, secondFrameList[1].CourseOverGround);
			Assert.AreEqual(firstFrameList[1].Altitude, secondFrameList[1].Altitude);
			Assert.AreEqual(firstFrameList[1].Heading, secondFrameList[1].Heading);
			Assert.AreEqual(firstFrameList[1].Flags, secondFrameList[1].Flags);
			Assert.AreEqual(firstFrameList[1].SoundedData, secondFrameList[1].SoundedData);

		}

		[Test(TestOf = typeof(Frame))]
		public void ReadAndWriteSL3Frames()
		{
			//read frames from first file, write it to second, read from second and compare with readed from first file
			var firstFrameList = new List<Frame>();
			var secondFrameList = new List<Frame>();

			var testDir = TestContext.CurrentContext.TestDirectory;
			var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory));

			var fromStream = new FileStream
				(solutionDir + @"\input.sl3", FileMode.Open, FileAccess.Read);

			using (var reader = new BinaryReader(fromStream))
			{
				var map = Frame.GetFrameMap(reader, Header.Lenght, FileVersion.SL3).ToArray();
				firstFrameList.Add(Frame.ReadFrame(reader, map[0], FileVersion.SL3));
				firstFrameList.Add(Frame.ReadFrame(reader, map[1], FileVersion.SL3));
				firstFrameList.Add(Frame.ReadFrame(reader, map[2], FileVersion.SL3));
			}

			fromStream.Close();
			fromStream.Dispose();

			var toStream = new FileStream
				(testDir + @"\toFrame.sl3", FileMode.Create, FileAccess.Write);

			using (var writer = new BinaryWriter(toStream))
			{
				//header
				writer.Write(new double());
				Frame.WriteFrames(writer, firstFrameList, Header.Lenght, FileVersion.SL3);
			}

			toStream.Close();
			toStream.Dispose();

			var fromStream2 = new FileStream
				(testDir + @"\toFrame.sl3", FileMode.Open, FileAccess.Read);

			using (var reader = new BinaryReader(fromStream2))
			{
				var map = Frame.GetFrameMap(reader, Header.Lenght, FileVersion.SL3).ToArray();
				secondFrameList.Add(Frame.ReadFrame(reader, map[0], FileVersion.SL3));
				secondFrameList.Add(Frame.ReadFrame(reader, map[1], FileVersion.SL3));
				secondFrameList.Add(Frame.ReadFrame(reader, map[2], FileVersion.SL3));
			}

			fromStream2.Close();
			fromStream2.Dispose();

			//check Lists
			Assert.IsTrue(firstFrameList.Count == 3);
			Assert.IsTrue(secondFrameList.Count == 3);

			//check properties of firs frame
			Assert.AreEqual(firstFrameList[0].ChannelType, secondFrameList[0].ChannelType);
			Assert.AreEqual(firstFrameList[0].PacketSize, secondFrameList[0].PacketSize);
			Assert.AreEqual(firstFrameList[0].FrameIndex, secondFrameList[0].FrameIndex);
			Assert.AreEqual(firstFrameList[0].UpperLimit, secondFrameList[0].UpperLimit);
			Assert.AreEqual(firstFrameList[0].LowerLimit, secondFrameList[0].LowerLimit);
			Assert.AreEqual(firstFrameList[0].Frequency, secondFrameList[0].Frequency);
			Assert.AreEqual(firstFrameList[0].Frequency, secondFrameList[0].Frequency);
			Assert.AreEqual(firstFrameList[0].Depth, secondFrameList[0].Depth);
			Assert.AreEqual(firstFrameList[0].KeelDepth, secondFrameList[0].KeelDepth);
			Assert.AreEqual(firstFrameList[0].SpeedGps, secondFrameList[0].SpeedGps);
			Assert.AreEqual(firstFrameList[0].Temperature, secondFrameList[0].Temperature);
			Assert.AreEqual(firstFrameList[0].Temperature, secondFrameList[0].Temperature);
			Assert.AreEqual(firstFrameList[0].Point, secondFrameList[0].Point);
			Assert.AreEqual(firstFrameList[0].WaterSpeed, secondFrameList[0].WaterSpeed);
			Assert.AreEqual(firstFrameList[0].CourseOverGround, secondFrameList[0].CourseOverGround);
			Assert.AreEqual(firstFrameList[0].Altitude, secondFrameList[0].Altitude);
			Assert.AreEqual(firstFrameList[0].Heading, secondFrameList[0].Heading);
			//Assert.AreEqual(firstFrameList[0].Flags.Count, secondFrameList[0].Flags.Count);
			//Assert.AreEqual(firstFrameList[0].TimeOffset.TotalMilliseconds, secondFrameList[0].TimeOffset.TotalMilliseconds);
			Assert.AreEqual(firstFrameList[0].SoundedData, secondFrameList[0].SoundedData);

			//check properties of second frame
			Assert.AreEqual(firstFrameList[1].ChannelType, secondFrameList[1].ChannelType);
			Assert.AreEqual(firstFrameList[1].PacketSize, secondFrameList[1].PacketSize);
			Assert.AreEqual(firstFrameList[1].FrameIndex, secondFrameList[1].FrameIndex);
			Assert.AreEqual(firstFrameList[1].UpperLimit, secondFrameList[1].UpperLimit);
			Assert.AreEqual(firstFrameList[1].LowerLimit, secondFrameList[1].LowerLimit);
			Assert.AreEqual(firstFrameList[1].Frequency, secondFrameList[1].Frequency);
			Assert.AreEqual(firstFrameList[1].Frequency, secondFrameList[1].Frequency);
			Assert.AreEqual(firstFrameList[1].Depth, secondFrameList[1].Depth);
			Assert.AreEqual(firstFrameList[1].KeelDepth, secondFrameList[1].KeelDepth);
			Assert.AreEqual(firstFrameList[1].SpeedGps, secondFrameList[1].SpeedGps);
			Assert.AreEqual(firstFrameList[1].Temperature, secondFrameList[1].Temperature);
			Assert.AreEqual(firstFrameList[1].Temperature, secondFrameList[1].Temperature);
			Assert.AreEqual(firstFrameList[1].Point, secondFrameList[1].Point);
			Assert.AreEqual(firstFrameList[1].WaterSpeed, secondFrameList[1].WaterSpeed);
			Assert.AreEqual(firstFrameList[1].CourseOverGround, secondFrameList[1].CourseOverGround);
			Assert.AreEqual(firstFrameList[1].Altitude, secondFrameList[1].Altitude);
			Assert.AreEqual(firstFrameList[1].Heading, secondFrameList[1].Heading);
			//Assert.AreEqual(firstFrameList[1].Flags, secondFrameList[1].Flags);
			Assert.AreEqual(firstFrameList[1].SoundedData, secondFrameList[1].SoundedData);
		}
	}
}
