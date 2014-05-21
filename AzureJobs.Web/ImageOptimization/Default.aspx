<%@ Page Language="C#" AutoEventWireup="true" Title="Image Optimization" MasterPageFile="~/Site.Master" CodeBehind="Default.aspx.cs" Inherits="AzureJobs.Web.Default2" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <link href="imageoptimization.css" rel="stylesheet" />
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <header>
        <h1>Image optimization</h1>
        <p>Upload an image, look at the file size, then refresh the browser</p>
    </header>

    <form id="form1" runat="server">
        <asp:FileUpload runat="server" ID="files" AllowMultiple="true" accept="image/png, image/jpeg, image/gif" />

        <asp:Button Text="Upload" runat="server" ID="btnUpload" OnClick="Upload_Click" />
        <asp:Button Text="Delete all images" OnClientClick="return confirm('Are you sure?')" runat="server" ID="btnClear" Enabled="false" OnClick="btnClear_Click" />
    </form>

    <div runat="server" class="images" id="divImages"></div>
</asp:Content>