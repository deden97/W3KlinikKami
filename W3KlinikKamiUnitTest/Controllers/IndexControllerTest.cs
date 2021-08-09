using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Web.Mvc;
using W3KlinikKami.Controllers;
using W3KlinikKami.Models;
using W3KlinikKami.Core;

namespace W3KlinikKamiUnitTest.Controllers
{
    [TestClass]
    public class IndexControllerTest
    {
        [TestInitialize]
        public void TestSetup() => new TesSetup().SetupHttp();

        /* Begin: Login Test------------------------------------------------------------------------------------------ */
        [TestMethod()]
        public void LoginGet_Session_Is_True()
        {
            IndexController indexController = new IndexController();

            // Contoh session dengan akun 'dokter yang sudah terdaftar di Db'
            csmSession.Set(13, "DKR");
            var result = (RedirectToRouteResult)indexController.Login();

            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("DKR", result.RouteValues["controller"]);
        }

        [TestMethod()]
        public void LoginGet_Session_Is_False()
        {
            IndexController indexController = new IndexController();

            csmSession.Set(-1, "tes");
            var result = indexController.Login() as ViewResult;

            Assert.IsNotNull(result.ViewName);
        }

        [TestMethod]
        public void LoginPost_Is_Success()
        {
            IndexController indexController = new IndexController();

            var dtLogin = new DbEntities().TB_AKUN.Find(13);
            var result = (RedirectToRouteResult)indexController.Login(dtLogin);

            Assert.AreEqual(csmSession.GetIdSession(), dtLogin.ID);
            Assert.AreEqual(csmSession.GetJabatanSession(), dtLogin.TB_USER.JABATAN);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("DKR", result.RouteValues["controller"]);
        }

        [TestMethod]
        public void LoginPost_Is_Failed()
        {
            IndexController indexController = new IndexController();

            var dtLogin = new TB_AKUN { USERNAME = "", PASSWORD_AKUN = "" };
            var result = (ViewResult)indexController.Login(dtLogin);
            var sessionId = csmSession.GetIdSession();
            var sessionJbt = csmSession.GetJabatanSession();

            Assert.IsFalse(new DbEntities().TB_AKUN.Any(d => d.ID == sessionId && d.TB_USER.JABATAN == sessionJbt));
            Assert.AreEqual("Login", result.ViewName);
        }
        /* End: Login Test------------------------------------------------------------------------------------------ */
    }
}