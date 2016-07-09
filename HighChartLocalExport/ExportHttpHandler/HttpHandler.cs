using System.Web;

namespace HighChartLocalExport.ExportHttpHandler
{
    /// <summary>
    /// 实现IHttpHandler接口
    /// 需要在WebConfg中system.web下httpHandlers节点配置此handler
    /// </summary>
    public class HttpHandler : IHttpHandler
    {
        /// <summary>
        /// 接口属性 指示是否可调用
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// 接口方法 处理指向此Handler的请求
        /// </summary>
        /// <param name="context">Http请求.</param>
        public void ProcessRequest(HttpContext context)
        {
            // 调用ExportChart类处理此请求
            ExportChart.ProcessExportRequest(context);
        }
    }
}