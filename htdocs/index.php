<?php

     // 1. Extract pagename from URI
    $url = strip_tags($_SERVER["REQUEST_URI"]);    $url_array = explode("/", $url);    array_shift($url_array); //the first one is empty anyway
    if($url_array[0] == "neo") // test environment?
        array_shift($url_array);
    $page = implode("/", $url_array);

    // 3. Check for index page
    if($page == "")
        $page = "news.html";


    function navitem($title, $target, $current)
    {
        print '<p class="navitem';
        if($target == $current)
            print 'active';
        print "\"><a href=\"$target\">$title</a></p>\n";
    }

?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">

<head>
	<meta http-equiv="content-type" content="text/html;charset=iso-8859-1" />
	<title>Neo (.NET Entity Objects)</title>
	<link rel="shortcut icon" href="/favicon.ico" type="image/x-icon" />
	<link rel="stylesheet" title="Standard" type="text/css" href="std.css" media="screen" />
</head>

<body>

<div id="header">
    <table width="99%">
        <tr>
            <td valign="middle" align="left">
                   <a href="news.html"> 
	               <img src="neo-94x59.png" width="94" height="59" alt="Neo Logo" style="border:0px" />
	               </a>
            </td>
            <td valign="middle" align="right">
                <a href="http://codehaus.org"><img src="http://codehaus.org/codehaus-small.gif" width="209" height="40" alt="codehaus.org" style="border:0px" /></a>
            </td>
        </tr>
    </table>
</div>

<div id="metanav">
    <a class="mn" href="http://docs.codehaus.org/display/NEO">Wiki</a> &middot; 
    <a class="mn" href="http://jira.codehaus.org/secure/BrowseProject.jspa?id=10377">Bugs</a> &middot
    <a class="mn" href="http://archive.neo.codehaus.org/">Lists</a> &middot
    <a class="mn" href="http://cvs.neo.codehaus.org/">CVS</a>
</div>

<div id="menubox">
	<p class="navtitle">About Neo</p>
	   <?php navitem("News", "news.html", $page) ?>
	   <?php navitem("5 Reasons", "whyneo.html", $page) ?>
	   <?php navitem("FAQ", "faq.html", $page) ?>
	   <?php navitem("Team", "team.html", $page) ?>
	   <?php navitem("Roadmap", "roadmap.html", $page) ?>
	<p class="navtitle">Using Neo</p>
	   <?php navitem("Download", "download.html", $page) ?>
	   <?php navitem("Quickstart", "quickstart.html", $page) ?>
	   <?php navitem("Documents", "docs.html", $page) ?>
	   <?php navitem("Resources", "resources.html", $page) ?>
</div>

<div id="contentbox">
    <?php @ require_once("$page"); ?>
</div>

</body>
</html>
