using System.IO;
using System.Reflection;
using System.Web;
using System.Web.SessionState;

namespace W3KlinikKamiUnitTest.Controllers
{
    public class TesSetup
    {
        public void SetupHttp()
        {
            // We need to setup the Current HTTP Context as follows:            

            // Step 1: Setup the HTTP Request
            HttpRequest httpRequest = new HttpRequest("", "https://localhost:44360/", "");

            // Step 2: Setup the HTTP Response
            HttpResponse httpResponce = new HttpResponse(new StringWriter());

            // Step 3: Setup the Http Context
            HttpContext httpContext = new HttpContext(httpRequest, httpResponce);

            HttpSessionStateContainer sessionContainer = new HttpSessionStateContainer(
                "id", new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(), 10,
                true, HttpCookieMode.AutoDetect,
                SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState)
                .GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null, CallingConventions.Standard,
                    new[] { typeof(HttpSessionStateContainer) }, null)
                .Invoke(new object[] { sessionContainer });

            // Step 4: Assign the Context
            HttpContext.Current = httpContext;
        }
    }
}
