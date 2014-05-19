<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AzureJobs.Web.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Image uploader</title>
</head>
<body>

    <form id="form1" runat="server">
        <fieldset>
            <legend>Upload an image</legend>
            <asp:FileUpload runat="server" ID="files" AllowMultiple="true" />

            <br /><br />
            <asp:Button Text="Upload" runat="server" ID="btnUpload" OnClick="Upload_Click" />
            <asp:Button Text="Delete all images" OnClientClick="return confirm('Are you sure?')" runat="server" ID="btnClear" OnClick="btnClear_Click" />
        </fieldset>
    </form>

    <div runat="server" id="divImages"></div>
</body>
</html>
