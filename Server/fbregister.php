<?php
//Used for registering. Should be self explanatory
$password = $_POST['password'];
$FBUser = $_POST['FBUser'];
$FBPass = $_POST['FBPass'];
$FBPageName = $_POST['FBPageName'];
$FBPageURL = $_POST['FBPageURL'];
$email = $_POST['email'];

$conn = new mysqli("localhost","root","","test");
if($conn->connect_error)
{
	die("Cannot Connect");
}
if(mysqli_num_rows(mysqli_query("SELECT FROM fbautomator WHERE email='$email'")) >= 1)
{
	die("Exists");
}
if($conn->query("INSERT INTO fbautomator (password, FBUser, FBPass, FBPageName, FBPageURL, email) VALUES ('" . $password . "', '" . $FBUser . "', '" . $FBPass . "', '" . $FBPageName . "', '" . $FBPageURL . "', '" . $email . "')") === TRUE)
{
	echo "Success";
}
else
{
	echo "Fail";
}