using System;
using System.Web.UI;
using HighChartLocalExport.ExportHttpHandler;

namespace HighChartLocalExport
{
    public partial class Export : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ExportChart.ProcessExportRequest(Context);
            }
        }
    }
}