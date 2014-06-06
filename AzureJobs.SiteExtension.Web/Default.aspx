<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AzureJobs.SiteExtension.Web.Default" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <header>
        <h1>Image Optimizer Dashboard</h1>
    </header>

    <nav>
        <ul id="menu" role="menu" runat="server">

        </ul>
    </nav>

    <form runat="server">
        <asp:GridView 
            runat="server" 
            ID="grid" 
            EnableViewState="false"
            AllowPaging="true"
            AllowSorting="true"
            PageSize="10"
            SelectMethod="grid_GetData" 
            ItemType="AzureJobs.SiteExtension.Web.Result" />
    </form>

</asp:Content>
