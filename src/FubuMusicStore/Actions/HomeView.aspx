<%@ Page Inherits="FubuMusicStore.Actions.HomeView" Language="C#" AutoEventWireup="true" %>
<ul>
<% foreach (var album in Model.Albums)
{
  %>
  <li><%= album.Name %></li>
  <%
} %>
</ul>