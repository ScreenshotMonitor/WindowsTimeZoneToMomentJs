<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Import Namespace="System.Web.Script.Serialization" %>
<script runat="server">
    private TimeZoneInfo LocalTimeZone { get; set; }
    private string TimeZoneIdJsSerialized { get; set; }

    private void Page_Init(object sender, EventArgs e)
    {
        LocalTimeZone = TimeZoneInfo.Local;
        string timeZoneId = LocalTimeZone.Id;
        TimeZoneIdJsSerialized = new JavaScriptSerializer().Serialize(timeZoneId);
    }
</script>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="/Scripts/moment.min.js"></script>
    <script src="/Scripts/moment-timezone.min.js"></script>
    <script>         
        <%= Pranas.WindowsTimeZoneToMomentJs.TimeZoneToMomentConverter.GenerateAddMomentZoneScript(LocalTimeZone, 2000, 2100)%>
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <script>
                var script = 'moment().tz(<%=TimeZoneIdJsSerialized%>).format()';

                document.write(script + ' = ' + eval(script));
            </script>
        </div>
    </form>
</body>
</html>
