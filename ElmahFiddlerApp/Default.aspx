<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ElmahFiddlerApp.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>
    <p>This is a demo of the <a href="http://bugsquash.blogspot.com/2010/03/fiddler-output-for-elmah.html">ElmahFiddler</a> module.</p>
    <ol>
        <li>Configure the errorMail section of this web.config with your email account settings</li>
        <li>(Optional) Configure the errorMailSAZ section of this web.config with the desired SAZ generation parameters</li>
        <li>Click one of the buttons below to generate an exception in this sample application. The exception will be logged by ELMAH to your email with all request information as a SAZ attachment.</li>
    </ol>
    <form action="" method="get">
        <input type="hidden" name="field" value="value" />
        <input type="submit" value="Trigger an exception in a GET request" />
    </form>
    <form action="" method="post">
        <input type="hidden" name="field" value="value" />
        <input type="submit" value="Trigger an exception in a POST request" />
    </form>
</body>
</html>
