<?php
//Connect to the sql server. The output echoed is how I chose to relay back to the client
//The information for all the page name and page ids, as well as their corresponding last scheduled post dates
$email = $_POST['email'];

$conn = new mysqli("localhost","root","","test");

$result = $conn->query("SELECT * FROM fbautomator WHERE email='$email'");
while($row = mysqli_fetch_array($result))
{
	echo $row['name'] . "<br>";
	echo $row['FBPageName'] . "<br>";
	echo $row['FBPageIDs'] . "<br>";
	echo $row['lastScheduledPostDate'];
}
?>