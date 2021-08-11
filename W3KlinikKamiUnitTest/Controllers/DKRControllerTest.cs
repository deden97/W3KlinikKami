using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using W3KlinikKami.Controllers;
using W3KlinikKami.Core;

namespace W3KlinikKamiUnitTest.Controllers
{
    [TestClass]
    public class DKRControllerTest : Controller
    {
        [TestInitialize]
        public void TestSetup()
        {
            new TesSetup().SetupHttp();
        }

        /* Begin: Index Test------------------------------------------------------------------------------------------ */
        [TestMethod]
        public void IndexGet_Session_Is_True()
        {
            DKRController dKRController = new DKRController();

            csmSession.Set(13, "DKR");
            var result = (RedirectToRouteResult)dKRController.Index();

            Assert.AreEqual("TanganiPasien", result.RouteValues["action"]);
        }

        [TestMethod]
        public void IndexGet_Session_Is_False()
        {
            DKRController dKRController = new DKRController();

            csmSession.Set(-1, "null");
            var result = (RedirectToRouteResult)dKRController.Index();

            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("Index", result.RouteValues["controller"]);
        }
        /* End: Index Test------------------------------------------------------------------------------------------ */

        /* Begin: TanganiPasien Test------------------------------------------------------------------------------------------ */
        [TestMethod]
        public void TanganiPasienGet_Session_Is_True()
        {
            DKRController dKRController = new DKRController();
            csmSession.Set(13, "DKR");
            int idPasien = 1;
            var result = (ViewResult)dKRController.TanganiPasien(idPasien);
            
            Assert.AreEqual(DKRController.menu.TanganiPasien.ToString(), result.ViewBag.Menu);
            Assert.IsNotNull(result.ViewData["Data"]);
            if (idPasien > 0)
                Assert.IsNotNull(result.ViewBag.DataPasienTerpilih);
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void TanganiPasienGet_Session_Is_False()
        {
            DKRController dKRController = new DKRController();
            csmSession.Set(-1, "null");
            var result = (RedirectToRouteResult)dKRController.TanganiPasien(null);

            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("Index", result.RouteValues["controller"]);
        }

        [TestMethod]
        public void TanganiPasienPost()
        {
            DKRController dKRController = new DKRController();

            var result = (ViewResult)dKRController.TanganiPasien();
            Assert.AreEqual("Index", result.ViewName);

        }
        /* End: TanganiPasien Test------------------------------------------------------------------------------------------ */
    }
}
