<%@ Page Language="C#" AutoEventWireup="true" Title="JavaScript and CSS minification" CodeBehind="Default.aspx.cs" MasterPageFile="~/Site.Master" Inherits="AzureJobs.Web.Minification.Default" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <link href="minification.css" rel="stylesheet" />
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <header>
        <h1>JavaScript and CSS minification</h1>
        <p>Upload a .js or .css file, look at the file size, then refresh the browser</p>
    </header>

    <form id="form1" runat="server">
        <asp:FileUpload runat="server" ID="files" AllowMultiple="true" accept=".js, .css" />

        <asp:Button Text="Upload" runat="server" ID="btnUpload" OnClick="Upload_Click" />
        <asp:Button Text="Delete all files" OnClientClick="return confirm('Are you sure?')" runat="server" OnClick="btnClear_Click" />
    </form>

    <br /><br />

    <asp:HyperLink runat="server" ID="aFile" />
    <pre runat="server" class="images" id="preResult"></pre>
</asp:Content>
