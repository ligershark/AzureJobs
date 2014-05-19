<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AzureJobs.Web.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Image uploader</title>
    <style>
        body {
            max-width: 960px;
            margin: 2em auto;
            font: 20px 'century gothic';
        }

        div {
            -moz-columns: 4 auto;
            -webkit-columns: 4 auto;
            columns: 4 auto;
            -moz-column-gap: 50px;
            -webkit-column-gap: 50px;
            column-gap: 50px;
            margin-top: 1em;
        }

        img {
            height: 100%;
            width: 100%;
        }

        span {
            position: relative;
            display: block;
            /*text-align: center;*/
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
                right: 0;
                display: block;
                z-index: 3;
            }
    </style>
</head>
<body>

    <h1>Image optimization</h1>

    <form id="form1" runat="server">
        <fieldset>
            <legend>Upload an image</legend>
            <asp:FileUpload runat="server" ID="files" AllowMultiple="true" />

            <br />
            <br />
            <asp:Button Text="Upload" runat="server" ID="btnUpload" OnClick="Upload_Click" />
            <asp:Button Text="Delete all images" OnClientClick="return confirm('Are you sure?')" runat="server" ID="btnClear" OnClick="btnClear_Click" />
        </fieldset>
    </form>

    <div runat="server" id="divImages"></div>
</body>
</html>
