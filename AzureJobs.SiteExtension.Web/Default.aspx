<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AzureJobs.SiteExtension.Web.Default" %>

<asp:Content ContentPlaceHolderID="body" runat="server">

    <header>        
        <h1>
            <a href="/"><asp:Literal runat="server" ID="name" /></a>
        </h1>
    </header>

    <asp:PlaceHolder runat="server" ID="error" Visible="false">
        <mark>No data has been collected yet</mark>
    </asp:PlaceHolder>

    <form runat="server" id="success">

        <ul>
            <li><span>Files processed: </span><asp:Literal runat="server" ID="filesProcessed" /></li>
            <li><span>Files optimized: </span><asp:Literal runat="server" ID="filesOptmized" /></li>
            <li><span>Total savings: </span><asp:Literal runat="server" ID="totalSavings" /> bytes</li>
        </ul>

        <div id="buttons">
            <asp:Button runat="server" ID="btnDelete" Text="Delete log" OnClick="btnDelete_Click" OnClientClick="return confirm('Are you sure?')" />
            <button onclick="location.href='?download=1'" type="button" download>Download log</button>
        </div>

        <asp:GridView
            runat="server"
            ID="grid"
            CssClass="grid"
            EnableViewState="false"
            AutoGenerateColumns="false"
            AllowPaging="true"
            AllowSorting="true"
            PageSize="10"
            SelectMethod="grid_GetData"
            ItemType="AzureJobs.SiteExtension.Web.Result"
            BorderStyle="None" >

            <Columns>
                <asp:BoundField DataField="Date" SortExpression="Date" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                <asp:TemplateField HeaderText="File" SortExpression="ShortFileName">
                    <ItemTemplate>
                        <span title="<%# Item.FileName %>"><%# Item.ShortFileName %></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Before" SortExpression="Original">
                    <ItemTemplate>
                        <%# Item.Original.ToString("#,#0") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="After" SortExpression="Optimized">
                    <ItemTemplate>
                        <%# Item.Optimized.ToString("#,#0") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Saving" SortExpression="Saving">
                    <ItemTemplate>
                        <%# Item.Saving.ToString("#,#0") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Percent" SortExpression="Percent" HeaderText="Percent" DataFormatString="{0:#0.0}%" />
            </Columns>
        </asp:GridView>
    </form>

</asp:Content>
