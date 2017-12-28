using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SonarLogAPI.CSV;
using SonarLogAPI.Lowrance;


namespace NUnit.Tests
{
	using System;
	using System.Globalization;

	using SonarLogAPI.Localization;
	using SonarLogAPI.Primitives;

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class LatitudeTests
	{

		const double _extremlyHighLatitudeDouble = 96.81234123;
		const double _northLatitudeDegrees = 43.81234123;
		const double _equatorLatitudeDegrees = 0;
		const double _southLatitudeDegrees = -60.813454769;
		const double _extremlyLowLatitudeDouble = -95.85472;

		private Latitude northLatitude = new Latitude(_northLatitudeDegrees);
		private Latitude equatorLatitude = new Latitude(_equatorLatitudeDegrees);
		private Latitude southLatitude = new Latitude(_southLatitudeDegrees);

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
		public void LatitudeToDegreesTest()
		{
			Assert.AreEqual(_northLatitudeDegrees,
				Latitude.ToDegrees(northLatitude.Degrees, northLatitude.Minutes, northLatitude.Seconds, northLatitude.Position));
			Assert.AreEqual(_equatorLatitudeDegrees,
				Latitude.ToDegrees(equatorLatitude.Degrees, equatorLatitude.Minutes, equatorLatitude.Seconds, equatorLatitude.Position));
			Assert.AreEqual(_southLatitudeDegrees,
				Latitude.ToDegrees(southLatitude.Degrees, southLatitude.Minutes, southLatitude.Seconds, southLatitude.Position));

			Assert.AreEqual(_northLatitudeDegrees, _northLatitudeDegrees);
			Assert.AreEqual(_equatorLatitudeDegrees, equatorLatitude.ToDegrees());
			Assert.AreEqual(_southLatitudeDegrees, southLatitude.ToDegrees());

		}

		[Test(TestOf = typeof(Latitude))]
		public void LatitudeToRadiansTest()
		{
			Assert.AreEqual(northLatitude.ToRadians(), _northLatitudeDegrees * Math.PI / 180d);
			Assert.AreEqual(equatorLatitude.ToRadians(), _equatorLatitudeDegrees * Math.PI / 180d);

			Assert.AreEqual(Math.Sin(11d / 6d * Math.PI), Math.Sin(-1d / 6d * Math.PI), 0.00000001d);
			Assert.AreEqual(southLatitude.ToRadians(), _southLatitudeDegrees * Math.PI / 180d);
		}

		[Test(TestOf = typeof(Latitude))]
		public void LatitudeParceTest()
		{
			var res = Latitude.TryParse(_northLatitudeDegrees.ToString(CultureInfo.InvariantCulture), out var latitude);
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
			var result = Longitude.TryParse(_westLongitudeDouble.ToString(CultureInfo.InvariantCulture), out var longitude);
			Assert.IsTrue(result);
			Assert.IsTrue(westLongitude.Equals(longitude));
			Assert.IsFalse(westLongitude == longitude);

		}
	}

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class CoordinatePointTests
	{

		[Test(TestOf = typeof(CoordinatePoint))]
		public void EqualTest()
		{
			var point1 = new CoordinatePoint(43.81234123, -164.243457567);
			var point2 = new CoordinatePoint(43.81234123, -164.243457567);
			var point3 = new CoordinatePoint(-60.813454769, 95.4567427812);

			Assert.AreEqual(point1, point2);
			Assert.IsTrue(point1 == point2);
			Assert.IsFalse(point1 == point3);
			Assert.IsTrue(point1 != point3);
		}

		[Test(TestOf = typeof(CoordinatePoint))]
		public void DistanceTest()
		{
			// expected distances takes from:
			//1. Lowrance HDS7 Gen3
			//2. http://edwilliams.org/gccalc.htm
			var basePoint = new CoordinatePoint(new Latitude(60, 07.328), new Longitude(32, 18.719));

			var two = new CoordinatePoint(new Latitude(60, 12.324), new Longitude(32, 15.530));
			//Lowrance HDS7 Gen3
			Assert.AreEqual(9710, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 5);
			Assert.AreEqual(9710, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 26);

			//edwilliams.org
			Assert.AreEqual(9735.229, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 25);
			Assert.AreEqual(9735.229, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 0.01d); //great accuracu

			two = new CoordinatePoint(new Latitude(57, 36.631), new Longitude(29, 44.306));
			//Lowrance HDS7 Gen3
			Assert.AreEqual(315800, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 200);
			Assert.AreEqual(315800, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 1000);

			//edwilliams.org
			Assert.AreEqual(316682.824, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 700);
			Assert.AreEqual(316682.824, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 10);

			two = new CoordinatePoint(new Latitude(51, 11.377), new Longitude(44, 14.519));
			Assert.AreEqual(1239000, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 700);
			Assert.AreEqual(1239000, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 5000);

			two = new CoordinatePoint(new Latitude(60, 07.317), new Longitude(32, 18.822));
			//Lowrance HDS7 Gen3
			Assert.AreEqual(97.3d, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 0.1d);
			Assert.AreEqual(97.3d, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 0.4d);

			//edwilliams.org
			Assert.AreEqual(97.598, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 0.4d);
			Assert.AreEqual(97.598, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 0.01d);

			two = new CoordinatePoint(new Latitude(60, 07.330), new Longitude(32, 18.716));
			Assert.AreEqual(5.1d, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 0.5d);
			Assert.AreEqual(5.1d, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 0.5d);
			//too poor accuracy with flat method
			Assert.AreEqual(5.1d, CoordinatePoint.GetDistanceBetweenPointsOnTheFlat(basePoint, two).GetMeters(), 2);

			two = new CoordinatePoint(new Latitude(60, 07.374), new Longitude(32, 18.754));
			Assert.AreEqual(90.6d, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 1);
			Assert.AreEqual(90.6d, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 0.8d);

			two = new CoordinatePoint(new Latitude(60, 07.318), new Longitude(32, 18.734));
			Assert.AreEqual(23.2d, CoordinatePoint.GetDistanceBetweenPointsWithHaversine(basePoint, two).GetMeters(), 1);
			Assert.AreEqual(23.2d, CoordinatePoint.GetDistanceBetweenPointsOnAnEllipsoid(basePoint, two).GetMeters(), 0.01d);

		}

		[Test(TestOf = typeof(CoordinatePoint))]
		public void GetCoordinatePointAtDistanceAndDirectionTest()
		{
			var baseCoordinatePoint = new CoordinatePoint(new Latitude(60, 07.328), new Longitude(32, 18.719));
			var _d2R = Math.PI / 180D;

			//expected values taken from http://edwilliams.org/gccalc.htm

			var pointWithSphereFormula = CoordinatePoint.GetCoordinatePointAtDistanceAndDirectionWithHaversine(baseCoordinatePoint, LinearDimension.FromMeters(3.1d), 70 * _d2R, out _);
			var pointWithEllipsoidFormula = CoordinatePoint.GetCoordinatePointAtDistanceAndDirectionOnAnEllipsoid(baseCoordinatePoint, LinearDimension.FromMeters(3.1d), 70 * _d2R, out _);

			//Sphere
			Assert.AreEqual(60 + 7.3286 / 60d, pointWithSphereFormula.Latitude.ToDegrees(), 0.000001d);
			Assert.AreEqual(32 + 18.7221 / 60d, pointWithSphereFormula.Longitude.ToDegrees(), 0.000001d);

			//Ellipsoid
			Assert.AreEqual(60 + 7.3286 / 60d, pointWithEllipsoidFormula.Latitude.ToDegrees(), 0.000001d);
			Assert.AreEqual(32 + 18.7221 / 60d, pointWithEllipsoidFormula.Longitude.ToDegrees(), 0.000001d);

			pointWithSphereFormula = CoordinatePoint.GetCoordinatePointAtDistanceAndDirectionWithHaversine(baseCoordinatePoint, LinearDimension.FromMeters(10.9d), 71 * _d2R, out _);
			pointWithEllipsoidFormula = CoordinatePoint.GetCoordinatePointAtDistanceAndDirectionOnAnEllipsoid(baseCoordinatePoint, LinearDimension.FromMeters(10.9d), 71 * _d2R, out _);

			//Sphere
			Assert.AreEqual(60 + 7.3299 / 60d, pointWithSphereFormula.Latitude.ToDegrees(), 0.000001d);
			Assert.AreEqual(32 + 18.7301 / 60d, pointWithSphereFormula.Longitude.ToDegrees(), 0.000002d); //2*10-6 !!!!

			//Ellipsoid
			Assert.AreEqual(60 + 7.3299 / 60d, pointWithEllipsoidFormula.Latitude.ToDegrees(), 0.000001d);
			Assert.AreEqual(32 + 18.7301 / 60d, pointWithEllipsoidFormula.Longitude.ToDegrees(), 0.000001d);

			pointWithSphereFormula = CoordinatePoint.GetCoordinatePointAtDistanceAndDirectionWithHaversine(baseCoordinatePoint, LinearDimension.FromMeters(97.8d), 88 * _d2R, out _);
			pointWithEllipsoidFormula = CoordinatePoint.GetCoordinatePointAtDistanceAndDirectionOnAnEllipsoid(baseCoordinatePoint, LinearDimension.FromMeters(97.8d), 88 * _d2R, out _);

			//Sphere
			Assert.AreEqual(60 + 7.3298 / 60d, pointWithSphereFormula.Latitude.ToDegrees(), 0.000001d);
			Assert.AreEqual(32 + 18.8245 / 60d, pointWithSphereFormula.Longitude.ToDegrees(), 0.000007d); //7*10-6 !!!!

			//Ellipsoid
			Assert.AreEqual(60 + 7.3298 / 60d, pointWithEllipsoidFormula.Latitude.ToDegrees(), 0.000001d);
			Assert.AreEqual(32 + 18.8245 / 60d, pointWithEllipsoidFormula.Longitude.ToDegrees(), 0.000001d);

			pointWithSphereFormula = CoordinatePoint.GetCoordinatePointAtDistanceAndDirectionWithHaversine(baseCoordinatePoint, LinearDimension.FromMeters(1400), 233 * _d2R, out _);
			pointWithEllipsoidFormula = CoordinatePoint.GetCoordinatePointAtDistanceAndDirectionOnAnEllipsoid(baseCoordinatePoint, LinearDimension.FromMeters(1400), 233 * _d2R, out _);

			//Sphere
			//poor accuracy
			Assert.AreEqual(60 + 6.8742 / 60d, pointWithSphereFormula.Latitude.ToDegrees(), 0.00002d); //2*10-5 !!!!
			Assert.AreEqual(32 + 17.5126 / 60d, pointWithSphereFormula.Longitude.ToDegrees(), 0.00008d); //8*10-5 !!!!

			//Ellipsoid
			Assert.AreEqual(60 + 6.8742 / 60d, pointWithEllipsoidFormula.Latitude.ToDegrees(), 0.0000011d);
			Assert.AreEqual(32 + 17.5126 / 60d, pointWithEllipsoidFormula.Longitude.ToDegrees(), 0.000003d);
		}

	}

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class LinearDimensionTests
	{
		[Test(TestOf = typeof(LinearDimension))]
		public void LinearDimensionCreationTest()
		{
			//45 foots == 13.716 meters
			var depth1 = new LinearDimension(45, LinearDimensionUnit.Foot);
			Assert.AreEqual(13.716, depth1.GetMeters(), 0.001);

			//20 meters == 65.61679 meters
			var depth2 = new LinearDimension(20, LinearDimensionUnit.Meter);
			Assert.AreEqual(65.61679, depth2.GetFoots(), 0.001);
		}


		[Test(TestOf = typeof(LinearDimension))]
		public void LinearDimensionEqualTest()
		{
			var depth1 = new LinearDimension(43.81234123, LinearDimensionUnit.Foot);
			var depth2 = new LinearDimension(depth1.GetMeters(), LinearDimensionUnit.Meter);

			Assert.AreEqual(depth1, depth2);
			Assert.IsTrue(depth1 == depth2);
		}

		[Test(TestOf = typeof(LinearDimension))]
		public void LinearDimensionOperationsTest()
		{
			var depth1 = new LinearDimension(43.81, LinearDimensionUnit.Foot);
			var depth2 = new LinearDimension(12.34, LinearDimensionUnit.Meter);
			var depth3 = new LinearDimension(depth1.GetMeters() + depth2.GetMeters(), LinearDimensionUnit.Meter);

			Assert.IsTrue(depth1 > depth2);
			Assert.IsTrue(depth2 < depth1);
			Assert.IsTrue(depth3 == depth1 + depth2);

		}
	}

