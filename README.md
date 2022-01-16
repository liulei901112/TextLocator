# 文本定位器
![主界面](%E4%B8%BB%E7%95%8C%E9%9D%A2.png)
![预览](%E9%A2%84%E8%A7%88.png)
![类型筛选](%E7%B1%BB%E5%9E%8B%E7%AD%9B%E9%80%89.png)
![自动分词](%E8%87%AA%E5%8A%A8%E5%88%86%E8%AF%8D.png)
![手动分词](%E6%89%8B%E5%8A%A8%E5%88%86%E8%AF%8D.png)
![索引重建确认](%E7%B4%A2%E5%BC%95%E9%87%8D%E5%BB%BA%E7%A1%AE%E8%AE%A4.png)
![重建索引](%E9%87%8D%E5%BB%BA%E7%B4%A2%E5%BC%95.png)

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
