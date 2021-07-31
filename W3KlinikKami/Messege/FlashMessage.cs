using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace W3KlinikKami.Messege
{
    public static class FlashMessage
    {
        public static string Message { get; set; }

        public enum Page
        {
            Off,
            Login,
            EditData,
            EditUsername,
            EditPassword,
            DaftarPasienBaru,
            PengambilanObat
        }

        public static Page PageDestination { get; set; }

        public enum FlashMessageType { Off, Success, Error, Warning }

        public static FlashMessageType SelectedType { get; set; }

        public static void SetFlashMessage(string message, FlashMessageType selectType, Page pageDestination)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                SelectedType = selectType;
                PageDestination = pageDestination;
            }
        }

        public static void TemFlashMessageLogin()
        {
            Message = "Anda Harus Login Terlebih Dahulu.";
            SelectedType = FlashMessageType.Warning;
            PageDestination = Page.Login;
        }
    }
}