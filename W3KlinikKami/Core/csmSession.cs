using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace W3KlinikKami.Core
{
    public static class csmSession
    {
        public static void Set(int id, string kodeJbt)
        {
            HttpContext.Current.Session.Add("ID", id);
            HttpContext.Current.Session.Add("JABATAN", kodeJbt);
        }

        public static int GetIdSession() => Convert.ToInt32(HttpContext.Current.Session["ID"]);

        public static string GetJabatanSession() => Convert.ToString(HttpContext.Current.Session["JABATAN"]);


    }
}