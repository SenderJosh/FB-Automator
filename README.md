# FB-Automator
A WPF application that uses the Facebook API to automate scheduling to pages to make the process of managing a large page with many administrators simple, and so content streams remain consistent.
This should also make it so anyone, even those with tight schedules between work, school, or life events, have an easier time integrating their dream of having a large Facebook page into their life seamlessly.

API used: https://github.com/facebook-csharp-sdk/facebook-csharp-sdk

Theme: Metro Dark Theme by Infragistics

# Technicals and details of this project

This project was originally intended to have a business model behind it. This is why the project was mapped around the SQL connections handled through the php files, which should be put on a server for the client to interact with via POST requests. I'm not too well natured in this field so I hold my tongue when using words like cURL and Relax APIs, for good reason, as I think they are incredibly different on the back end. In any case, the database is necessary and the php files are there, but the login information must be modified. With the database, you get the email, pageids the user manages, a hashed password (uses password_hash function/bcrypt with the cost currently being 11 *which should be modified depending on how powerful your server is*.

This project utilizes multi-threading techniques to handle bulk scheduling and any web interaction that waits for responses, as to keep the UI interactable, and have the posting be cancelled if the user wants. This also allows to visually display to the user how many posts have been scheduled, and what posts are left. The overall importance behind this is if they have perhaps made a mistake in a post and would like to correct it, there must be a way to cancel the post so they won't have to go through the hassle of searching for it manually on Facebook.
That would make everything done in this project fairly redundant in my mind, due to the possibility of it creating more work due to that simple lack of functionality.

There are two types of ways to upload to Facebook. The first is bulk, the second is custom schedule. Each come with their own sub-options to allow the user to specify how exactly they want to schedule their posts, which may contain media (video) and text, images and text, or just text posts.

Note that the API used is few years old, and I had to do a bit of research into how the current supported APIs work (that aren't for C# WPF applications) in order to access the things I need, and to work around deprecated stuff. With that in mind, should you manipulate this project yourself, be wary that messing with logging in and refreshing the user's token and the page tokens are not entirely recommended.

# Setting the project up for yourself.

1. Since this project was originially intended to have a business model behind it, there is a necessity behind a database as mentioned above.
  **Ensure the SQL database holds these properties:**
  - Name: email | Type: text | Collation: latin1_swedish_ci | Null: No | Default: None
  - Name: password | Type: text | Collation: latin1_swedish_ci | Null: No | Default: None
  - Name: FBPageName | Type: text | Collation: latin1_swedish_ci | Null: No | Default: None
  - Name: FBPageIDs | Type: text | Collation: latin1_swedish_ci | Null: No | Default: None
  - Name: name | Type: text | Collation: latin1_swedish_ci | Null: No | Default: None
  - Name: lastScheduledPostDate | Type: text | Collation: latin1_swedish_ci | Null: No | Default: None
  - Name: id | Type: int(11) | Null: No | Default: None | AUTO_INCREMENT
2. Modify the PHP files for the connection parameters to match your credentials. At the moment they match the default localhost options if you've installed something like PHPMyAdmin ("localhost", "root", "" | Your credentials might look like: "myepicdomain.com", "dbroot", "superepicpassword123").
3. You'll need to make your own Facebook account if you haven't already and sign up to be a developer: https://developers.facebook.com/
  1. Once done, add a new app to your developer account and then name it whatever you want. I'd recommend using something similar if not the same thing as your Facebook page, for obvious reasons.
  2. Go to the dashboard of your app and get copy the App Secret and App ID into the corresponding sections of the Storage.cs file  
4. On line 37 in the MainWindow.xaml.cs file, change the url to your server's url to the myDBLogin.php file. Example: http://myurl/myDBLogin.php
5. On line 615 of the FBWindow.xaml.cs file, change the url to your server's url to the fbSetLastScheduledPostDat.php file
6. On line 39 of the Splash.xaml.cs file, change the url to your server's url to the fbGetInfo.php file
7. From that, all you'll have to do is make a registration page which is something I didn't provide since I am not well versed in CSS or front-end web development in general. It shouldn't be too hard. The file fbregister.php is already made for you to handle the actions required.

Every function is documented with what it specifically does. Hope that helps.
