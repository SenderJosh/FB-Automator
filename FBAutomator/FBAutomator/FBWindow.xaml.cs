using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Facebook;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Threading;
using System.Net.Http;

namespace FBAutomator
{
    /// <summary>
    /// This is the main window for FBAutomator
    /// </summary>
    public partial class FBWindow : Window
    {

        private string currSelectedPageName = "automator"; //Must be dynamic based off user combo box -- THIS IS NOT THE ACTUAL PAGE ID OR CUSTOM URL
        private DateTime customScheduledDateTime;

        /// <summary>
        /// Constructor to initialize the main window for the automator
        /// </summary>
        public FBWindow()
        {
            InitializeComponent();
            
            groupBox.Visibility = Visibility.Visible;
            groupBox1.Visibility = Visibility.Visible;
            facebookNameLabel.Content = Storage.name;
            emailLabel.Content = Storage.email;
            licenseLabel.Content = "Expert";
            progressBarUpload.Maximum = 100;

            foreach(KeyValuePair<string, string> en in Storage.pageNamesAndIDs)
            {
                comboBox.Items.Add(en.Key + "-" + en.Value);
            }
            comboBox.SelectedIndex = 0;

            if(Storage.lastScheduledPostDate == null)
            {
                radioButtonBasedOffLastPostTime.Content = "From now";
            }
            else
            {
                radioButtonBasedOffLastPostTime.Content = "From " + Storage.lastScheduledPostDate.ToString();
            }

            comboBoxDay.Visibility = Visibility.Hidden;
            comboBoxMonth.Visibility = Visibility.Hidden;
            comboBoxYear.Visibility = Visibility.Hidden;

            comboBoxDay.Items.Add("DD");
            comboBoxMonth.Items.Add("MM");
            comboBoxYear.Items.Add("YYYY");

            for(int i = 1; i<=30; i++)
            {
                comboBoxDay.Items.Add(i);
            }

            DateTime dt = DateTime.Now;
            for(int i = 0; i<6; i++)
            {
                comboBoxMonth.Items.Add(dt.Month);
                dt = dt.AddMonths(1);
            }
            comboBoxYear.Items.Add(DateTime.Now.Year);
            if(DateTime.Now.Month+6 < 12)
            {
                comboBoxYear.Items.Add(DateTime.Now.AddYears(1).Year);
            }
            comboBoxDay.SelectedIndex = 0;
            comboBoxMonth.SelectedIndex = 0;
            comboBoxYear.SelectedIndex = 0;
        }

        /// <summary>
        /// Using tasks/factories to check access token and refresh it so they can post to facebook in a separate thread
        /// </summary>
        /// <param name="currentAccessToken">Current access token</param>
        /// <param name="content">Post content</param>
        /// <param name="img">Image</param>
        /// <param name="me">Media Element</param>
        /// <param name="timeInc">The time increment</param>
        /// <param name="basedOffLastPostTime">From what time</param>
        /// <param name="name">Name</param>
        /// <param name="source">Source</param>
        /// <param name="selectedPageID">PageID the person will post to</param>
        /// <returns></returns>
        public Task<string> refreshTokenAndPostToFacebook(string currentAccessToken, string content, Image img, MediaElement me, double timeInc, bool basedOffLastPostTime, string name, string source, string selectedPageID)
        {
            return Task.Factory.StartNew(() => refreshTokenAndPostToFacebookTask(currentAccessToken, content, img, me, timeInc, basedOffLastPostTime, name, source, selectedPageID)); // replace current access token with this
        }

        /// <summary>
        /// Parallel thread that actually will post to facebook and refresh the token and page access token
        /// </summary>
        /// <param name="currentAccessToken">Current access token</param>
        /// <param name="content">Post content</param>
        /// <param name="img">Image</param>
        /// <param name="me">Media Element</param>
        /// <param name="timeInc">The time increment</param>
        /// <param name="basedOffLastPostTime">From what time</param>
        /// <param name="name">Name</param>
        /// <param name="source">Source</param>
        /// <param name="selectedPageID">PageID the person will post to</param>
        /// <returns>Access token</returns>
        private string refreshTokenAndPostToFacebookTask(string currentAccessToken, string content, Image img, MediaElement me, double timeinc, bool basedOffLastPostTime, string name, string source, string selectedPageID)
        {
            string newAccessToken = refreshAccessToken(currentAccessToken);
            string pageAccessToken = getPageAccessToken(newAccessToken);
            if (!string.IsNullOrEmpty(pageAccessToken))
            {
                postToFacebook(pageAccessToken, content, img, me, timeinc, basedOffLastPostTime, source, name, selectedPageID);
            }
            else
            {
                MessageBox.Show("Your page name (not your custom URL) is invalid.\nPlease check that both your page name and your page ID are correct." +
                    "\nYou can find your page ID at www.facebook.com/YourCustomURL/info");
            }
            return newAccessToken;
        }

