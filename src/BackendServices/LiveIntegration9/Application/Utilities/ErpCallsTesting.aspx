<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ErpCallsTesting.aspx.cs" Inherits="Dna.Ecommerce.LiveIntegration.Utilities.ErpCallsTesting" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
         <div>
            <b>Request:</b>
        </div>
        <div>
            <textarea runat="server" id="inputArea" style="width:100%; min-height: 200px;" enableviewstate="false" />
        </div>
        <div style="margin-top: 20px;">
            <button runat="server" title="Submit" id="submitButton" onserverclick="SubmitButton_ServerClick">Submit</button>
        </div>
        <div style="margin-top: 20px;">
            <b>Response<asp:Literal runat="server" ID="responseInfo" ViewStateMode="Disabled" EnableViewState="false" />:</b>
        </div>
        <div>
            <textarea runat="server" id="outputArea" style="width:100%; min-height: 500px"  enableviewstate="false"/>
        </div>
    </div>
    </form>
</body>
</html>
