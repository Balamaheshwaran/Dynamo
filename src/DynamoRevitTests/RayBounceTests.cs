﻿using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class RayBounceTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void RayBounce()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\RayBounce\RayBounce.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //ensure that the bounce curve count is the same
            var curveColl = new FilteredElementCollector(dynRevitSettings.Doc.Document, dynRevitSettings.Doc.ActiveView.Id);
            curveColl.OfClass(typeof(CurveElement));
            Assert.AreEqual(curveColl.ToElements().Count(), 36);
        }
    }
}
