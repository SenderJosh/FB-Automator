using System;
using System.Collections.Generic;
using Facebook;

namespace FBAutomator
{
    /// <summary>
    /// Contains all of the global user-wide important stuff. Obviously, don't put your appSecret and appID here locally
    /// It's better to handle this through PHP requests
    /// </summary>
    class Storage
    {
        public static string appSecret = "APPSECRET";
        public static string appID = "APPID";
        public static string name { get; set; }
        public static string fbAuthToken { get; set; }
        public static string email { get; set; }
        public static FacebookClient fb { get; set; }
        public static Dictionary<string, string> pageNamesAndIDs = new Dictionary<string, string>();
        public static DateTime? lastScheduledPostDate { get; set; }

    }
}
