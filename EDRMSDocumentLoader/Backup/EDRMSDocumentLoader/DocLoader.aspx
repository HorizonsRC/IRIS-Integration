<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DocLoader.aspx.cs" Inherits="EDRMSDocumentLoader.DocLoader" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .style1
        {
            font-family: "Comic Sans MS";
            font-size: large;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="style1">
    
        <asp:Panel ID="Panel1" runat="server" BorderColor="#999999" BorderStyle="Solid" 
            BorderWidth="2px" Height="130px">
            <br />
            <br />
            &nbsp;
            <asp:Label ID="labelError" runat="server" Text="Loading...." 
                Font-Names="Verdana" ForeColor="Black"></asp:Label>
        </asp:Panel>
    </div>
    </form>
</body>
</html>
