using RegularTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace RegularTool.ViewModel
{
    public class MainVm : VmBase
    {
        private static readonly object lockObj = new object();
        private static MainVm instance;
        public static MainVm Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null) instance = new MainVm();
                    }
                }
                return instance;
            }
        }
        private MainVm()
        {
            try
            {
                var jsonContent = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "grammar.json");
                GrammarModels = JsonConvert.DeserializeObject<ObservableCollection<GrammarModel>>(jsonContent);
                SetRealIcon(GrammarModels);
            }
            catch (Exception ex)
            {
                MessageBox.Show("grammar.json文件不存在或内容格式有误！" + ex.Message);
                GrammarModels.Add(new GrammarModel()
                {
                    IsGrouping = true,
                    IsExpanded = false,
                    Header = "正则语法说明",
                    Children = new ObservableCollection<GrammarModel>() {
                    new GrammarModel(){Header="转义符",Content="\\\n将下一个字符标记为特殊字符或字面值。例如\"n\"与字符\"n\"匹配。\"\\n\"与换行符匹配。序列\"\\\\\"与\"\\\"匹配对面，\"\\(\"与\"(\"匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="段落开头", Content="^\n匹配输入的开始位置。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="段落结尾", Content="$\n匹配输入的结尾。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="0次或多次", Content="*\n匹配前一个字符零次或几次。例如，\"zo*\"可以匹配\"z\"、\"zoo\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="1次或多次", Content="+\n匹配前一个字符一次或多次。例如，\"zo+\"可以匹配\"zoo\",但不匹配\"z\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="最短匹配", Content="?\n匹配前一个字符零次或一次。例如，\"a?ve?\"可以匹配\"never\"中的\"ve\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="任意字符", Content=".\n匹配换行符以外的任何字符(如果勾选多行匹配时可以匹配换行）。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="并列匹配", Content="(x|y)\n匹配x或y，括号内用|分隔。例如\"z|food\"可匹配\"z\"或\"food\"。\"(z|f)ood\"匹配\"zoo\"或\"food\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="固定匹配次数", Content="{n}\nn为非负的整数。匹配恰好n次。例如，\"o{2}\"不能与\"Bob中的\"o\"匹配，但是可以与\"foooood\"中的前两个o匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="至少匹配次数", Content="{n,}\nn为非负的整数。匹配至少n次。例如，\"o{2,}\"不匹配\"Bob\"中的\"o\"，但是匹配\"foooood\"中所有的o。\"o{1,}\"等价于\"o+\"。\"o{0,}\"等价于\"o*\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="范围匹配次数", Content="{n,m}\nm和n为非负的整数。匹配至少n次，至多m次。例如，\"o{1,3}\"匹配\"fooooood\"中前三个o。\"o{0,1}\"等价于\"o?\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="字符集", Content="[xyz]\n一个字符集。与括号中字符的其中之一匹配。例如，\"[abc]\"匹配\"plain\"中的\"a\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="排除字符集", Content="[^xyz]\n一个否定的字符集。匹配不在此括号中的任何字符。例如，\"[^abc]\"可以匹配\"plain\"中的\"p\".", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="字符集范围", Content="[a-z]\n表示某个范围内的字符。与指定区间内的任何字符匹配。例如，\"[a-z]\"匹配\"a\"与\"z\"之间的任何一个小写字母字符。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="排除字符集范围", Content="[^m-z]\n否定的字符区间。与不在指定区间内的字符匹配。例如，\"[m-z]\"与不在\"m\"到\"z\"之间的任何字符匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\b", Content=" \n与单词的边界匹配，即单词与空格之间的位置。例如，\"er\\b\"与\"never\"中的\"er\"匹配，但是不匹配\"verb\"中的\"er\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\B", Content=" \n与非单词边界匹配。\"ea*r\\B\"与\"neverearly\"中的\"ear\"匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\d", Content=" \n与一个数字字符匹配。等价于[0-9]。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\D", Content=" \n与非数字的字符匹配。等价于[^0-9]。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\f", Content=" \n与分页符匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\n", Content=" \n与换行符字符匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\r", Content=" \n与回车字符匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\s", Content=" \n与任何白字符匹配，包括空格、制表符、分页符等。等价于\"[\\f\\n\\r\\t\\v]\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\S", Content=" \n与任何非空白的字符匹配。等价于\"[^\\f\\n\\r\\t\\v]\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\t", Content=" \n与制表符匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\v", Content=" \n与垂直制表符匹配。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\w", Content=" \n与任何单词字符匹配，包括下划线。等价于\"[A-Za-z0-9_]\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\W", Content=" \n与任何非单词字符匹配。等价于\"[^A-Za-z0-9_]\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\num", Content=" \n匹配num个，其中num为一个正整数。引用回到记住的匹配。例如，\"(.)\\1\"匹配两个连续的相同的字符。\"。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\n", Content=" \n匹配n，其中n是一个八进制换码值。八进制换码值必须是1,2或3个数字长。例如，\"\\11\"和\"\\011\"都与一个制表符匹配。\"\\0011\"等价于\"\\001\"与\"1\"。八进制换码值不得超过256。否则，只有前两个字符被视为表达式的一部分。允许在正则表达式中使用ASCII码。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="\\xn", Content=" \n匹配n，其中n是一个十六进制的换码值。十六进制换码值必须恰好为两个数字长。例如，\"\\x41\"匹配\"A\"。\"\\x041\"等价于\"\\x04\"和\"1\"。允许在正则表达式中使用ASCII码。", Icon="./Icons/grammar.png"},
                      new GrammarModel(){ Header="(pattern)", Content=" \n与模式匹配并记住匹配。匹配的子字符串可以从作为结果的Matches集合中使用Item[0]...[n]取得。如果要匹配括号字符(和)，可使用\"\\(\"或\"\\)\"。", Icon ="./Icons/grammar.png"},

                }
                });
                GrammarModels.Add(new GrammarModel()
                {
                    IsGrouping = true,
                    Header = "正则常用例程",
                    Children = new ObservableCollection<GrammarModel>() {
                    new GrammarModel(){
                        IsGrouping=true,
                        Header="网页相关",
                        Children=new ObservableCollection<GrammarModel>(){
                             new GrammarModel(){Header="匹配HTML标记",Content="<(\\S*?)[^>]*>.*?</\\1>|<.*? />\n评注：网上流传的版本太糟糕，上面这个也仅仅能匹配部分，对于复杂的嵌套标记依旧无能为力", Icon="./Icons/grammar.png"},
                              new GrammarModel(){Header="匹配链接地址",Content="href *= *['\"]* (\\S +)[\"']评注：匹配网页上的链接地址", Icon="./Icons/grammar.png"},
                              new GrammarModel(){Header="匹配链接地址和标题",Content="\\<a href *= *['\"]* (\\S +)[\"'].*\\>(.[^\\<]*)?\\</a>\n评注：匹配网页上的链接地址及标题", Icon="./Icons/grammar.png"},
                              new GrammarModel(){Header="匹配网址URL",Content="[a-zA-z]+://[^\\s]*\n评注：网上流传的版本功能很有限，上面这个基本可以满足需求", Icon="./Icons/grammar.png"},
                              new GrammarModel(){Header="匹配网站域名",Content="[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\\.?\n评注：一个完整的域名，由根域、顶级域、二级、三级……域名构成，每级域名之间用点分开，每级域名由字母、数字和减号构成（第一个字母不能是减号），不区分大小写，长度不超过63。\n很显然，单独的名字可以由正则表达式[a-zA-Z0-9][-a-zA-Z0-9]{0,62}来匹配，而完整的域名至少包括两个名字（比如google.com，由google和com构成），最后可以有一个表示根域的点（在规范中，最后有一个点的才是完整域名，但一般认为包括两个以上名字的域名也是完整域名，哪怕它后面没有点）。", Icon="./Icons/grammar.png"},
                              new GrammarModel(){Header="匹配IP地址",Content="\\d+\\.\\d+\\.\\d+\\.\\d+", Icon="./Icons/grammar.png"},
                              new GrammarModel(){Header="匹配Email地址",Content="\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*\n评注：表单验证时很实用", Icon="./Icons/grammar.png"},
                              new GrammarModel(){Header="匹配图片引用地址",Content="\\<img.*src *= *['\"]* (\\S +)[\"'].*\\>\n评注：可用在网页图片提取等地方", Icon="./Icons/grammar.png"},
                              new GrammarModel(){ Header="匹配帐号是否合法", Content="匹配帐号是否合法(字母开头，允许5-16字节，允许字母数字下划线)：^[a-zA-Z][a-zA-Z0-9_]{4,15}$\n评注：表单验证时很实用", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配密码是否合法", Content="^[a-zA-Z]\\w{5,17}$\n评注：密码正确格式为：以字母开头，长度在6-18之间，只能包含字符、数字和下划线。", Icon="./Icons/grammar.png"},
                        }
                    },
                     new GrammarModel(){
                        IsGrouping=true,
                        Header="号码相关",
                        Children=new ObservableCollection<GrammarModel>(){
                            new GrammarModel(){ Header="匹配国内电话号码", Content="\\d{3}-\\d{8}|\\d{4}-\\d{7}\n评注：匹配形式如 0511-4405222 或 021-87888822", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配手机号码", Content="^[+]{0,1}(\\d){1,3}[ ]?([-]?((\\d)|[ ]){1,12})+\n评注：校验手机号码：必须以数字开头，除数字外，可含有“-”", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配腾讯QQ号", Content="[1-9][0-9]{4,}\n评注：腾讯QQ号从10000开始", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配中国邮政编码", Content="[1-9]\\d{5}(?!\\d)\n评注：中国邮政编码为6位数字", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配身份证号码", Content="\\d{15}|\\d{18}\n评注：中国的身份证为15位或18位", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配特定的数字", Content="^[1-9]\\d*$　 　 //匹配正整数\n^-[1-9]\\d*$ 　 //匹配负整数\n^-?[1-9]\\d*$　　 //匹配整数"+
"\n^[1-9]\\d*|0$　 //匹配非负整数(正整数 + 0)"+
"\n^-[1-9]\\d*|0$　　 //匹配非正整数(负整数 + 0)"+
"\n^[1-9]\\d*\\.\\d*|0\\.\\d*[1-9]\\d*$　　 //匹配正浮点数"+
"\n^-([1-9]\\d*\\.\\d*|0\\.\\d*[1-9]\\d*)$　 //匹配负浮点数"+
"\n^-? ([1 - 9]\\d *\\.\\d *| 0\\.\\d *[1 - 9]\\d *| 0 ?\\.0 +| 0)$　 //匹配浮点数"+
"\n^[1 - 9]\\d *\\.\\d *| 0\\.\\d *[1 - 9]\\d *| 0 ?\\.0 +| 0$　　  //匹配非负浮点数(正浮点数 + 0)"+
"\n^(-([1 - 9]\\d *\\.\\d *| 0\\.\\d *[1 - 9]\\d *))| 0 ?\\.0 +| 0$ //匹配非正浮点数(负浮点数 + 0)"+
"\n评注：处理大量数据时有用，具体应用时注意修正", Icon=" ./Icons/grammar.png"},
                        }
                    },
                      new GrammarModel(){
                        IsGrouping=true,
                        Header="匹配字符串",
                        Children=new ObservableCollection<GrammarModel>()
                        {
                            new GrammarModel(){ Header="匹配中文字符", Content="[\\u4e00-\\u9fa5]\n评注：匹配中文还真是个头疼的事，有了这个表达式就好办了", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配双字节字符", Content="包括汉字在内：[^\\x00-\\xff]\n评注：可以用来计算字符串的长度(一个双字节字符长度计2，ASCII字符计1)", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配空白行", Content="\\n\\s*\\r\n评注：可以用来删除空白行", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配首尾空白字符", Content="^\\s*|\\s*$\n评注：可以用来删除行首行尾的空白字符(包括空格、制表符、换页符等等)，非常有用的表达式", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配特定字符串", Content="^[A-Za-z]+$　　//匹配由26个英文字母组成的字符串"+
"\n^[A-Z]+$　　//匹配由26个英文字母的大写组成的字符串"+
"\n^[a-z]+$　　//匹配由26个英文字母的小写组成的字符串"+
"\n^[A-Za-z0-9]+$　　//匹配由数字和26个英文字母组成的字符串:"+
"\n^\\w+$　　//匹配由数字、26个英文字母或者下划线组成的字符串"+
"\n评注：最基本也是最常用的一些表达式", Icon="./Icons/grammar.png"},
                            new GrammarModel(){ Header="匹配多行文本", Content="开始关键字([\\s\\S]*?)结束关键字"+
"\n----应用实例：-----------------"+
"\n<table>"+
"\n<tr>"+
"\n<td>单元格1</td><td>单元格2</td>"+
"\n</tr>"+
"\n<tr>"+
"\n<td>单元格3</td><td>单元格4</td>"+
"\n</tr>"+
"\n</table>"+
"\n正则表达式.创建 (“<tr>([\\s\\S]*?)</tr>”)", Icon="./Icons/grammar.png"},
                        }
                    },
                }
                });
                var grammarStr = JsonConvert.SerializeObject(GrammarModels, Formatting.Indented);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "grammar.json", grammarStr);
            }
        }

        public ObservableCollection<GrammarModel> GrammarModels { get; set; }

        private GrammarModel _SelectGrammar;

        public GrammarModel SelectGrammar
        {
            get { return _SelectGrammar; }
            set
            {
                _SelectGrammar = value;

                RaisePropertyChanged(() => SelectGrammar);
            }
        }

        private void SetRealIcon(IEnumerable<GrammarModel> grammarModels)
        {
            foreach (var item in grammarModels)
            {
                if(item.Icon!=null)
                    item.Icon= item.Icon.Replace("./", AppDomain.CurrentDomain.BaseDirectory);
                if (item.Children != null)
                    SetRealIcon(item.Children);
            }
        }
    }

}
