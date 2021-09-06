# 文本定位器
![主界面](https://images.gitee.com/uploads/images/2021/0906/224436_07453e0f_995027.png "主界面")

#### 介绍
基于.net实现的本地文档的全文索引定位器，根据关键词搜索定位本地文档内容。便于查找历史文档时节省时间。

#### 软件架构
本地单机软件。
* WPF实现的UI（RubyerUI组件）
* Lucene.Net实现的索引（PanGu.Lucene.Analyzer分词器）
* NPOI、Spire、Microsoft.Office实现的文档内容读取


#### 安装教程
* 运行环境基于.net freamwork 4.6（需要安装此环境才能运行，win10+默认有此环境）
* 发布版下载解压可用

#### 使用说明
1. 双击文件夹设置自己需要搜索的文件夹
2. 点击“重建”按钮创建文档索引，更新文档索引点击“优化”按钮。
3. 索引创建结束后，搜索框输入关键词后，回车或者点击搜索按钮。搜索结果列表会显示搜索结果列表
4. 点击文档，右侧预览框会显示文档内容

#### 依赖组件
1. [Rubyer](https://gitee.com/wuyanxin1028/rubyer-wpf)
2. [Lucene.Net](http://lucenenet.apache.org)
3. [PanGu.Lucene.Analyzer](https://github.com/NeverCL/PanGu.Lucene.Analyzer)
4. [NPOI](https://github.com/nissl-lab/npoi)
5. [Spire.Office](https://www.e-iceblue.com/Introduce/spire-office-for-net.html)
6. [Microsoft.Office.Interop.Excel](https://www.nuget.org/packages/Microsoft.Office.Interop.Excel/)
7. [Microsoft.Office.Interop.Word](https://www.nuget.org/packages/Microsoft.Office.Interop.Word/)
8. [Microsoft.Office.Interop.PowerPoint](https://www.nuget.org/packages/Microsoft.Office.Interop.PowerPoint/)