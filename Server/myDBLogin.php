<?php
//Login to the database, verify if the account exists or has the wrong credentials.
$user = $_POST['email'];
$pass = $_POST['password'];
$options = array('cost' => 11);
//user, pass, PID (16char)
$conn = new mysqli("localhost","senderz","S3fayh7sS&!DTF&ASD");
mysqli_select_db("fbautomator");
if($conn->connect_error)
{
	die("ConnectE");
}
$pass = password_hash($pass, PASSWORD_HASH, $options);
$result = mysqli_query($conn, "SELECT * FROM fbautomator WHERE email='$user' AND password='$pass'");
$success = false;
echo "Password: " . $pass;
echo "<br>";
while($row = mysqli_fetch_array($result))
{
	$success = true;
}
if($success == true)
{
	echo "Success";
}
else
{
	echo "Fail";
}
		

?>