	[TestFixture(Author = ProjectDescriptions.Company)]
	public class MagneticVariationTests
	{
		private const double _toRadians = Math.PI / 180d;

		[Test(TestOf = typeof(MagneticVariation))]
		public void GetMagneticVariationTest()
		{
			var julianDay = new DateTimeOffset(2017, 09, 16, 00, 00, 00, TimeSpan.Zero);

			//expected values from windows gui app https://www.ngdc.noaa.gov/geomag/WMM/wmm_gdownload.shtml
			var lat1 = 60 * _toRadians;
			var lon1 = 30 * _toRadians;
			var point1ExpectedDecination = 10.65 * _toRadians;
			Assert.AreEqual(point1ExpectedDecination,
				MagneticVariation.GetMagneticVariation(lat1, lon1, 0, julianDay, MagneticVariation.MagneticVariationModels.WMM2015, out _), 0.0001d);

			var lat2 = -30 * _toRadians;
			var lon2 = 130 * _toRadians;
			var point2ExpectedDecination = 3.95 * _toRadians;

			Assert.AreEqual(point2ExpectedDecination,
				MagneticVariation.GetMagneticVariation(lat2, lon2, 0, julianDay, MagneticVariation.MagneticVariationModels.WMM2015, out _), 0.0001d);

			var lat3 = 40 * _toRadians;
			var lon3 = -70 * _toRadians;
			var point3ExpectedDecination = -14.5 * _toRadians;

			Assert.AreEqual(point3ExpectedDecination,
				MagneticVariation.GetMagneticVariation(lat3, lon3, 0, julianDay, MagneticVariation.MagneticVariationModels.WMM2015, out _), 0.0001d);

			var lat4 = -50 * _toRadians;
			var lon4 = -170 * _toRadians;
			var point4ExpectedDecination = 31.04 * _toRadians;

			Assert.AreEqual(point4ExpectedDecination,
				MagneticVariation.GetMagneticVariation(lat4, lon4, 0, julianDay, MagneticVariation.MagneticVariationModels.WMM2015, out _), 0.0001d);

        }

