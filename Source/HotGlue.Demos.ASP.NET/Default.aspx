﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HotGlue.Demos.ASP.NET.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>HotGlue Test</title>
    <%= HotGlue.Script.Reference("/Scripts/Module1/app.js") %>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    Test
    </div>
    </form>
</body>
</html>