        /// <summary>
        /// Gets the page access token. Required to get the user token first (otherwise we won't know if the user is still valid)
        /// </summary>
        /// <param name="userAccessToken">User access token</param>
        /// <returns>Page access token</returns>
        public string getPageAccessToken(string userAccessToken)
        {
            FacebookClient fbClient = new FacebookClient();
            fbClient.AppId = Storage.appID;
            fbClient.AppSecret = Storage.appSecret;
            fbClient.AccessToken = userAccessToken;
            Dictionary<string, object> fbParams = new Dictionary<string, object>();
            JsonObject publishedResponse = fbClient.Get("/me/accounts", fbParams) as JsonObject;
            JArray data = JArray.Parse(publishedResponse["data"].ToString());

            foreach (var account in data)
            {
                Console.WriteLine(account["name"].ToString().ToLower());
                if (account["name"].ToString().ToLower().Equals(currSelectedPageName))
                {
                    return account["access_token"].ToString();
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Refreshes the user's access token
        /// </summary>
        /// <param name="currentAccessToken">Current access token</param>
        /// <returns>New access token</returns>
        public string refreshAccessToken(string currentAccessToken)
        {
            FacebookClient fbClient = new FacebookClient();
            Dictionary<string, object> fbParams = new Dictionary<string, object>();
            fbParams["client_id"] = Storage.appID;
            fbParams["grant_type"] = "fb_exchange_token";
            fbParams["client_secret"] = Storage.appSecret;
            fbParams["fb_exchange_token"] = currentAccessToken;
            JsonObject publishedResponse = fbClient.Get("/oauth/access_token", fbParams) as JsonObject;
            return publishedResponse["access_token"].ToString();
        }

        /// <summary>
        /// Posts to facebook
        /// </summary>
        /// <param name="currentAccessToken">Current access token</param>
        /// <param name="content">Post content</param>
        /// <param name="img">Image</param>
        /// <param name="me">Media Element</param>
        /// <param name="timeInc">The time increment</param>
        /// <param name="basedOffLastPostTime">From what time</param>
        /// <param name="name">Name</param>
        /// <param name="source">Source</param>
        /// <param name="selectedPageID">PageID the person will post to</param>
        public void postToFacebook(string pageAccessToken, string content, Image img, MediaElement me, double timeToIncrement, bool basedOffLastPostTime, string source, string name, string selectedPageID)
        {
            FacebookClient fbClient = new FacebookClient(pageAccessToken);
            fbClient.AppId = Storage.appID;
            fbClient.AppSecret = Storage.appSecret;
            Dictionary<string, object> fbParams = new Dictionary<string, object>();
            if (basedOffLastPostTime)
            {
                if (Storage.lastScheduledPostDate != null)
                {
                    Storage.lastScheduledPostDate = ((DateTime)Storage.lastScheduledPostDate).AddMinutes(timeToIncrement);
                    fbParams.Add("scheduled_publish_time", Facebook.DateTimeConvertor.ToUnixTime((DateTime)Storage.lastScheduledPostDate));
                }
                else
                {
                    Storage.lastScheduledPostDate = DateTime.Now;
                    Storage.lastScheduledPostDate = ((DateTime)Storage.lastScheduledPostDate).AddMinutes(timeToIncrement);
                    fbParams.Add("scheduled_publish_time", Facebook.DateTimeConvertor.ToUnixTime((DateTime)Storage.lastScheduledPostDate));
                }
            }
            else
            {
                fbParams.Add("scheduled_publish_time", Facebook.DateTimeConvertor.ToUnixTime(customScheduledDateTime));
                customScheduledDateTime = customScheduledDateTime.AddMinutes(timeToIncrement);
                Console.WriteLine(customScheduledDateTime);
            }
            fbParams.Add("published", "0");
            DateTime dt = DateTime.Now;
            dt = dt.AddMonths(6);
            if (dt <= Storage.lastScheduledPostDate)
            {
                cancelOperation = true;
                MessageBox.Show("Scheduling stopped!\nTime has exceeded Facebook's 6 month limit.", "ERROR");
                return;
            }

            if (img != null)
            {
                FacebookMediaObject fmo = new FacebookMediaObject { FileName = name, ContentType = "image/jpeg" };
                byte[] imgBytes = File.ReadAllBytes(@source.Replace("file:///", ""));
                fmo.SetValue(imgBytes);
                fbParams.Add("source", fmo);
                fbParams["message"] = content;
                try
                {
                    var publishedResponse = fbClient.Post(string.Format("/{0}/photos", selectedPageID), fbParams); //Put pageID here; also must be dynamic for combobox
                }
                catch(FacebookOAuthException authEx)
                {
                    MessageBox.Show(string.Format("Something went wrong while scheduling your post, contact the developer.\nError code: {0}\nMsg: {1}", authEx.ErrorCode, authEx.Message), "ERROR");
                }
            }
            else if(me != null)
            {
                FacebookMediaObject fmo = new FacebookMediaObject { FileName = name, ContentType = "video/mp4" };
                byte[] meBytes = File.ReadAllBytes(source.ToString().Replace("file:///", ""));
                fmo.SetValue(meBytes);
                fbParams.Add("source", fmo);
                fbParams.Add("description", content);
                try
                {
                    var publishedResponse = fbClient.Post(string.Format("/{0}/videos", selectedPageID), fbParams);
                }
                catch(FacebookOAuthException authEx)
                {
                    MessageBox.Show(string.Format("Something went wrong while scheduling your post, contact the developer.\nError code: {0}\nMsg: {1}", authEx.ErrorCode, authEx.Message), "ERROR");
                }
            }
            else
            {
                fbParams["message"] = content;
                try
                {

                }
                catch(FacebookOAuthException authEx)
                {
                    MessageBox.Show(string.Format("Something went wrong while scheduling your post, contact the developer.\nError code: {0}\nMsg: {1}", authEx.ErrorCode, authEx.Message), "ERROR");
                }
            }
        }

        private int currInd = 0;

        /// <summary>
        /// Adds new type of image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSelectContent_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Content Paths";
            ofd.Filter = "Image Files (.jpg .png)|*.jpg;*.png;";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                foreach (string s in ofd.FileNames)
                {
                    contentBox.Items.Add(new DirectoryListing { Name = "", Path = @s, TDimX = "350", TDimY = "50", IDim = "50", IDimV = "0", PathV = "" });
                }
                if (ofd.FileNames.Length > 0)
                {
                    contentBox.SelectedIndex = ++currInd;
                }
            }
        }

        /// <summary>
        /// Adds new type of video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddVideoContent_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Content Paths";
            ofd.Filter = "Video Files (.3gpp)|*.3gpp;";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                foreach (string s in ofd.FileNames)
                {
                    contentBox.Items.Add(new DirectoryListing { Name = "", PathV = @s, TDimX = "350", TDimY = "50", IDimV = "50", IDim = "0", Path = "" });
                }
                if (ofd.FileNames.Length > 0)
                {
                    contentBox.SelectedIndex = ++currInd;
                }
            }
        }

