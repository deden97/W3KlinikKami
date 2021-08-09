using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace W3KlinikKami.Core
{
    public static class FlashMessage
    {
        public static string Message { get; set; }

        public enum FlashMessageType { Off, Success, Error, Warning }

        public static FlashMessageType SelectedType { get; set; }

        public static void SetFlashMessage(string message, FlashMessageType selectType)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                SelectedType = selectType;
            }
        }

        public static void TemFlashMessageLogin()
        {
            Message = "Anda Harus Login Terlebih Dahulu.";
            SelectedType = FlashMessageType.Warning;
        }
    }
}