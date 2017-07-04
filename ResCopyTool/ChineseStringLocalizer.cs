using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using Sce.Atf;

namespace ResCopyTool
{
    [Export(typeof(IInitializable))]
    class ChineseStringLocalizer : StringLocalizer, IInitializable
    {
        public ChineseStringLocalizer()
        {
        }

        static string Convert(string s, string context)
        {
            string ret;
            bool enable = m_strMap.TryGetValue( s, out ret );
            return (enable ? ret : s);
        }

        public override string Localize(string s, string context)
        {
            return (Convert(s, context));
        }

        static private readonly Dictionary<string, string> m_strMap = new Dictionary<string,string>();

        void IInitializable.Initialize()
        {
            Register();
        }

        public static void Register()
        {
            m_strMap["New"] = "新建";
            m_strMap["Open"] = "打开";
            m_strMap["Close"] = "关闭";
            m_strMap["Copy"] = "拷贝";
            m_strMap["Save"] = "保存";
            m_strMap["Save As"] = "另存为";
            m_strMap["Exit"] = "退出";

            m_strMap["File"] = "文件";
            m_strMap["Edit"] = "编辑";
            m_strMap["View"] = "视图";
            m_strMap["Window"] = "窗口";
            m_strMap["Help"] = "帮助";

            m_strMap["File Commands"] = "文件指令";
            m_strMap["Editing Commands"] = "编辑指令";
            m_strMap["Window Management Commands"] = "窗口管理";
            m_strMap["Help Commands"] = "帮助指令";

            m_strMap["Ready"] = "就绪";

            m_strMap["Recent Files"] = "最近浏览";
            
            m_strMap["Output"] = "输出";

            //Property Editor
            m_strMap["Property Editor"] = "属性编辑";
            m_strMap["Edits selected object properties"] = "编辑选中信息";

            //Standard File Command
            m_strMap["File extension not supported"] = "不支持此类型文件";
            m_strMap["A file with that name is already open"] = "同名文件已打开";
            m_strMap["Document Saved"] = "文件已保存";
            m_strMap["Document Saved As"] = "文件已另存为";
            m_strMap["All documents saved"] = "所有文件已保存";
            m_strMap["Couldn't save all documents"] = "不能保存所有文件";
            m_strMap["Creates a new {0} document"] = "创建一个新 {0} 文件";
            m_strMap["Open an existing {0} document"] = "打开已有 {0} 文件";
            m_strMap["Open {0}"] = "打开 {0}";
            m_strMap["New {0}"] = "创建 {0}";
            m_strMap["There was a problem opening the file"] = "打开文件失败";

            // Error Dialog Service
            m_strMap["Error!"] = "错误";
            m_strMap["Warning"] = "警告";
            m_strMap["Online Help"] = "在线文档";
            m_strMap["Keyboard Shortcuts"] = "快捷键";
            m_strMap["info"] = "提示";
            
            // Command Info
            m_strMap["Save the active file under a new name"] = "保存统计结果";
        }
    }
}
