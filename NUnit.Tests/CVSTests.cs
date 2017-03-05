namespace NUnit.Tests
{
	using NUnit.Framework;

	using SonarLogAPI.CVS;
	using SonarLogAPI.Localization;
	using SonarLogAPI.Primitives;

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class CVSTests
	{

		[Test(TestOf = typeof(CvsLogEntry))]
		public void CvsLogEntryTest()
		{
			var entry1 = new CvsLogEntry(67.564353467453, 120.346372572, 5.34145, LinearDimensionUnit.Meter);
			var entry2 = new CvsLogEntry(67.564353467453, 120.346372572, 5.34145, LinearDimensionUnit.Meter);

			Assert.AreEqual(entry1, entry2);
			Assert.IsTrue(entry1 == entry2);

		}

	}
}