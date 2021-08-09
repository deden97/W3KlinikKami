using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Mvc;
using W3KlinikKami.Controllers;
using W3KlinikKami.Core;

namespace W3KlinikKamiUnitTest.Controllers
{
    [TestClass]
    public class ADMControllerTest
    {

        [TestInitialize]
        public void TestSetup() => new TesSetup().SetupHttp();

        [TestMethod]
        public void IndexGet_Session_Is_True()
        {
            ADMController aDMController = new ADMController();

            csmSession.Set(14, "ADM");
            var result = (RedirectToRouteResult)aDMController.Index();

            Assert.AreEqual(ADMController.PenangananPasien.BerobatPasien.ToString(), result.RouteValues["action"]);
        }

        [TestMethod]
        public void IndexGet_Session_Is_False()
        {
            ADMController aDMController = new ADMController();

            csmSession.Set(-1, "");
            var result = (RedirectToRouteResult)aDMController.Index();

            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("Index", result.RouteValues["controller"]);
        }
    }
}
