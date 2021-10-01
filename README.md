# 文本定位器
![主界面](https://images.gitee.com/uploads/images/2021/1001/193814_76bc7801_995027.png "主界面")
![索引重建](https://images.gitee.com/uploads/images/2021/1001/193844_2bdbd85a_995027.png "索引重建")
![预览](https://images.gitee.com/uploads/images/2021/1001/193954_8c7e90ce_995027.png "预览")
![分词](https://images.gitee.com/uploads/images/2021/1001/194106_c65124b9_995027.png "分词")

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
5. 使用细节说明：
- 自动分词：数据库表结构 -> 数据库表结构,数据,库表,结构（勾选匹配全词后不分词）
- 手动分词：数据库 表 结构 -> 数据库,表,结构（空格作为分隔符）
- 仅文件名：关键词不匹配内容和路径，只匹配文件名
- 文件类型：筛选不同类型的文件

#### 依赖组件
1. [Rubyer](https://gitee.com/wuyanxin1028/rubyer-wpf)
2. [Lucene.Net](http://lucenenet.apache.org)
3. [PanGu.Lucene.Analyzer](https://github.com/NeverCL/PanGu.Lucene.Analyzer)
4. [NPOI](https://github.com/nissl-lab/npoi)
5. [Spire.Office](https://www.e-iceblue.com/Introduce/spire-office-for-net.html)
6. [Microsoft.Office.Interop.Excel](https://www.nuget.org/packages/Microsoft.Office.Interop.Excel/)
7. [Microsoft.Office.Interop.Word](https://www.nuget.org/packages/Microsoft.Office.Interop.Word/)
8. [Microsoft.Office.Interop.PowerPoint](https://www.nuget.org/packages/Microsoft.Office.Interop.PowerPoint/)