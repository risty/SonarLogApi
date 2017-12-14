using System;
using System.Collections.Generic;

using NUnit.Framework;

using SonarLogAPI.Localization;

namespace NUnit.Tests
{
	using System.IO;
	using System.Linq;

	using SonarLogAPI.Compass;
	using SonarLogAPI.CSV;
	using SonarLogAPI.Lowrance;

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class CompassLogTests
	{
		[Test(TestOf = typeof(CompassLogEntry))]
		public void CompassLogEntryParseTest()
		{
			var entry = new CompassLogEntry(DateTimeOffset.Now, 236, 5, -3);
			var valuesOrder1 = new Dictionary<int, string> { { 0, "dAtatimE" }, { 3, "headIng" }, { 4, "piTch" }, { 5, "roLl" } };

			Assert.IsTrue(CompassLogEntry.TryParse(entry.ToString(), ',', valuesOrder1, out var entry2));

			//with 1 millisecond accuracy
			Assert.AreEqual(0, (entry.EntryDateTimeOffset - entry2.EntryDateTimeOffset).TotalMilliseconds, 1d);
			Assert.AreEqual(entry.Point, entry2.Point);
			Assert.AreEqual(entry.Heading, entry2.Heading);
			Assert.AreEqual(entry.Pitch, entry2.Pitch);
			Assert.AreEqual(entry.Roll, entry2.Roll);

		}

		[Test(TestOf = typeof(CompassLogData))]
		public void CompassLogDataReadWriteTest()
		{
			var testDir = TestContext.CurrentContext.TestDirectory;
			var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(testDir));

			var compassFirstReadedData = new CompassLogData();
			var compassSecondReadedData = new CompassLogData();

			//takes compass data from solution dirr
			using (var compassStream = new FileStream(solutionDir + @"\ValuesFromCompass.csv", FileMode.Open, FileAccess.Read))
			{
				compassFirstReadedData = CompassLogData.ReadFromStream(compassStream);
			}

			//writes it to bin dirr
			using (var compassStream = new FileStream(testDir + @"\WrittenValuesFromCompass.csv", FileMode.Create, FileAccess.Write))
			{
				 CompassLogData.WriteToStream(compassStream, compassFirstReadedData);
			}

			//second compass data read
			using (var compassStream = new FileStream(testDir + @"\WrittenValuesFromCompass.csv", FileMode.Open, FileAccess.Read))
			{
				compassSecondReadedData = CompassLogData.ReadFromStream(compassStream);
			}

			Assert.IsTrue(compassFirstReadedData.CompassLogEntrys.Count() == compassSecondReadedData.CompassLogEntrys.Count());

			foreach (var compassLogEntry in compassFirstReadedData.CompassLogEntrys)
			{
				Assert.IsTrue(compassSecondReadedData.CompassLogEntrys.Contains(compassLogEntry));
			}
		}
	}
}
