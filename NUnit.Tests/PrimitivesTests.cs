using NUnit.Framework;


namespace NUnit.Tests
{
	using System;

	using SonarLogAPI.Localization;
	using SonarLogAPI.Primitives;

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class LatitudeTests
	{

		const double _extremlyHighLatitudeDouble = 96.81234123;
		const double _northLatitudeDouble = 43.81234123;
		const double _equatorLatitudeDouble = 0;
		const double _southLatitudeDouble = -60.813454769;
		const double _extremlyLowLatitudeDouble = -95.85472;

		private Latitude northLatitude = new Latitude(_northLatitudeDouble);
		private Latitude equatorLatitude = new Latitude(_equatorLatitudeDouble);
		private Latitude southLatitude = new Latitude(_southLatitudeDouble);

		[Test(TestOf = typeof(Latitude))]
		public void DifferentLatitudeCreation()
		{
			Assert.That(() => { new Latitude(_extremlyHighLatitudeDouble); },
				Throws.TypeOf<ArgumentOutOfRangeException>());

			Assert.AreEqual(43, northLatitude.Degrees);
			Assert.AreEqual(48, northLatitude.Minutes);
			Assert.AreEqual(44.4, northLatitude.Seconds, 0.1);
			Assert.AreEqual(LatitudePosition.North, northLatitude.Position);

			Assert.AreEqual(0, equatorLatitude.Degrees);
			Assert.AreEqual(0, equatorLatitude.Minutes);
			Assert.AreEqual(0, equatorLatitude.Seconds);

			Assert.AreEqual(60, southLatitude.Degrees);
			Assert.AreEqual(48, southLatitude.Minutes);
			Assert.AreEqual(48.4, southLatitude.Seconds, 0.1);
			Assert.AreEqual(LatitudePosition.South, southLatitude.Position);

			Assert.That(() => { new Latitude(_extremlyLowLatitudeDouble); },
				Throws.TypeOf<ArgumentOutOfRangeException>());

		}

		[Test(TestOf = typeof(Latitude))]
		public void LatitudeToDoubleTest()
		{

			Assert.AreEqual(_northLatitudeDouble,
				Latitude.ToDouble(northLatitude.Degrees, northLatitude.Minutes, northLatitude.Seconds, northLatitude.Position));

			Assert.AreEqual(_equatorLatitudeDouble,
				Latitude.ToDouble(equatorLatitude.Degrees, equatorLatitude.Minutes, equatorLatitude.Seconds, equatorLatitude.Position));

			Assert.AreEqual(_southLatitudeDouble,
				Latitude.ToDouble(southLatitude.Degrees, southLatitude.Minutes, southLatitude.Seconds, southLatitude.Position));

			Assert.AreEqual(_northLatitudeDouble, northLatitude.ToDouble());
			Assert.AreEqual(_equatorLatitudeDouble, equatorLatitude.ToDouble());
			Assert.AreEqual(_southLatitudeDouble, southLatitude.ToDouble());

		}

		[Test(TestOf = typeof(Latitude))]
		public void LatitudeParceTest()
		{
			Latitude latitude;

			var res = Latitude.TryParse(_northLatitudeDouble.ToString(), out latitude);
			Assert.IsTrue(res);
			Assert.IsTrue(northLatitude.Equals(latitude));
			Assert.IsFalse(northLatitude == latitude);

		}
	}

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class LongitudeTests
	{

		const double _extremlyWestLongitudeDouble = -196.81234123;
		const double _westLongitudeDouble = -164.243457567;
		const double _baseLongitudeDouble = 0;
		const double _eastLongitudeDouble = 95.4567427812;
		const double _extremlyEastLongitudeDouble = 195.85472;

		private Longitude westLongitude = new Longitude(_westLongitudeDouble);
		private Longitude baseLongitude = new Longitude(_baseLongitudeDouble);
		private Longitude eastLongitude = new Longitude(_eastLongitudeDouble);

		[Test(TestOf = typeof(Longitude))]
		public void DifferentLongitudeCreation()
		{
			Assert.That(() => { new Longitude(_extremlyWestLongitudeDouble); },
				Throws.TypeOf<ArgumentOutOfRangeException>());

			Assert.AreEqual(164, westLongitude.Degrees);
			Assert.AreEqual(14, westLongitude.Minutes);
			Assert.AreEqual(36.4, westLongitude.Seconds, 0.1);
			Assert.AreEqual(LongitudePosition.West, westLongitude.Position);

			Assert.AreEqual(0, baseLongitude.Degrees);
			Assert.AreEqual(0, baseLongitude.Minutes);
			Assert.AreEqual(0, baseLongitude.Seconds);

			Assert.AreEqual(95, eastLongitude.Degrees);
			Assert.AreEqual(27, eastLongitude.Minutes);
			Assert.AreEqual(24.3, eastLongitude.Seconds, 0.1);
			Assert.AreEqual(LongitudePosition.East, eastLongitude.Position);

			Assert.That(() => { new Longitude(_extremlyEastLongitudeDouble); },
				Throws.TypeOf<ArgumentOutOfRangeException>());

		}

		[Test(TestOf = typeof(Longitude))]
		public void LongitudeParceTest()
		{
			Longitude longitude;

			var result = Longitude.TryParse(_westLongitudeDouble.ToString(), out longitude);
			Assert.IsTrue(result);
			Assert.IsTrue(westLongitude.Equals(longitude));
			Assert.IsFalse(westLongitude == longitude);

		}
	}

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class CoordinatePointTests
	{
		const double _northLatitudeDouble = 43.81234123;
		const double _equatorLatitudeDouble = 0;
		const double _southLatitudeDouble = -60.813454769;

		const double _westLongitudeDouble = -164.243457567;
		const double _baseLongitudeDouble = 0;
		const double _eastLongitudeDouble = 95.4567427812;

		[Test(TestOf = typeof(CoordinatePoint))]
		public void EqualTest()
		{
			var point1 = new CoordinatePoint(43.81234123, -164.243457567);
			var point2 = new CoordinatePoint(43.81234123, -164.243457567);
			var point3 = new CoordinatePoint(-60.813454769, 95.4567427812);

			Assert.AreEqual(point1,point2);
			Assert.IsTrue(point1 == point2);
			Assert.IsFalse(point1 == point3);
			Assert.IsTrue(point1 != point3);
		}

		[Test(TestOf = typeof(CoordinatePoint))]
		public void DistanceTest()
		{
			var one = new CoordinatePoint(new Latitude(60,12.698), new Longitude(32,25.259));

			var two = new CoordinatePoint(new Latitude(60, 12.730), new Longitude(32, 25.346));

			// distance takes from Lowrance Elite-5 HDI
			Assert.AreEqual(99, CoordinatePoint.DistanceBetweenPoints(one, two).GetMeters(), 1);
			
		}

	}

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class LinearDimensionTests
	{
		[Test(TestOf = typeof(LinearDimension))]
		public void LinearDimensionEqualTest()
		{
			var depth1 = new LinearDimension(43.81234123, LinearDimensionUnit.Foot);
			var depth2 = new LinearDimension(depth1.GetMeters(), LinearDimensionUnit.Meter);

			Assert.AreEqual(depth1, depth2);
			Assert.IsTrue(depth1 == depth2);
		}
	}


}
