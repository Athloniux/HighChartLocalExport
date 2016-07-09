using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using sharpPDF;
using Svg;
using Svg.Transforms;

namespace HighChartLocalExport.ExportHttpHandler
{
    internal class Exporter
    {

        #region 属性

        /// <summary>
        /// 默认导出文件名
        /// </summary>
        private const string DefaultFileName = "Chart";

        /// <summary>
        /// 导出文件的HTTP Content头描述
        /// </summary>
        private string ContentDisposition { get;  set; }

        /// <summary>
        /// 导出文件的HTTP Content类型
        /// </summary>
        private string ContentType { get; set; }

        /// <summary>
        /// 导出文件名
        /// </summary>
        private string FileName { get; set; }

        /// <summary>
        /// 图表名 默认即为文件名(不包含扩展名) 用于PDF文件导出
        /// </summary>
        private string Name { get; set; }

        /// <summary>
        ///  SVG源文件字符串
        /// </summary>
        private string Svg { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        private int Width { get; set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="type">导出类型</param>
        /// <param name="width">宽度</param>
        /// <param name="svg">SVG字符串</param>
        internal Exporter(string fileName,string type,int width, string svg)
        {
            //名称 宽度 直接赋值
            Name = fileName;
            Width = width;

            //文件名 文件HTTP Content类型 svg字符串 解码后赋值
            fileName = HttpUtility.UrlDecode(fileName);
            var urlDecode = HttpUtility.UrlDecode(type);
            if (urlDecode != null) ContentType = urlDecode.ToLower();
            Svg = HttpUtility.UrlDecode(svg);

            //文件扩展名
            string extension;

            //由文件HTTP Content类型确定
            switch (ContentType)
            {
                case "image/jpeg":
                    extension = "jpg";
                    break;

                case "image/png":
                    extension = "png";
                    break;

                case "application/pdf":
                    extension = "pdf";
                    break;

                case "image/svg+xml":
                    extension = "svg";
                    break;

                // 不支持的类型 抛出异常
                default:
                    throw new ArgumentException(string.Format("不支持'{0}'类型文件导出.", type));
            }

            // 拼合文件名.
            FileName = string.Format("{0}.{1}",string.IsNullOrEmpty(fileName) ? DefaultFileName : fileName,extension);

            // 拼合HTTP Content头.
            ContentDisposition = string.Format("attachment; filename={0}", FileName);
        }

        #endregion

        #region 方法

        #region 解码SVG对象 (核心方法)

        /// <summary>
        /// 解码SVG字符串获得SVG对象
        /// 引用了Svg.dll
        /// </summary>
        /// <returns>SVG对象</returns>
        private SvgDocument CreateSvgDocument()
        {
            //将SVG字符串转换成XML对象
            var xml = new XmlDocument();
            xml.LoadXml(Svg);

            #region 处理不应显示在导出文件中的series和tooltip

            //获取所有g标签对象
            var nodeListAllg = xml.GetElementsByTagName("g");
            //用于保存被隐藏的series和不应显示tooltip
            //最后统一移除
            var dic = new Dictionary<int, XmlNode[,]>();
            //计数器
            var i = 0;
            //处理SVG
            foreach (XmlNode xNod in nodeListAllg)
            {
                i++;
                if (xNod.Attributes != null)
                {
                    var xmlvisibility = xNod.Attributes.GetNamedItem("class");
                    //找出series中的series节点
                    if (xmlvisibility != null && xmlvisibility.Value == "highcharts-series-group")
                    {
                        foreach (XmlNode xNod2 in xNod.ChildNodes)
                        {
                            i++;
                            if (xNod2.Attributes != null)
                            {
                                var xmlvisibility1 = xNod2.Attributes.GetNamedItem("visibility");
                                if (xmlvisibility1 != null && xmlvisibility1.Value == "hidden")
                                {
                                    //将此节点信息保存
                                    var xmln = new XmlNode[1, 2];
                                    xmln[0, 0] = xNod;
                                    xmln[0, 1] = xNod2;
                                    dic.Add(i, xmln);
                                }
                            }
                        }
                    }
                    else if (xmlvisibility != null && xmlvisibility.Value == "highcharts-tooltip")
                    {
                        var xmln = new XmlNode[1, 2];
                        xmln[0, 0] = xml.FirstChild;
                        xmln[0, 1] = xNod;
                        dic.Add(i, xmln);
                    }
                }

            }

            //移除隐藏的series和tooltip
            foreach (var a in dic)
            {
                a.Value[0, 0].RemoveChild(a.Value[0, 1]);
            } 

            #endregion


            //处理后的svg字符串
            Svg = xml.OuterXml;

            //构造SVG对象
            SvgDocument svgDoc;

            //通过内存流转换为SVG对象
            using (var streamSvg = new MemoryStream(
                Encoding.UTF8.GetBytes(Svg)))
            {
                svgDoc = SvgDocument.Open<SvgDocument>(streamSvg);
            }

            //计算宽度 高度 比例等SVG对象信息
            svgDoc.Transforms = new SvgTransformCollection();
            var scalar = Width / svgDoc.Width;
            svgDoc.Transforms.Add(new SvgScale(scalar, scalar));
            svgDoc.Width = new SvgUnit(svgDoc.Width.Type, svgDoc.Width * scalar);
            svgDoc.Height = new SvgUnit(svgDoc.Height.Type, svgDoc.Height * scalar);

            return svgDoc;
        }

        #endregion

        #region 写入流

        /// <summary>
        /// 写入输入出流
        /// </summary>
        /// <param name="outputStream">输出流</param>
        private void WriteToStream(Stream outputStream)
        {
            //根据类型进行输出
            switch (ContentType)
            {
                case "image/jpeg":
                    //直接调用Svg.dll中的方法输出至HttpResponse
                    CreateSvgDocument().Draw().Save(outputStream, ImageFormat.Jpeg);
                    break;

                case "image/png":
                    //PNG格式要求可寻址的流
                    //使用MemoryStream
                    using (var seekableStream = new MemoryStream())
                    {
                        //调用Svg.dll中的方法输出至seekableStream
                        CreateSvgDocument().Draw().Save(seekableStream, ImageFormat.Png);
                        //seekableStream再输出至HttpResponse
                        seekableStream.WriteTo(outputStream);
                    }
                    break;

                case "application/pdf":
                    //将SVG对象转换为BMP图像
                    var bmp = CreateSvgDocument().Draw();
                    //调用sharpPDF.dll创建PDF文件
                    //第二个参数 作者 可选 目前为空
                    var doc = new pdfDocument(Name, null);
                    //将图片作为PDF中的一页插入
                    var page = doc.addPage(bmp.Height, bmp.Width);
                    page.addImage(bmp, 0, 0);
                    //调用sharpPDF.dll中的方法输出至HttpResponse
                    doc.createPDF(outputStream);
                    break;

                case "image/svg+xml":
                    //SVG文件 直接输出即可
                    using (var writer = new StreamWriter(outputStream))
                    {
                        writer.Write(Svg);
                        writer.Flush();
                    }

                    break;

                default:
                    //不支持的类型 抛异常
                    throw new InvalidOperationException(string.Format("不支持'{0}'类型文件导出.", ContentType));
            }
            //清除缓冲区
            outputStream.Flush();
        }

        #endregion

        #region 写入HttpResponse (对外方法)

        /// <summary>
        /// 将导出文件内容写入HttpResponse
        /// </summary>
        /// <param name="httpResponse">HttpResponse对象</param>
        internal void WriteToHttpResponse(HttpResponse httpResponse)
        {
            //清除缓冲区数据
            httpResponse.ClearContent();
            httpResponse.ClearHeaders();
            //加入当前导出文件的ContentType和Content头
            httpResponse.ContentType = ContentType;
            httpResponse.AddHeader("Content-Disposition", ContentDisposition);
            //写入输出流
            WriteToStream(httpResponse.OutputStream);
        }

        #endregion

        #endregion
              
    }
}