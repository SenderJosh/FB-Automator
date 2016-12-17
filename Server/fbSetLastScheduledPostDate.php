<?php
//Whenever a post occurs, we will set the last scheduled post date so it's always incremental to the last one
//Unless specified otherwise.
$lastScheduledPostDate = $_POST['lastScheduledPostDate'];
$email = $_POST['email'];

$conn = new mysqli("localhost","root","","test");
if($conn->connect_error)
{
	die("Cannot Connect");
}
if($conn->query("UPDATE fbautomator SET lastScheduledPostDate='$lastScheduledPostDate' WHERE email='$email'"))
{
	echo "Success";
}
else
{
	echo "Error";
}
?>