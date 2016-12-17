using System;
using System.Collections.Generic;
using System.Windows;
using System.Net.Http;
using Facebook;
using mshtml;

namespace FBAutomator
{
    /// <summary>
    /// Splash screen, does background stuff and logs the user in to Facebook to grab the auth tokens
    /// </summary>
    public partial class Splash : Window
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Splash()
        {
            InitializeComponent();
            //Use Storage class to load all information after logging in; also get login auth from fb
            progressBarLabel.Content = "Grabbbing user info...";
            doPost();
        }

        /// <summary>
        /// Will run everything in Async so the user can still interact with the UI
        /// Does POST actions to log user in
        /// </summary>
        private async void doPost()
        {
            IEnumerable<KeyValuePair<string, string>> queries = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("email", Storage.email),
            };
            HttpContent q = new FormUrlEncodedContent(queries);
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage hrm = await client.PostAsync("http://myurl/fbGetInfo.php", q))
                {
                    using (HttpContent cont = hrm.Content)
                    {
                        string it = await cont.ReadAsStringAsync();
                        string[] items = it.Split(new string[] { "<br>" }, StringSplitOptions.None);
                        Storage.name = items[0];
                        string[] pageNames = items[1].Split(',');
                        string[] pageIDs = items[2].Split(',');
                        for(int i = 0; i<pageNames.Length-1; i++)
                        {
                            Storage.pageNamesAndIDs.Add(pageNames[i], pageIDs[i]); //Gotta fix this to return ID instead; but that can be done SQL
                        }
                        Storage.lastScheduledPostDate = DateTime.Parse(items[3]);
                        if(Storage.lastScheduledPostDate < DateTime.Now)
                        {
                            Storage.lastScheduledPostDate = null;
                        }
                        progressBar.Value = 40;
                        progressBarLabel.Content = "Grabbing auth...";
                    }
                }
            }
            string scope = "manage_pages,publish_pages";
            //Console.WriteLine(string.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&display=popup&response_type=token&scope={2}", Storage.appID, "https://www.facebook.com/connect/login_success.html", scope));
            wb.Navigate(new Uri(string.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&display=popup&response_type=token&scope={2}", Storage.appID, "https://www.facebook.com/connect/login_success.html", scope), UriKind.Absolute));
        }

        private Uri currURI;
        private bool gotInfoAndLoggedIn = false, finished = false, completed = false;

        /// <summary>
        /// If the login was successful, we can move on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wb_Navigated_1(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            currURI = e.Uri;
            if (currURI.ToString().StartsWith("http://www.facebook.com/connect/login_success.html") || currURI.ToString().StartsWith("https://www.facebook.com/connect/login_success.html") && !finished)
            {
                finished = true;
                Storage.fbAuthToken = currURI.Fragment.Split('&')[0].Replace("#access_token=", "");
                progressBarLabel.Content = "Grabbing auth...";
                progressBar.Value = 90;
                Storage.fb = new FacebookClient(Storage.fbAuthToken);
                progressBar.Value = 100;
                progressBarLabel.Content = "Done!";
                if(!completed)
                {
                    completed = true;
                    new FBWindow().Show();
                    this.Close();
                }
            }
        }

        /// <summary>
        /// WebBrowser event, loadcomplete, checks with facebook auths
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wb_LoadComplete(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!finished)
            {
                var document = wb.Document as mshtml.HTMLDocument;
                if (gotInfoAndLoggedIn)
                {
                    if (currURI.ToString().StartsWith("https://www.facebook.com/v2.6/dialog/oauth?"))
                    {
                        progressBarLabel.Content = "Confirming identity...";
                        foreach (IHTMLElement ele in document.getElementsByTagName("button"))
                        {
                            if (ele.getAttribute("name") == "__CONFIRM__")
                            {
                                ele.click();
                            }
                        }
                        progressBar.Value = 80;
                    }
                    else
                    {
                        gotInfoAndLoggedIn = false;
                        progressBar.Value = 40;
                        wb.Navigate(new Uri(string.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&display=popup&response_type=token", Storage.appID, "https://www.facebook.com/connect/login_success.html"), UriKind.Absolute));
                    }
                }
                else
                {
                    progressBarLabel.Content = "Requesting Facebook information...";
                    FBInformationDialog dialog = new FBInformationDialog();
                    if (dialog.ShowDialog() == true)
                    {
                        progressBarLabel.Content = "Logging in...";
                        if(document.getElementById("email") != null)
                        {
                            document.getElementById("email").innerText = dialog.Email;
                        }
                        progressBar.Value = 50;
                        document.getElementById("pass").innerText = dialog.Pass;
                        progressBar.Value = 60;
                        dialog.Close();
                        document.getElementById("u_0_2").click();
                        gotInfoAndLoggedIn = true;
                    }
                }
            }
        }
    }
}
