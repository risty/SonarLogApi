using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SonarLogAPI.CSV;
using SonarLogAPI.Localization;
using SonarLogAPI.Lowrance;
using SonarLogAPI.Primitives;

namespace UnitTests
{
    [TestFixture(Author = ProjectDescriptions.Company)]
	public class CSVTests
	{

		[Test(TestOf = typeof(CsvLogEntry))]
		public void CsvLogEntryTest()
		{
			var entry1 = new CsvLogEntry(67.564353467453, 120.346372572, 5.34145, LinearDimensionUnit.Meter);
			var entry2 = new CsvLogEntry(67.564353467453, 120.346372572, 5.34145, LinearDimensionUnit.Meter);

			Assert.AreEqual(entry1, entry2);
			Assert.IsTrue(entry1 == entry2);

		}

		[Test(TestOf = typeof(CsvLogEntry))]
		public void CsvLogEntryTryParseAndToStringTest()
		{
			var entry1 = new CsvLogEntry(67.564353467453, 120.346372572, 5.34145, LinearDimensionUnit.Meter);
			var csvString = entry1.ToString();

			var valuesOrder1 = new Dictionary<int, string> { { 0, "Latitude" }, { 1, "Longitude" }, { 2, "Depth" } };

			Assert.IsTrue(CsvLogEntry.TryParse(csvString, ',', LinearDimensionUnit.Meter, valuesOrder1, out var result));
			Assert.AreEqual(entry1, result);

			//chars with random cases
			var valuesOrder2 = new Dictionary<int, string> { { 0, "latituDe" }, { 1, "lonGitude" }, { 2, "dePth" } };

			Assert.IsTrue(CsvLogEntry.TryParse(csvString, ',', LinearDimensionUnit.Meter, valuesOrder2, out var result2));
			Assert.AreEqual(entry1, result2);
		}

		[Test(TestOf = typeof(CsvLogData))]
		public void CsvLogDataReadWriteTest()
		{
			var testDir = TestContext.CurrentContext.TestDirectory;
			
			var csvWriteData = new CsvLogData();
			var csvReadData = new CsvLogData();

			//takes csv data from demo .sl2
			using (var fromSl2Stream = new FileStream(testDir + @"\format_examples\input.sl2", FileMode.Open, FileAccess.Read))
			{
				csvWriteData.Points = LowranceLogData.ReadFromStream(fromSl2Stream).Frames
				.Select(frame => new CsvLogEntry(frame));
			}

			//write it with csv format
			using (var toStream = new FileStream(testDir + @"\testCSVData.csv", FileMode.Create, FileAccess.Write))
			{
				CsvLogData.WriteToStream(toStream,csvWriteData);
			}

			using (var fromCSVStream = new FileStream(testDir + @"\testCSVData.csv", FileMode.Open, FileAccess.Read))
			{
				csvReadData = CsvLogData.ReadFromStream(fromCSVStream);
			}

			var writeDepthPoints = csvWriteData.Points as IDepthPointSource[] ?? csvWriteData.Points.ToArray();

			Assert.IsTrue(writeDepthPoints.Count() == csvReadData.Points.Count());

			foreach (var point in writeDepthPoints)
			{
				Assert.IsTrue(csvReadData.Points.Contains(point));
			}

		}
	}
}