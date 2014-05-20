<%@ Page Language="C#" AutoEventWireup="true" Title="Image Optimization" MasterPageFile="~/Site.Master" CodeBehind="Default.aspx.cs" Inherits="AzureJobs.Web.Default" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <style>
        h1, p {
            margin: 0;
        }

        form {
            margin-top: 1em;
        }

        .images {
            -moz-columns: 200px auto;
            -webkit-columns: 200px auto;
            columns: 200px auto;
            -moz-column-gap: 50px;
            -webkit-column-gap: 50px;
            column-gap: 50px;
            margin-top: 3em;
        }


        span {
            position: relative;
            display: block;
            text-align: center;
            margin-bottom: 1em;
            -webkit-column-break-inside: avoid;
            overflow: hidden;
        }

            span:before {
                content: attr(title);
                background: rgba(0, 0, 0, 0.46);
                color: white;
                padding: 5px;
                font-size: 12px;
                position: absolute;
                bottom: 5px;
                text-align: center;
                z-index: 3;
            }

        img {
            max-width: 100%;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <header>
        <h1>Image optimization</h1>
        <p>Upload an image, look at the file size, then refresh the browser</p>
    </header>

    <form id="form1" runat="server">
        <asp:FileUpload runat="server" ID="files" AllowMultiple="true" />

        <asp:Button Text="Upload" runat="server" ID="btnUpload" OnClick="Upload_Click" />
        <asp:Button Text="Delete all images" OnClientClick="return confirm('Are you sure?')" runat="server" ID="btnClear" OnClick="btnClear_Click" />
    </form>

    <div runat="server" class="images" id="divImages"></div>

</asp:Content>
