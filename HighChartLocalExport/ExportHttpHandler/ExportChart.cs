using System;
using System.Web;

namespace HighChartLocalExport.ExportHttpHandler
{
    internal static class ExportChart
    {
        /// <summary>
        /// 处理Http请求 写入HttpResponse中
        /// </summary>
        /// <param name="context">Http请求</param>
        internal static void ProcessExportRequest(HttpContext context)
        {
            //非空验证 只处理POST请求
            if (context != null && context.Request.HttpMethod == "POST")
            {
                //Request对象
                var request = context.Request;

                //处理Post值 非空验证
                var filename = request.Form["filename"];//文件名
                var type = request.Form["type"];//导出类型 (jpg,png,pdf,svg)
                var svg = request.Form["svg"];//svg字符串
                //宽度 默认为900
                int width = (Int32.TryParse(request.Form["width"], out width) && width != 0) ? width : 900;
                
                //非空验证
                if (filename != null && type != null  && svg != null)
                {
                    //创建Exporter对象
                    var export = new Exporter(filename, type, width, svg);

                    //导出文件流写入写入HttpResponse中
                    export.WriteToHttpResponse(context.Response);

                    //结束此request
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                    context.Response.End();
                }
            }
        } 
    }
}