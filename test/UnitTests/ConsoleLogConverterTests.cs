using NUnit.Framework;
using SonarLogAPI.Localization;

namespace UnitTests
{
    [TestFixture(Author = ProjectDescriptions.Company)]
	public class ConsoleLogConverterTests
	{
		[Test]
		public void DepthShiftTryParseTest()
		{
			var positiveValueString = "p15.1256";
			var negativeValueString = "m4567.126458";

			double value0;
			var result0 = ConsoleLogConverter.Program.DepthShiftTryParse(positiveValueString, out value0);
			Assert.IsTrue(result0);
			Assert.AreEqual(15.1256d,value0);

			double value1;
			var result1 = ConsoleLogConverter.Program.DepthShiftTryParse(negativeValueString, out value1);
			Assert.IsTrue(result1);
			Assert.AreEqual(-4567.126458d, value1);

		}
	}
}