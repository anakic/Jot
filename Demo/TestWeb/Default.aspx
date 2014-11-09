<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="TestWeb._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Tracking.NET ASP.NET demo!
    </h2>
    <p>
        Press the button to increase the counter (the value is per-user, since the datastore is the user profile). 
        Tracking is configured in the global.asax file. Once tracking is configured, all that is needed to make properties persistent is 
        implementing ITrackable (marker interface) and applying the [Trackable] attributes on the desired properties of pages and other objects.
    </p>
    <p>
        In this demo I added a property (Counter) to the page and marked it with the [Trackable] attribute.
    </p>

    <p>
        <asp:Label ID="lblCounter" runat="server" Text=""></asp:Label>
        <asp:Button ID="Button1" runat="server" Text="++" onclick="Button1_Click"/>
    </p>

    <p>
        Try creating additional users, and log in to see how it affects the counter.
    </p>
</asp:Content>