        /// <summary>
        /// Adds text content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddTextContent_Click(object sender, RoutedEventArgs e)
        {
            contentBox.Items.Add(new DirectoryListing { Name = "", Path = "", TDimX = (contentBox.Width - 50).ToString(), TDimY = "50", IDim = "0", IDimV = "0", PathV = "" });
            contentBox.SelectedIndex = contentBox.Items.Count - 1;
            currInd = contentBox.SelectedIndex;
        }

        private bool cancelOperation = false;
        /// <summary>
        /// Starts the upload in a new thread (the list of items of to-post content)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonBeginUpload_Click(object sender, RoutedEventArgs e)
        {
            groupBox.IsEnabled = false;
            groupBox1.IsEnabled = false;
            await Task.Factory.StartNew(() => doUpload());
            groupBox.IsEnabled = true;
            groupBox1.IsEnabled = true;
        }

        /// <summary>
        /// Will cancel the upload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancelUpload_Click(object sender, RoutedEventArgs e)
        {
            cancelOperation = true;
        }

        /// <summary>
        /// The upload function. For everything it uploads, it'll pop it from the list visually so the user knows how much has been uploaded
        /// This is why it's in a new thread as well. Mainly for UX, and so it can be cancelled/UI is interactable
        /// </summary>
        private void doUpload()
        {
            cancelOperation = false;
            Dispatcher.Invoke(new Action(async () => 
            {
                double inc;
                if (double.TryParse(textBoxTimeIncrement.Text, out inc))
                {
                    if (inc > 10)
                    {
                        if (radioButtonBasedOffCustomTime.IsChecked == true)
                        {
                            if (comboBoxDay.SelectedIndex != 0 && comboBoxMonth.SelectedIndex != 0 && comboBoxYear.SelectedIndex != 0)
                            {
                                DateTime dt = DateTime.Now;
                                IFormatProvider culture = new System.Globalization.CultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name, true);
                                customScheduledDateTime = DateTime.Parse(string.Format("{0}/{1}/{2} {3}:{4}:{5}", comboBoxMonth.SelectedValue, comboBoxDay.SelectedValue, comboBoxYear.SelectedValue, dt.Hour, dt.Minute, dt.Second), culture, System.Globalization.DateTimeStyles.AssumeLocal);
                            }
                            else
                            {
                                MessageBox.Show("Please input your custom date before attempting to upload.", "ERROR");
                                return;
                            }
                        }
                        int count = contentBox.Items.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if(cancelOperation)
                            {
                                cancelOperation = false;
                                progressBarUpload.Value = 0;
                                radioButtonBasedOffLastPostTime.Content = "From " + Storage.lastScheduledPostDate.ToString();
                                doPost();
                                return;
                            }
                            ListBoxItem item = (ListBoxItem)(contentBox.ItemContainerGenerator.ContainerFromIndex(((bool)checkBoxRemoveOnUpload.IsChecked) ? 0 : i));
                            TextBox tb = FindDescendant<TextBox>(item);
                            Image img = FindDescendant<Image>(item);
                            MediaElement me = FindDescendant<MediaElement>(item);
                            bool basedOffLastPostTime = false, textPost = true;
                            if (radioButtonBasedOffLastPostTime.IsChecked == true)
                            {
                                basedOffLastPostTime = true;
                            }
                            if (img.Source != null)
                            {
                                if (!string.IsNullOrEmpty(img.Source.ToString()))
                                {
                                    Storage.fbAuthToken = await refreshTokenAndPostToFacebook(Storage.fbAuthToken, tb.Text, img, null, inc, basedOffLastPostTime, img.Name, img.Source.ToString(), comboBox.SelectedItem.ToString().Split('-')[1]);
                                    textPost = false;
                                }
                            }
                            if (me.Source != null)
                            {
                                if (!string.IsNullOrEmpty(me.Source.ToString()))
                                {
                                    Storage.fbAuthToken = await refreshTokenAndPostToFacebook(Storage.fbAuthToken, tb.Text, null, me, inc, basedOffLastPostTime, me.Name, me.Source.ToString(), comboBox.SelectedItem.ToString().Split('-')[1]);
                                    textPost = false;
                                }
                            }
                            if (textPost)
                            {
                                Storage.fbAuthToken = await refreshTokenAndPostToFacebook(Storage.fbAuthToken, tb.Text, null, null, inc, basedOffLastPostTime, "", "", comboBox.SelectedItem.ToString().Split('-')[1]);
                            }
                            double val = i*100 / count;
                            await progressBarUpload.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate 
                            {
                                progressBarUpload.Value = val;
                                return null;
                            }), null);
                            if ((bool)checkBoxRemoveOnUpload.IsChecked)
                            {
                                contentBox.Items.RemoveAt(0);
                            }
                        }
                        await progressBarUpload.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
                        {
                            progressBarUpload.Value = 100;
                            return null;
                        }), null);
                        MessageBox.Show("Done!", "Facebook Automator");
                        progressBarUpload.Value = 0;
                        radioButtonBasedOffLastPostTime.Content = "From " + Storage.lastScheduledPostDate.ToString();
                        doPost();
                    }
                    else
                    {
                        MessageBox.Show("Time must be greater than 10 minutes (i.e: 10.1)", "ERROR");
                    }
                }
                else
                {
                    MessageBox.Show("Your inputted Time to Increment must be numeric.", "ERROR");
                }
            }));
        }

        /// <summary>
        /// You need to find the descendent because I'm looking through a list of items within a list.
        /// As in I have a box, and in that box I'm looking for a media element, image, and/or text.
        /// </summary>
        /// <typeparam name="T">Either a Media Element, image, or textbox</typeparam>
        /// <param name="obj">Parent</param>
        /// <returns></returns>
        public T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            // Check if this object is the specified type
            if (obj is T)
                return obj as T;

            // Check for children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return null;

            // First check all the children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
            }

            // Then check the childrens children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = FindDescendant<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null && child is T)
                    return child as T;
            }

            return null;
        }

        /// <summary>
        /// Custom time, then show more options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonBasedOffCustomTime_Checked(object sender, RoutedEventArgs e)
        {
            if(comboBoxDay != null)
            {
                comboBoxDay.Visibility = Visibility.Visible;
                comboBoxMonth.Visibility = Visibility.Visible;
                comboBoxYear.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Based off last post time (stored in db), then don't do anything
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonBasedOffLastPostTime_Checked(object sender, RoutedEventArgs e)
        {
            if(comboBoxDay != null)
            {
                comboBoxDay.Visibility = Visibility.Hidden;
                comboBoxMonth.Visibility = Visibility.Hidden;
                comboBoxYear.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Shows a different tab (makeshift)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTabBulk_Click(object sender, RoutedEventArgs e)
        {
            buttonTabBulk.IsEnabled = false;
            buttonTabCustom.IsEnabled = true;
            gridBulkSchedule.Visibility = Visibility.Visible;
            gridCustomSchedule.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows a different tab (makeshift)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTabCustom_Click(object sender, RoutedEventArgs e)
        {
            buttonTabCustom.IsEnabled = false;
            buttonTabBulk.IsEnabled = true;
            gridBulkSchedule.Visibility = Visibility.Collapsed;
            gridCustomSchedule.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Bottom right hyperlink url. On click, go to the specified url
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        /// <summary>
        /// Preview the media (kinda broken)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vid_MediaOpened(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < contentBox.Items.Count; i++)
            {
                ListBoxItem item = (ListBoxItem)(contentBox.ItemContainerGenerator.ContainerFromIndex(i));
                MediaElement me = FindDescendant<MediaElement>(item);
                if(me.LoadedBehavior.Equals(MediaState.Manual))
                {
                    me.Play();
                    me.Stop();
                    me.LoadedBehavior = MediaState.Pause;
                }
            }
        }

        /// <summary>
        /// When you right click a box, it'll remove the item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contentBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            contentBox.Items.Remove(contentBox.SelectedItem);
        }

        /// <summary>
        /// Clicking on a box will select that item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            item.IsSelected = true;
        }

        /// <summary>
        /// Async task to actually do the post to facebook
        /// </summary>
        private async void doPost()
        {
            if (Storage.lastScheduledPostDate != null)
            {
                IEnumerable<KeyValuePair<string, string>> queries = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("lastScheduledPostDate", Storage.lastScheduledPostDate.ToString()),
                    new KeyValuePair<string, string>("email", Storage.email)
                };
                HttpContent q = new FormUrlEncodedContent(queries);
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage hrm = await client.PostAsync("http://localhost:8080/fbSetLastScheduledPostDate.php", q))
                    {
                        if (hrm.IsSuccessStatusCode)
                        {
                            using (HttpContent cont = hrm.Content)
                            {
                                string it = await cont.ReadAsStringAsync();
                                if (!it.ToLower().Equals("success"))
                                {
                                    MessageBox.Show("Error updating last scheduled post date. Contact the dev if the issue persists.");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Some simple combobox logic for datetime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int m, d, y;
            if(comboBoxDay.SelectedItem != null && comboBoxMonth.SelectedItem != null && comboBoxYear.SelectedItem != null)
            {
                if (int.TryParse(comboBoxMonth.SelectedItem.ToString(), out m) && int.TryParse(comboBoxDay.SelectedItem.ToString(), out d) && int.TryParse(comboBoxYear.SelectedItem.ToString(), out y))
                {
                    DateTime now = DateTime.Now;
                    try
                    {
                        DateTime dt = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}:{4}:{5}", m, d, y, now.Hour, now.Minute, now.Second));
                        if (dt < now)
                        {
                            MessageBox.Show("The time you have selected is in the past.", "ERROR");
                        }
                        now = now.AddMonths(6);
                        if (dt >= now)
                        {
                            MessageBox.Show("The time you have selected is more than 6 months.\nFacebook does not allow scheduling past 6 months.\nFor more information, contact the developer.", "ERROR");
                            ComboBox cb = (ComboBox)sender;
                            cb.SelectedIndex = 0;
                        }
                    }
                    catch (FormatException ex)
                    {
                    }
                }
            }
        }
    }

    /// <summary>
    /// DirectoryListing for XAML->C#
    /// </summary>
    public class DirectoryListing
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string PathV { get; set; }
        public string IDim { get; set; }
        public string IDimV { get; set; }
        public string TDimX { get; set; }
        public string TDimY { get; set; }
    }

    public class CustomDirectorylisting
    {

    }
}