	    [Test(TestOf = typeof(MagneticVariation))]
        public void GetMagneticVariationTest2()
	    {
            // expected values from https://www.ngdc.noaa.gov/geomag/WMM/data/WMM2015/WMM2015_Report.pdf, page 20

            var date1 = new DateTimeOffset(2015, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
            var point1 = new CoordinatePoint(Latitude.FromDegrees(80),Longitude.FromDegrees(0));

	        var point1ExpectedDecination = -3.85 * _toRadians;

            Assert.AreEqual(point1ExpectedDecination,
            MagneticVariation.GetMagneticVariation(point1, 5, 0, date1, MagneticVariation.MagneticVariationModels.WMM2015
	            , out _),0.00002d);


	        var date2 = new DateTimeOffset(2015, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
	        var point2 = new CoordinatePoint(Latitude.FromDegrees(0), Longitude.FromDegrees(120));

	        var point2ExpectedDecination = 0.57 * _toRadians;

	        Assert.AreEqual(point2ExpectedDecination,
	            MagneticVariation.GetMagneticVariation(point2, 5, 0, date2, MagneticVariation.MagneticVariationModels.WMM2015
	                , out _), 0.00001d);

        }

	    [Test(TestOf = typeof(MagneticVariation))]
        public void CoordinatesRoundingAndMagVarCalculationTest()
	    {
            var point1 = new CoordinatePoint(Latitude.FromDegrees(60.12345888),Longitude.FromDegrees(30.12345888));
	        var point2 = new CoordinatePoint(Latitude.FromDegrees(60.12345666), Longitude.FromDegrees(30.12345666));

	        var point1Variation = MagneticVariation.GetMagneticVariation(point1, 6, 0, DateTimeOffset.UtcNow, 
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);
	        var point2Variation = MagneticVariation.GetMagneticVariation(point2, 6, 0, DateTimeOffset.UtcNow,
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);

            Assert.AreNotEqual(point1Variation, point2Variation);

	        point1Variation = MagneticVariation.GetMagneticVariation(point1, 5, 0, DateTimeOffset.UtcNow,
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);
	        point2Variation = MagneticVariation.GetMagneticVariation(point2, 5, 0, DateTimeOffset.UtcNow,
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);

	        Assert.AreEqual(point1Variation, point2Variation);
        }

	    [Test(TestOf = typeof(MagneticVariation))]
	    public void MagneticVariationCalculationCacheTest()
	    {
	        var point1 = new CoordinatePoint(Latitude.FromDegrees(60.12345888), Longitude.FromDegrees(30.12345888));

	        var watch = Stopwatch.StartNew();
	        var point1Variation = MagneticVariation.GetMagneticVariation(point1, 6, 0, DateTimeOffset.UtcNow,
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);
            watch.Stop();

	        var elapsed1 = watch.Elapsed;

	        point1Variation = MagneticVariation.GetMagneticVariation(point1, 6, 0, DateTimeOffset.UtcNow,
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);
	        point1Variation = MagneticVariation.GetMagneticVariation(point1, 6, 0, DateTimeOffset.UtcNow,
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);
            point1Variation = MagneticVariation.GetMagneticVariation(point1, 6, 0, DateTimeOffset.UtcNow,
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);

            watch.Restart();
            point1Variation = MagneticVariation.GetMagneticVariation(point1, 6, 0, DateTimeOffset.UtcNow,
	            MagneticVariation.MagneticVariationModels.WMM2015, out _);
	        watch.Stop();

	        var elapsed2 = watch.Elapsed;

            // first run takes more time
	        Assert.IsTrue(elapsed1 > elapsed2);
        }
    }
}
