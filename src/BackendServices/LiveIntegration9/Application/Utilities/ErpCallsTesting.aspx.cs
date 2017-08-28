using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Dna.Ecommerce.LiveIntegration.Utilities
{
    public partial class ErpCallsTesting : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void SubmitButton_ServerClick(object sender, EventArgs e)
        {
            string input = inputArea.Value;

            string output;
            try
            {
                Stopwatch watch = Stopwatch.StartNew();

                output = Connector.RetrieveDataFromRequestString(input, true)?.OuterXml;

                watch.Stop();

                responseInfo.Text = $" (took {watch.ElapsedMilliseconds} ms)";

                File.WriteAllText(MapPath("/Files/AxResponse.txt"), output);
            }
            catch (Exception ex)
            {
                output = ex.ToString();
            }

            outputArea.Value = output;
        }
    }
}