namespace ConsoleLogConverter
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;

	using CommandLine;
	using CommandLine.Text;

	using SonarLogAPI;
	using SonarLogAPI.CSV;
	using SonarLogAPI.Lowrance;
	using SonarLogAPI.Primitives;

	public class Program
	{
		class Options
		{
			[Option('i', "input", Required = true,
				 HelpText = "Input file name to be processed.")]
			public string InputFile { get; set; }

			[Option('s', "search", Required = false, DefaultValue = -1,
				 HelpText = "Enables research mode. Research value at specified byte offset inside frame will be printed to console.")]
			public int SearchOffset { get; set; }

			[Option('d', "dpajust", Required = false,
				HelpText = "Input filename for depth adjust")]
			public string DepthAdjustFile { get; set; }

			[OptionList('o', "output", Separator = ':',
				 HelpText = "Enables convertion mode. Output file version. sl2:sl3:csv")]
			public IList<string> OutputFileVersion { get; set; }

			[OptionList('c', "channel", Separator = ':', Required = false,
				 HelpText = "Channels, included in output file. Format: channel numbers, separated by colon. By default: all channels. Primary = 0, Secondary = 1, DownScan = 2, SidescanLeft = 3, SidescanRight = 4, SidescanComposite = 5, ThreeD = 9")]
			public IList<string> Channels { get; set; }

			[Option('f', "from", DefaultValue = uint.MinValue,
				 HelpText = "Get frames From specifieg number")]
			public uint FramesFrom { get; set; }

			[Option('t', "to", DefaultValue = uint.MaxValue,
				 HelpText = "Get frames To specifieg number")]
			public uint FramesTo { get; set; }

			[Option('a', "anonymous", Required = false, DefaultValue = false,
				 HelpText = "Makes output file GPS coordinates anonymous. Sets Latitude and Longitude to zero.")]
			public bool CoordinatesDelete { get; set; }

			[Option('v', "verbose", DefaultValue = true,
				 HelpText = "Prints all messages to standard output.")]
			public bool Verbose { get; set; }

			[Option('h', "dshift", Required = false,
				 HelpText = "Depth value for adding or subtraction to the depth values in the log.")]
			public string DepthShift { get; set; }

			[OptionList('g', "generate", Separator = ':', Required = false,
				 HelpText = "Generate frames for specific channel from the frames at other channel(s). Format: numbers and chars, " +
							"separated by colon. " +
							"Options: numbers - destionation frame channel and source channel(s); " +
				            "d - if specified generate sounded data from source frame depth value,(otherwise take sounded " +
							"data from source frame); f - if specified generate frame in destination channel even if frame with " +
				            "such coordinates already exist." +
				            "Examples: \"-g 0:2:\" - find in the 2th channel(DownScan) frames with coordinates " +
							"that are absent in the 0th channel(Primary) and generate frames with such coordinates in the 0th " +
							"channel. \"-g 1:2:5:f\" - get frames from 2th(DownScan) and 5th (SidescanComposite) channels and " +
							"group em by unique coordinates. After that generate 1th(Secondary) channel frames (even frames with " +
							"coordinates from 2th and 5th chanel are in 1th(\"f\" option)). \"-g 0:5:d\" - get frames from 5th " +
				            "(SidescanComposite) channel and generate frames (with generatend by depth(\"d\" option) SoundedData) for 0th(Primary) " +
				            "channel if they are not allready exist.")]
			public IList<string> GenegateParams { get; set; }

			[HelpOption]
			public string GetUsage()
			{
				var text = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
				text.AddPostOptionsLine("EXAMPLES:\n");
				text.AddPostOptionsLine("\"ConsoleLogConverter.exe -i input.sl3 -s 30 -t 10 -c 0:2\"\n");
				text.AddPostOptionsLine("Command takes all frames from input.sl3. At the next step it takes frames from channels 0 and 2 with frame index from 0 to 10. " +
										"An finally it takes four bytes at 30 offset from each frame start and represent them as differet types of value(string, single bytes, short from first two bytes, short from second two bytes, integer, float).\n\n");
				text.AddPostOptionsLine("\"ConsoleLogConverter.exe -i input.sl2 -f 10 -t 509 -c 0 -a -o sl2:csv\"\n");
				text.AddPostOptionsLine("Command takes all frames from input.sl2. At the next step it takes frames from channel 0 with frame index from 10 to 509 and delete GPS coordinates from it . And finally it save frames to two files with \"sl2\" and \"csv\" format.\n\n");
				text.AddPostOptionsLine("\"ConsoleLogConverter.exe -i BaseDepthPoints.sl2 -d pointsForAdjust.sl2 -o csv\"\n");
				text.AddPostOptionsLine("Command takes all frames from BaseDepthPoints.sl2 and pointsForAdjust.sl2 files. At the next step it finds nearest points at two sequences and calculate depth difference between em. After that it add difference to each pointsForAdjust.sl2 frame. Finally it contact two sequences and save frames to file with \"csv\" format.\n\n");
				text.AddPostOptionsLine("\"ConsoleLogConverter.exe -i input.sl2 -h m1.15 -o csv\"\n");
				text.AddPostOptionsLine("Command takes all frames from input.sl2. At the next step it subtract (use \"m\"(minus) prefix to substract and \"p\"(plus) to add value) 1.15 meters from depth value at each frame and save frames to \"csv\" format.\n\n");
				text.AddPostOptionsLine("\"ConsoleLogConverter.exe -i input.sl2 -g 0:5:f -o sl2\"\n");
				text.AddPostOptionsLine("Command takes all frames from input.sl2. At the next step it generate all(\"f\" option) the frames at 0th(Primary) channel from 5th(SidescanComposite) clannel and finally save frames from all input file channels to \"sl2\" format.\n\n");
				return text;
			}

		}

		private static void LowranceLogDataInfo(LowranceLogData data)
		{
			if (data == null)
				return;
			Console.WriteLine("File Version = {0}, Hardware Version = {1}, Block Size = {2}\n", data.Header.FileVersion, data.Header.HardwareVersion, data.Header.BlockSize);

			var tableHeader = $"|{"Channel Type",20}|{"Frequency",22}|{"First Frame №",13}|{"Last Frame №",12}|{"Frames Total",12}|";

			Console.WriteLine(tableHeader);
			Console.WriteLine("-------------------------------------------------------------------------------------");

			foreach (var channel in data.Frames.Select(fr => fr.ChannelType).Distinct())
			{
				var channelFrames = data.Frames.Where(fr => fr.ChannelType == channel).ToList();
				var firstFrame = channelFrames.First();

				var str = $"|{channel + "(" + (byte)channel + ")",20}|{firstFrame.Frequency,22}|{firstFrame.FrameIndex,13}|{channelFrames.Last().FrameIndex,12}|{channelFrames.Count,12}|";
				Console.WriteLine(str);

			}
			Console.WriteLine("-------------------------------------------------------------------------------------");
			var lastStr = $"|{"",20}|{"",22}|{"",13}|{"",12}|{data.Frames.Count,12}|";
			Console.WriteLine(lastStr);
		}

		private static LowranceLogData ReadFile(string filename, bool verbose)
		{
			var stopWatch = new Stopwatch();
			if (verbose)
				Console.WriteLine("Reads file: {0}\n", filename);

			LowranceLogData data;
			try
			{
				using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				{
					stopWatch.Start();
					data = LowranceLogData.ReadFromStream(stream);
					stopWatch.Stop();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Console.WriteLine("Please press any key to exit...");
				Console.ReadKey(true);
				return null;
			}

			if (verbose)
			{
				Console.WriteLine("Read time: " + stopWatch.Elapsed + "\n");
				stopWatch.Reset();
				LowranceLogDataInfo(data);
			}

			return data;
		}


		public static bool DepthShiftTryParse(string inputString, out double value)
		{
			value = double.NaN;
			var nfi = new NumberFormatInfo
			{
				NegativeSign = "m",
				PositiveSign = "p"
			};

			try
			{
				value = double.Parse(inputString, nfi);
			}
			catch (Exception e)
			{
				Console.WriteLine("Can't parse Depth Shift. Full error message:\"{0}\"", e);
			}


			return !double.IsNaN(value);
		}

		static void Main(string[] args)
		{
			var options = new Options();
			if (Parser.Default.ParseArguments(args, options))
			{
				//make console window widther
				Console.WindowWidth = 120;

				//convert channels from string to enum
				var outputchannels = new List<ChannelType>();
				if (options.Channels != null && options.Channels.Any())
				{
					outputchannels.AddRange(options.Channels.Select(channel => (ChannelType)Enum.Parse(typeof(ChannelType), channel)));
				}

				var stopWatch = new Stopwatch();

				//Read Files
				var data = ReadFile(options.InputFile, options.Verbose);

				#region Depth Adjust

				if (!string.IsNullOrWhiteSpace(options.DepthAdjustFile))
				{
					var adjust = ReadFile(options.DepthAdjustFile, options.Verbose);
					var da = new DepthAdjuster(data.Frames, adjust.Frames);

					if (options.Verbose)
					{
						Console.WriteLine("Depth adjust option enabled.");
						Console.WriteLine("Adjust file name:\"{0}\".", options.DepthAdjustFile);

						da.NearestPointsFound += (o, e) =>
						{
							Console.WriteLine("Nearest points are:\nBase - {0} with {1} depth.\nAdjust - {2} with {3}.",
											  e.FirstPoint.Point, e.FirstPoint.Depth, e.SecondPoint.Point, e.SecondPoint.Depth);
							Console.WriteLine("Distance = {0}", e.Distance);
						};

						Console.WriteLine("Looking for the nearest points.....");
					}

					stopWatch.Start();
					var points = da.AdjustDepth();

					if (options.Verbose)
					{
						Console.WriteLine("Adjust time: " + stopWatch.Elapsed + "\n");
					}
					stopWatch.Reset();

					//add points to original sequence
					foreach (var point in points)
					{
						data.Frames.Add((Frame)point);
					}

				}
				#endregion

				#region Depth Shift

				//add or substract depth shift for all frames in data
				if (!string.IsNullOrWhiteSpace(options.DepthShift))
				{
					if (options.Verbose)
					{
						Console.WriteLine("Depth shift option enabled.");
						Console.WriteLine("Trying parse depth shift value...");
					}

					double value;

					if (DepthShiftTryParse(options.DepthShift, out value))
					{
						if (options.Verbose) Console.WriteLine("Depth shift value is:{0}", value);

						//applying depth shift for all frames in data
						foreach (var frame in data.Frames)
						{
							frame.Depth = new LinearDimension(frame.Depth.GetMeters() + value, LinearDimensionUnit.Meter);
						}
					}

				}
				#endregion

				#region Generate channels frames
				//check generate parametrs
				if (options.GenegateParams != null && options.GenegateParams.Any())
				{
					if (options.Verbose)
					{
						Console.WriteLine("Generate frames for specific channel option enabled.");
						Console.WriteLine("Try parse parametrs...");
					}
					var dstChannel = new List<ChannelType>();
					var sourceChannels = new List<ChannelType>();
					var forceGenerate = false;
					var generateFromDepth = false;
					foreach (var optionsGenegateParam in options.GenegateParams)
					{
						ChannelType channelType;

						if (Enum.TryParse(optionsGenegateParam, out channelType))
						{
							if (!dstChannel.Any()) dstChannel.Add(channelType);
							else sourceChannels.Add(channelType);
						}
						if (string.Compare(optionsGenegateParam, "f", StringComparison.InvariantCultureIgnoreCase) == 0)
							forceGenerate = true;
						if (string.Compare(optionsGenegateParam, "d", StringComparison.InvariantCultureIgnoreCase) == 0)
							generateFromDepth = true;
					}

					if (options.Verbose)
					{
						Console.Write("Source channel(s) are:");
						foreach (var sourceChannel in sourceChannels)
						{
							Console.Write(sourceChannel + ",");
						}
						Console.Write("\n");
						Console.WriteLine("Channel for frame generation is:{0}", dstChannel.FirstOrDefault());

						Console.WriteLine("Force erase points at destination channel before generate(\"f\" option): {0}", forceGenerate);
						Console.WriteLine("Generate sounded data from depth value(\"d\" option): {0}", generateFromDepth);

						if (!sourceChannels.Any()) Console.WriteLine("There is no channels for frame generation. Skip generation option.");
					}

					//continue if sourceChannels.Any()
					if (sourceChannels.Any())
					{
						//get unique frames from sourse channel(s)
						var unicueFrameFromSourceChanels = data.Frames
							.Where(frame => sourceChannels.Contains(frame.ChannelType))
							.GroupBy(frame => frame.Point)
							.Select(g => g.First())
							.ToList();

						var erasedPointsCountAtDstChannel = 0;
						//erase dstChannel frames from data
						if (forceGenerate)
						{
							erasedPointsCountAtDstChannel = data.Frames.RemoveAll(frame => frame.ChannelType == dstChannel.FirstOrDefault());		
						}

						//get points for existed dst channel frames
						var dstChannelFramesPoints = data.Frames
							.Where(frame => frame.ChannelType == dstChannel.FirstOrDefault())
							.Select(frame => frame.Point).ToList();

						//generate and add frame for each unique point
						foreach (var unicueFrame in unicueFrameFromSourceChanels)
						{
							if (!dstChannelFramesPoints.Contains(unicueFrame.Point))
							{
								data.Frames.Add(Frame.GenerateFromOtherChannelFrame(dstChannel.FirstOrDefault(), unicueFrame, generateFromDepth));
							}
						}

						if (options.Verbose)
						{
							Console.WriteLine("Unique points at source channel(s):{0}", unicueFrameFromSourceChanels.Count);
							if (forceGenerate)
								Console.WriteLine("Points erased at destination channel:{0}", erasedPointsCountAtDstChannel);
							Console.WriteLine("Points added to destination channel:{0}", unicueFrameFromSourceChanels.Count- dstChannelFramesPoints.Count);
						}
					}
				}


				#endregion

				#region Research mode consloe output

				//if research mode, then opens file again
				if (options.SearchOffset >= 0)
				{
					Console.WriteLine("Try reserch values from " + options.SearchOffset + " bytes offset...\n");
					try
					{
						using (var stream = new FileStream(options.InputFile, FileMode.Open, FileAccess.Read))
						{
							stopWatch.Start();
							using (var reader = new BinaryReader(stream))
							{
								var fileHeader = Header.ReadHeader(reader, 0);

								var researchResult = Frame.ValuesResearch(reader, Header.Lenght, options.SearchOffset, fileHeader.FileVersion);

								var tableHeader = $"|{"String Value",12}|{"Bytes",16}|{"Short #1",8}|{"Short #2",8}|{"Integer",10}|{"Float",15}|{"Frame Index",11}|{"Channel",17}|";
								//Console.WriteLine("String Value \t| Bytes \t| First Short \t| Second Short \t| Integer \t| Float \t| Frame Index \t| Channel");
								Console.WriteLine(tableHeader);
								Console.WriteLine("------------------------------------------------------------------------------------------------------------");

								foreach (var offset in researchResult.Keys)
								{
									var tuple = researchResult[offset];

									//skip Console.WriteLine if frame channel is not selected
									if (outputchannels.Any() && !outputchannels.Contains(tuple.Item7))
										continue;

									//skip Console.WriteLine if frame is not inside diapason
									if (tuple.Item6 < options.FramesFrom || tuple.Item6 > options.FramesTo)
										continue;

									var stringbuilder = new StringBuilder();
									foreach (var onebyte in tuple.Item1)
									{
										stringbuilder.Append(onebyte + ",");
									}

									var str = $"|{BitConverter.ToString(tuple.Item1),12}|{stringbuilder,16}|{tuple.Item2,8}|{tuple.Item3,8}|{tuple.Item4,10}|{tuple.Item5,15}|{tuple.Item6,11}|{tuple.Item7,17}|";

									Console.WriteLine(str);
								}
							}
							stopWatch.Stop();
							Console.WriteLine("Read and research time: " + stopWatch.Elapsed + "\n");
							stopWatch.Reset();
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}

				#endregion

				#region Creating output file

				//makes outut file if it necessary
				if (options.OutputFileVersion != null && options.OutputFileVersion.Any())
				{
					Func<Frame, bool> frameValidationFunc = frame =>
					{
						var isIndexValid = frame.FrameIndex >= options.FramesFrom && frame.FrameIndex <= options.FramesTo;

						return outputchannels.Any() ? outputchannels.Contains(frame.ChannelType) && isIndexValid : isIndexValid;

					};

					Console.WriteLine("Making new frames list...\n");
					stopWatch.Start();
					var newFrames = data.Frames.Where(frameValidationFunc).ToList();
					stopWatch.Stop();
					Console.WriteLine("List created.  Creation time: " + stopWatch.Elapsed + "\n");
					stopWatch.Reset();


					//delete coordinates if it necessary
					if (options.CoordinatesDelete)
					{
						Console.WriteLine("Deleting coordinate points...\n");
						stopWatch.Start();
						Func<Frame, Frame> coordinatesDelete = frame =>
						{
							frame.Point = new CoordinatePoint(0, 0);
							return frame;
						};

						newFrames = newFrames.Select(coordinatesDelete).ToList();
						stopWatch.Stop();
						Console.WriteLine("Points deleted.  Delete time: " + stopWatch.Elapsed + "\n");
						stopWatch.Reset();
					}

					var newData = new LowranceLogData { Frames = newFrames };

					//checks output formats and write to files
					foreach (var format in options.OutputFileVersion)
					{

						#region Creating SL2

						if (string.Compare(format, "sl2", StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							//if original header have the same format then reuse it
							newData.Header = data.Header.FileVersion == FileVersion.SL2 ? data.Header : Header.sl2;
							try
							{
								using (var stream = new FileStream(@"out.sl2", FileMode.Create, FileAccess.Write))   //- мой короткий 	)
								{
									Console.WriteLine("Writing \"out.sl2\" file...\n");
									stopWatch.Start();

									LowranceLogData.WriteToStream(stream, newData);

									stopWatch.Stop();
									Console.WriteLine("Writing complete.  Writing time: " + stopWatch.Elapsed + "\n");
									stopWatch.Reset();
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex);
							}

						}
						#endregion

						#region Creating SL3

						if (string.Compare(format, "sl3", StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							//if original header have the same format then reuse it
							newData.Header = data.Header.FileVersion == FileVersion.SL3 ? data.Header : Header.sl3;
							try
							{
								using (var stream = new FileStream(@"out.sl3", FileMode.Create, FileAccess.Write))   //- мой короткий 	)
								{
									Console.WriteLine("Writing \"out.sl3\" file...\n");
									stopWatch.Start();

									LowranceLogData.WriteToStream(stream, newData);

									stopWatch.Stop();
									Console.WriteLine("Writing complete.  Writing time: " + stopWatch.Elapsed + "\n");
									stopWatch.Reset();
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex);
							}

						}
						#endregion

						#region Creating CVS

						if (string.Compare(format, "csv", StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							//create CVSLogData object
							var cvsData = new CsvLogData()
							{
								CreationDateTime = DateTimeOffset.Now,
								Name = "CVSLogData object",
								Points = new List<CsvLogEntry>()
							};

							//and fill Points list
							foreach (var frame in newFrames)
							{
								cvsData.Points.Add(new CsvLogEntry(frame));
							}

							//cvsData.Points = cvsData.Points.Distinct().ToList();
							//new CvsLogEntry.CvsLogEntryComparer()
							//var groups = cvsData.Points.GroupBy(point => point.Point).ToList();

							//grouping by coordinate points
							cvsData.Points = cvsData.Points.GroupBy(point => point.Point)
															.Select(g => g.First())
															.ToList();

							//writing points to file
							try
							{
								using (var stream = new FileStream(@"out.csv", FileMode.Create, FileAccess.Write))
								{
									Console.WriteLine("Writing \"out.csv\" file...\n");
									stopWatch.Start();

									CsvLogData.WriteToStream(stream, cvsData);

									stopWatch.Stop();
									Console.WriteLine("{0} points writing complete.  Writing time: {1} \n", cvsData.Points.Count, stopWatch.Elapsed);
									stopWatch.Reset();

								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex);
							}

						}

						#endregion
					}
				}
				#endregion

				Console.WriteLine("Please press any key to exit...");
				Console.ReadKey(true);
				return;


			}

			Console.WriteLine("Can't parce params =(\nPlease press any key to exit...");
			Console.ReadKey(true);
		}
	}
}
