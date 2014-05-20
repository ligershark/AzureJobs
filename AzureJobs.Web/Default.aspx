<%@ Page Language="C#" AutoEventWireup="true" Title="Image Optimization" MasterPageFile="~/Site.Master" CodeBehind="Default.aspx.cs" Inherits="AzureJobs.Web.Default" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <header>
        <h1>LigerShark Azure Jobs</h1>
        <p>NuGet packages that adds additional functionality to any website</p>
    </header>

    <div role="main" id="default">
        <article>
            <h2>Background jobs</h2>
            <p>
                Azure WebJobs runs in the background of your Azure Website 
                and doesn't interfere with the execution of your web application.
            </p>
        </article>
        <article>
            <h2>No code needed</h2>
            <p>
                The LigerShark Azure Jobs are easy to add to any web application 
                running in Azure Websites. No code is needed for the developer
                to add any Azure Jobs in the LigerShark collection.
            </p>
        </article>
        <article>
            <h2>Powered by NuGet</h2>
            <p>
                To add a LigerShark Azure Job, simply add the NuGet package of one
                or more of the available Azure Jobs and then publish your
                web application as normal.  
            </p>
        </article>
    </div>

</asp:Content>
