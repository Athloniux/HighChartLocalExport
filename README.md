# HighChartLocalExport
HighChart图表本地导出服务 C#实现

##1.背景
HighChart本身提供了php版,java/Servlet版,java/Stuts2版,asp.NET版的本地导出服务,其中[asp.NET版](https://github.com/hcharts/Highcharts_Export_.Net)运行版本较旧(.Net Framework 2.0+、IIS 6.0+),并对图表中某些隐藏元素未进行处理,故编写了此版本实现HighChart图表本地导出服务.

##2.实现功能
- 使用HttpHandler处理导出
- JPEG,PNG,PDF,SVG四种格式的导出
- 隐藏的系列导出文件中不显示
- 增加默认宽度
- 系统编码转换

##3.引入文件
- [svg.dll](http://svg.codeplex.com/)
- [sharpPDF.dll](http://sharppdf.sourceforge.net/index.html)

##4.已知问题
- 标题过长时可能出现显示错误
- 屏蔽了.NET中默认的不安全Request代码检测

##5.计划添加
- aspx页响应导出请求

##6.参考项目
- [Export server for ASP.NET provided by Clément Agarini](https://github.com/imclem/Highcharts-export-module-asp.net)
- [Highcharts_Export_.Net by lanhouzi](https://github.com/lanhouzi/Highcharts_Export_.Net)
