using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;

namespace FBAutomator
{
    /// <summary>
    /// This will only really need to be used if you want to manage a mass state of users on a db and restrict access to only those registered
    /// It's not necessary to have, but if removed you'll need to make further edits in the program to accomodate this missing piece (anything that interacts with SQL)
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            email.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(username_MouseLeftButtonDown), true);
            email.AddHandler(FrameworkElement.GotFocusEvent, new RoutedEventHandler(username_GotFocus), true);
            password.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(password_MouseLeftButtonDown), true);
            password.AddHandler(FrameworkElement.GotFocusEvent, new RoutedEventHandler(password_GotFocus), true);
            //DateTime dt1 = Convert.ToDateTime("5/27/2016 " + "2:22:22 PM"); -- Works; DateTime.Now.toString() is in that format as well
            //Console.WriteLine(dt1.ToString());
        }

        private bool changeOnceUser = false, changeOncePass = false;
        /// <summary>
        /// When clicked, attempt to login to the db and verify if the user is really a user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            hideAllLabel();
            if (!(string.IsNullOrEmpty(email.Text) || string.IsNullOrEmpty(password.Password)))
            {
                isAlive("http://myurl/myDBLogin.php");
                Storage.email = email.Text;
                new Splash().Show();
                this.Close();
            }
            else
            {
                invalidCredentialsLabel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Async task to check if the user is actually a user
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="user">User (email)</param>
        /// <param name="pass">Password (pass)</param>
        private async void postInfo(string url, string user, string pass)
        {
            IEnumerable<KeyValuePair<string, string>> queries = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("email", user),
                new KeyValuePair<string, string>("password", pass)
            };
            HttpContent q = new FormUrlEncodedContent(queries);
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage hrm = await client.PostAsync(url, q))
                {
                    using (HttpContent cont = hrm.Content)
                    {
                        string items = await cont.ReadAsStringAsync();
                        hideAllLabel();
                        if (items.Contains("Success"))
                        {
                            Storage.email = user;
                            new Splash().Show();
                            this.Close();
                        }
                        else if(items.Contains("ConnectE"))
                        {
                            failedConnectLabel.Visibility = Visibility.Visible;
                        }
                        else if(items.Contains("Fail"))
                        {
                            invalidCredentialsLabel.Visibility = Visibility.Visible;
                        }
                        Console.WriteLine(items);
                    }
                }
            }
        }

        /// <summary>
        /// Simply checks if the URL is alive
        /// </summary>
        /// <param name="url">URL</param>
        private async void isAlive(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage hrm = await client.GetAsync(url))
                    {
                        if (hrm.IsSuccessStatusCode)
                        {
                            connectedLabel.Visibility = Visibility.Visible;
                            postInfo(url, email.Text, password.Password);
                        }
                        else
                        {
                            failedConnectLabel.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                failedConnectLabel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Hides all labels
        /// </summary>
        private void hideAllLabel()
        {
            connectedLabel.Visibility = Visibility.Hidden;
            failedConnectLabel.Visibility = Visibility.Hidden;
            invalidCredentialsLabel.Visibility = Visibility.Hidden;
            noInformationLabel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Kind of a way to simulate 'preview text' like you see in HTML
        /// Done by click; it's a bit redundant since we have the event for GotFocus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void password_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!changeOncePass)
            {
                password.Password = "";
                changeOncePass = true;
            }
        }

        /// <summary>
        /// Kind of a way to simulate 'preview text' like you see in HTML
        /// This way is done by focus (in other words, they used tabs to navigate and not clicks)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!changeOnceUser)
            {
                email.Text = "";
                changeOnceUser = true;
            }
        }

        /// <summary>
        /// Kind of a way to simulate 'preview text' like you see in HTML
        /// This way is done by focus (in other words, they used tabs to navigate and not clicks)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!changeOncePass)
            {
                password.Password = "";
                changeOncePass = true;
            }
        }

        /// <summary>
        /// Kind of a way to simulate 'preview text' like you see in HTML
        /// Done by click; it's a bit redundant since we have the event for GotFocus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void username_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!changeOnceUser)
            {
                email.Text = "";
                changeOnceUser = true;
            }
        }
    }
}
