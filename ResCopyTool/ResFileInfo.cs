using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Sce.Atf;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace ResCopyTool
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ResFileInfo))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ResFileInfo : IInitializable
    {
        private string m_name = "";
        private string m_dir = "";
        private Utility.DataType m_dataType = Utility.DataType.DT_NULL;
        private string m_fullName = "";
        private bool m_isAtom = true;
        private List<ResFileInfo> m_subFiles = null;
        public bool IsAtom
        {
            get
            {
                return m_isAtom;
            }
        }
        public string Name
        {
            get
            {
                return m_name;
            }
        }
        
        [ImportingConstructor]
        public ResFileInfo()
        {

        }


        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding tree control to panel</summary>
        void IInitializable.Initialize()
        {
        }

        #endregion

        public static List<ResFileInfo> Copy(IEnumerable<ResFileInfo> arr)
        {
            List<ResFileInfo> list = new List<ResFileInfo>();
            foreach(ResFileInfo info in arr)
            {
                string fullName = info.FullName;
                ResFileInfo file = new ResFileInfo();
                file.FullName = fullName;
                list.Add(file);
            }
            return list;
        }
        public string Dir
        {
            get { return m_dir; }
            //set { m_dir = value; }
        }

        public Utility.DataType DataType
        {
            get { return m_dataType; }
        }
        public string FullName
        {
            get { return m_fullName; }
            set
            {
                m_fullName = value;
                if (value.Length > 0)
                {
                    m_name = value.Substring(value.LastIndexOf("\\") + 1);
                    m_dir = Utility.GetDir(value);
                    m_dataType = Utility.GetDataType(value);
                    if (m_name.Length > "_cloth.bp3".Length)
                    {
                        string namePost = m_fullName.Substring(m_fullName.Length - "_cloth.bp3".Length);
                        if (namePost.Equals("_cloth.bp3") || namePost.Equals("_cloth.bp4") || namePost.Equals("_cloth.bp5"))
                        {
                            m_isAtom = true;
                            return;
                        }
                    }

                    m_isAtom = Utility.isAtomFile(m_dataType);
                }
            }
        }

        public List<string> GetXmlSubFiles(string buffer)
        {
            List<string> list = new List<string>();
            if (buffer.Contains(@"<SHADERLIB N"))
            {
                return GetMaterialSubFiles(buffer);
            }
            else if (buffer.Contains(@"<Scene"))
            {
                return GetSceneSubFiles(buffer);
            }
            else
            {
                return list;
            }
        }

        public List<string> GetMaterialSubFiles(string buffer)
        {
            List<string> list = new List<string>();

            foreach (Match match in Regex.Matches(buffer, @"\<TEXTURE DIRECTORY=\""(\S*.*?)"))
            {
                string str = match.Value;
                
                str = str.Substring(str.IndexOf("\"") + 1);
                str = str.Substring(0, str.Length - 1);
                //2zhounian_shd.xml里有的是空，增加特殊处理
                if (str.Length == 0)
                {
                    continue;
                }
                if (str.IndexOf(@"/") >= 0)
                {
                    str = str.Replace(@"/", @"\");
                }
                //得到相对路径./../Resources/Media/Shaders/cart\scenes\fengche\textures\wt_fengche_cao_02.tga
                int i = str.IndexOf("\\");
                if (str.IndexOf("basedir") >= 0)
                {
                    //diting_shd.xml中basedir/C_01.tga路径为resources\media\shaders\cart\Scenes\gongyong/C_01.tga
                    str = str.Substring(str.LastIndexOf("\\") + 1);
                    Regex regex = new Regex(@"[abcABC]{1}_0[12345]{1}.tga");
                    if (regex.IsMatch(str))
                    {
                        str = @".\Resources\Media\Shaders\cart\Scenes\gongyong\" + str;
                        str = RelativeToAbsolute(str, m_dir);
                    }
                    else
                    {
                        str = m_dir + @"\textures\" + str;
                    }
                }
                else if (i >= 0)
                {
                    //E:\exe\exe\resources\media\shaders\c_effect\c_effect_weiyan.spe中url="Cart\SpecialEffect\che_temp.dml"
                    if (str.IndexOf(@"Resources") < 0 && str.IndexOf(@"resources") < 0)
                    {
                        str = @".\Resources\Media\Shaders\" + str;
                    }
                    string str1 = RelativeToAbsolute(str, m_dir);
                    if (str1.Length==0)
                    {
                        Outputs.WriteLine(OutputMessageType.Error, m_fullName + "中，所依赖的文件：" + str + "路径中不包含resources或Resources，请检查后重试");
                        list.Add(str);
                        continue;
                    }
                    str = str1;
                }
                
                else
                {
                    //同级目录下的处理，此处str为XX.tga
                    str = m_dir + "\\" + str;
                }
                if (Utility.GetExtensionName(str).Equals("tga") && !File.Exists(str))
                {
                    str = str.Substring(0, str.LastIndexOf("tga")) + "dds";
                }
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            return list;
        }
        public List<string> GetSceneSubFiles(string buffer)
        {
            List<string> list = new List<string>();

            foreach (Match match in Regex.Matches(buffer, @"\<Mesh PATH=\""(\S*.*?)"))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf("\"") + 1);
                str = str.Substring(0, str.Length - 1);
                str = m_dir + "\\" + str;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }

            foreach (Match match in Regex.Matches(buffer, @"\<Shader PATH=\""(\S*.*?)"))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf("\"") + 1);
                str = str.Substring(0, str.Length - 1);
                str = m_dir + "\\" + str;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            return list;
        }
        public List<string> GetDmlSubFiles(string buffer)
        {
            List<string> list = new List<string>();
            foreach (Match match in Regex.Matches(buffer, @"\[SHADER]\r?\n(\S*.*?)"))
            {
                //MessageBox.Show(match.Value);
                string str = match.Value;
                str = str.Substring(str.IndexOf('\n') + 1);
                if (str.IndexOf(".xml") < 0)
                {
                    str += ".xml";
                }
                str = this.Dir + "\\" + str;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            foreach (Match match in Regex.Matches(buffer, @"\[GEOMETRY MESH]\r?\n(\S*.*?)"))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf('\n') + 1);
                if (str.IndexOf(".ase") < 0 && str.IndexOf(".ASE") < 0)
                {
                    str += ".ASE";
                }
                str = this.Dir + "\\" + str;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            return list;
        }
        //将依赖资源的包含resources的相对路径，依据资源本身（包含resources的绝对路径），得出要求的真实路径
        public static string RelativeToAbsolute(string src, string myPath)
        {
            string dstPath = myPath.Substring(0, myPath.IndexOf("resources"));
            //dstPath = "";
            int index = (src.IndexOf("resources") >= 0) ? src.IndexOf("resources") : src.IndexOf("Resources");
            if (index < 0)
            {
                return "";
            }
            string rel = src.Substring(index);
            if (rel.IndexOf(@"/")>=0)
            {
                rel = rel.Replace("/", "\\");
            }
            dstPath += rel;
            //MessageBox.Show(dstPath);
            return dstPath;
        }
        public List<string> GetEffSubFiles(string buffer)
        {
            List<string> list = new List<string>();
            foreach (Match match in Regex.Matches(buffer, @"<shaderName name=\""(\S*)\"""))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf('\"') + 1, str.Length - 2 - str.IndexOf('\"'));
                string strRelative = @"../resources/media/shaders/".Replace("/", "\\");
                if (str.IndexOf("../resources/") < 0)
                {
                    str = strRelative + str;
                }
                if (str.IndexOf(".xml") < 0)
                {
                    str += ".xml";
                }
                string str1 = RelativeToAbsolute(str, m_dir);
                if (str1.Length == 0)
                {
                    Outputs.WriteLine(OutputMessageType.Error, m_fullName + "中，所依赖的文件：" + str + "路径中不包含resources或Resources，请检查后重试");
                    list.Add(str);
                    continue;
                }
                str = str1;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            foreach (Match match in Regex.Matches(buffer, @"<ParticleModelName value=\""(\S*)\"""))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf('\"') + 1, str.Length - 2 - str.IndexOf('\"'));
                if (str.Length == 0)
                {
                    continue;
                }
                if (str.IndexOf("../resources/") < 0)
                {
                    str = @"../resources/media/shaders/" + str;
                }
                string str1 = RelativeToAbsolute(str, m_dir);
                if (str1.Length == 0)
                {
                    Outputs.WriteLine(OutputMessageType.Error, m_fullName + "中，所依赖的文件：" + str + "路径中不包含resources或Resources，请检查后重试");
                    list.Add(str);
                    continue;
                }
                str = str1;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            return list;
        }
        public string ModifyEffFilePath(string buffer, string dstPath)
        {
            foreach (Match match in Regex.Matches(buffer, @"<shaderName name=\""(\S*)\"""))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf('\"') + 1, str.Length - 2 - str.IndexOf('\"'));
                string str1 = str;
                if (str.IndexOf(@"/") >= 0)
                {
                    str1 = str1.Replace(@"/", @"\");
                }
                string dstStr = str1;
                int i = dstStr.LastIndexOf(@"\");
                if (i > 0)
                {
                    dstStr = dstStr.Substring(i + 1);
                }

                if (dstStr.Equals("effect.xml"))
                {
                    dstStr = "\"../../../" + dstPath + "/effect.xml\"";
                    buffer = buffer.Replace("\"effect.xml\"", dstStr);
                }
                else if (dstStr.Equals("effect"))
                {
                    dstStr = "\"../../../" + dstPath + "/effect.xml\"";
                    buffer = buffer.Replace("\"effect\"", dstStr);
                }
                else
                {
                    dstStr = dstPath + @"/" + dstStr;
                    if (dstStr.IndexOf(".xml") < 0)
                    {
                        dstStr += ".xml";
                    }
                    if (str.Length > 0 && buffer.IndexOf(str) >= 0)
                    {
                        buffer = buffer.Replace(str, dstStr);
                    }
                }
            }
            foreach (Match match in Regex.Matches(buffer, @"<ParticleModelName value=\""(\S*)\"""))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf('\"') + 1, str.Length - 2 - str.IndexOf('\"'));
                if (str.Length == 0)
                {
                    continue;
                }
                string str1 = str;
                if (str.IndexOf(@"/") >= 0)
                {
                    str1 = str1.Replace(@"/", @"\");
                }
                string dstStr = str1;
                int i = dstStr.LastIndexOf(@"\");
                if (i > 0)
                {
                    dstStr = dstStr.Substring(i + 1);
                }
                dstStr = dstPath + @"/" + dstStr;
                if(str.Length > 0 && buffer.IndexOf(str)>=0)
                {
                    buffer = buffer.Replace(str, dstStr);
                }
            }
            return buffer;
        }
        public List<string> GetSpeSubFiles(string buffer)
        {
            List<string> list = new List<string>();
            foreach (Match match in Regex.Matches(buffer, @"<Shader name=\""(\S*)\"""))
            {
                string str = match.Value;

                str = str.Substring(str.IndexOf("\"") + 1);
                str = str.Substring(0, str.Length - 1);
                if (str.Equals("effect") || str.Equals("effect.xml"))
                {
                    str = @"../resources/media/shaders/effect.xml";
                }
                if (str.IndexOf(@"/") >= 0)
                {
                    str = str.Replace(@"/", @"\");
                }
                string dstStr = str;
                int i = str.IndexOf("\\");
                if (i > 0)
                {
                    if (str.IndexOf("resources") < 0)
                    {
                        str = @"resources\media\shaders\" + str;
                    }
                    str = RelativeToAbsolute(str, m_dir);
                }
                else
                {
                    //同级目录下的处理，此处str为XX.tga
                    str = m_dir + "\\" + str;
                }
                if (str.IndexOf(".xml") < 0)
                {
                    str += ".xml";
                }
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            foreach (Match match in Regex.Matches(buffer, @"<Entity type=\""(\S*)\"" url=\""(\S*)\"""))
            {
                string str = match.Value;

                str = str.Substring(str.IndexOf("url=\"") + "url=\"".Length);
                str = str.Substring(0, str.Length - 1);
                if (str.Equals("nil"))
                {
                    continue;
                }
                if (str.IndexOf(@"/") >= 0)
                {
                    str = str.Replace(@"/", @"\");
                }
                int i = str.IndexOf("\\");
                if (i > 0)
                {
                    if (str.IndexOf("resources") < 0)
                    {
                        str = @"resources\media\shaders\" + str;
                    }
                    str = RelativeToAbsolute(str, m_dir);
                }
                else
                {
                    //同级目录下的处理，此处str为XX.tga
                    str = m_dir + "\\" + str;
                }
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            return list;
        }

        public List<string> GetBp3SubFiles()
        {
            List<string> list = new List<string>();
            //以前的资源如100000000.BP3在resources\media\shaders\role\Output；对应lod在resources\media\shaders\role\bp3Lod下：如103007004_lod.lod
            int index = m_fullName.LastIndexOf("output");
            if (m_fullName.IndexOf(@"/")>=0)
            {
                m_fullName = m_fullName.Replace(@"/", "\\");
            }
            int index1 = m_fullName.LastIndexOf("\\");
            string name = m_name.Substring(0, m_name.LastIndexOf("."));
            if (index >= 0)
            {
                string path = m_fullName.Substring(0, index);
                path = path + "bp3Lod" + "\\" + name + "_lod.lod";
                //有的依赖lod文件
                if (File.Exists(path) && !list.Contains(path))
                {
                    list.Add(path);
                }
                //有的依赖布料文件
                string clothPath = m_fullName.Substring(0, index1 + 1) + name + "_cloth.bp3";
                if (File.Exists(clothPath) && !list.Contains(clothPath))
                {
                    list.Add(clothPath);
                }
                return list;
            }
            index = m_fullName.LastIndexOf("Output");
            if (index >= 0)
            {
                string timeStr = m_fullName.Substring(index + "Output".Length, index1 - index - "Output".Length);
                string path = m_fullName.Substring(0, index);
                path = path + "bp3Lod" + timeStr + "\\" + name + "_lod.lod";
                //有的依赖lod文件
                if (File.Exists(path) && !list.Contains(path))
                {
                    list.Add(path);
                }
                //有的依赖布料文件
                string clothPath = m_fullName.Substring(0, index1 + 1) + name + "_cloth.bp3";
                if (File.Exists(clothPath) && !list.Contains(clothPath))
                {
                    list.Add(clothPath);
                }
                return list;
            }
            return list;
        }

        public List<string> GetBp4SubFiles()
        {
            List<string> list = new List<string>();
            //贴图文件和材质文件在resources\media\shaders\role\NewOutput下的文件夹里边，
            //如600000001对应的贴图文件（可能有多个贴图）和材质文件位于resources\media\shaders\role\NewOutput\600000001下,和bp4文件同目录
            //有的在该目录下还存在_cloth.bp4文件
            DirectoryInfo info = new DirectoryInfo(m_dir);
            string name = m_name.Substring(0, m_name.LastIndexOf("."));
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                if (fsi is FileInfo)
                {
                    if (fsi.FullName.IndexOf(name) >= 0 && !fsi.FullName.Equals(m_fullName))
                    {
                        list.Add(fsi.FullName);
                    }
                }
            }

            string path = m_dir.Substring(0, m_dir.IndexOf("NewOutput"));
            if (path.IndexOf("/")>=0)
            {
                path = path.Replace("/", "\\");
            }
            string path1 = path + "bp4Lod\\" + name + "_lod.lod";
            if (File.Exists(path1))
            {
                list.Add(path1);
            }
            return list;
        }

        public List<string> GetRoleSubFiles()
        {
            List<string> list = new List<string>();
            DirectoryInfo dicInfo = new DirectoryInfo(m_dir);
            foreach (FileSystemInfo fsi in dicInfo.GetFileSystemInfos())
            {
                if (fsi is FileInfo && Utility.GetExtensionName(fsi.FullName).Equals("fig"))
                {
                    list.Add(fsi.FullName);
                }
            }
            return list;
        }

        public List<string> GetBp5SubFiles()
        {
            List<string> list = new List<string>();
            //贴图文件和材质文件在resources\media\shaders\role\NewOutput下的文件夹里边，
            //如600000001对应的贴图文件（可能有多个贴图）和材质文件位于resources\media\shaders\role\NewOutput\600000001下,和bp4文件同目录
            //有的在该目录下还存在_cloth.bp4文件
            string name = m_name.Substring(0, m_name.LastIndexOf("."));
            DirectoryInfo info = new DirectoryInfo(m_dir);
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                if (fsi is FileInfo)
                {
                    if (fsi.FullName.IndexOf(name) >= 0 && !fsi.FullName.Equals(m_fullName))
                    {
                        list.Add(fsi.FullName);
                    }
                }
            }

            string path = m_dir.Substring(0, m_dir.IndexOf("NewBpOutput"));
            if (path.IndexOf("/")>=0)
            {
                path = path.Replace("/", "\\");
            }
            path += "bp5Lod\\" + name + "_lod.lod";
            if (File.Exists(path))
            {
                list.Add(path);
            }
            return list;
        }

        public List<string> GetChrSubFiles(string buffer)
        {
            List<string> list = new List<string>();
            foreach (Match match in Regex.Matches(buffer, @"\[SHADER]\r?\n(\S*.*?)"))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf('\n') + 1);
                if (str.IndexOf(".xml") < 0)
                {
                    str += ".xml";
                }
                str = this.Dir + "\\" + str;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            foreach (Match match in Regex.Matches(buffer, @"\[SKELETON]\r?\n(\S*.*?)"))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf('\n') + 1);
                if (str.IndexOf(".ase") < 0 && str.IndexOf(".ASE") < 0)
                {
                    str += ".ase";
                }
                str = this.Dir + "\\" + str;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            foreach (Match match in Regex.Matches(buffer, @"\[SKIN]\r?\n(\S*.*?)"))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf('\n') + 1);
                if (str.IndexOf(".ase") < 0 && str.IndexOf(".ASE") < 0)
                {
                    str += ".ase";
                }
                str = this.Dir + "\\" + str;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            foreach (Match match in Regex.Matches(buffer, @"\[ANIMATION]\r?\n(\S*\s\S*.*?)"))
            {
                string str = match.Value;
                str = str.Substring(str.IndexOf(' ') + 1);
                if (str.IndexOf(".ase") < 0 && str.IndexOf(".ASE") < 0)
                {
                    str += ".ase";
                }
                str = this.Dir + "\\" + str;
                if (!list.Contains(str))
                {
                    list.Add(str);
                }
            }
            return list;
        }

        public Dictionary<string, ResFileInfo> GetAllSubFiles()
        {
            Dictionary<string, ResFileInfo> dicFiles = new Dictionary<string, ResFileInfo>();
            if (!IsAtom)
            {
                List<ResFileInfo> listSubFiles = SubFiles;
                foreach (ResFileInfo info in listSubFiles)
                {
                    if (!dicFiles.ContainsKey(info.FullName))
                    {
                        dicFiles.Add(info.FullName, this);
                    }
                    if (!info.IsAtom && File.Exists(info.FullName))
                    {
                        Dictionary<string, ResFileInfo> dicSubFiles = info.GetAllSubFiles();
                        foreach (KeyValuePair<string, ResFileInfo> subFile in dicSubFiles)
                        {
                            if (!dicFiles.ContainsKey(subFile.Key))
                            {
                                dicFiles.Add(subFile.Key, subFile.Value);
                            }
                        }
                    }
                }
            }
            return dicFiles;
        }
        public Dictionary<string, ResFileInfo> GetNotExistSubFiles()
        {
            Dictionary<string, ResFileInfo> dicFiles = new Dictionary<string, ResFileInfo>();
            if (!IsAtom)
            {
                List<ResFileInfo> listFile = SubFiles;
                foreach (ResFileInfo info in listFile)
                {
                    if (!File.Exists(info.FullName) && !dicFiles.ContainsKey(info.FullName))
                    {
                        dicFiles.Add(info.FullName, this);
                        continue;
                    }

                    if (!info.IsAtom)
                    {
                        foreach (KeyValuePair<string, ResFileInfo> kv in info.GetNotExistSubFiles())
                        {
                            if (!dicFiles.ContainsKey(kv.Key))
                            {
                                dicFiles.Add(kv.Key, kv.Value);
                            }
                        }
                    }
                }
            }
            return dicFiles;

        }
        public void ModifyFilePath(string dstPath)
        {
            FileStream stream = null;
            StreamReader reader = null;
            StreamWriter writer = null;
            try
            {
                FileInfo fileInfo = new FileInfo(m_fullName);
                FileAttributes fileAttr = fileInfo.Attributes;
                fileInfo.Attributes = fileAttr & (~FileAttributes.ReadOnly);
                stream = fileInfo.Open(FileMode.Open, FileAccess.Read);
                reader = new StreamReader(stream, System.Text.Encoding.Default);
                string buffer = reader.ReadToEnd();

                if (DataType == Utility.DataType.DT_XML)
                {
                    buffer = ModifyXmlFilePath(buffer, dstPath);
                }
                else if (DataType == Utility.DataType.DT_EFF)
                {
                    buffer = ModifyEffFilePath(buffer, dstPath);
                }
                else if (DataType == Utility.DataType.DT_SPE)
                {
                    buffer = ModifySpeFilePath(buffer, dstPath);
                }
                if (stream != null)
                {
                    stream.Close();
                }
                stream = fileInfo.Open(FileMode.Truncate, FileAccess.Write);
                //stream.Seek(0, SeekOrigin.Begin);
                writer = new StreamWriter(stream, System.Text.Encoding.Default);
                writer.Write(buffer);
                writer.Flush();
                reader.Close();
                writer.Close();
            }
            catch (Exception e)
            {
                Outputs.WriteLine(OutputMessageType.Error, e.Message);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }
        public string ModifyXmlFilePath(string buffer, string dstPath)
        {
            if (buffer.Contains(@"<SHADERLIB N"))
            {
                return ModifyMaterialFilePath(buffer, dstPath);
            }
            //else if (buffer.Contains(@"<Scene"))
            //{
            //    return ModifySceneFilePath(buffer);
            //}

            return "";
        }
        //public string ModifySceneFilePath(string buffer)
        //{
        //    foreach (Match match in Regex.Matches(buffer, @"\<BaseDir PATH=\""(\S*.*?)"))
        //    {
        //        string str = match.Value;

        //        str = str.Substring(str.IndexOf("\"")+1);
        //        buffer = buffer.Replace(str, @".\""");
        //    }
        //    return buffer;
        //}

        public string ModifySpeFilePath(string buffer, string dstResPath)
        {
            foreach (Match match in Regex.Matches(buffer, @"<Shader name=\""(\S*)\"""))
            {
                string str = match.Value;

                str = str.Substring(str.IndexOf("\"") + 1);
                str = str.Substring(0, str.Length - 1);
                string dstStr = str;
                if (str.IndexOf(@"/")>=0)
                {
                    dstStr = str.Replace(@"/", @"\");
                }
                int i = dstStr.LastIndexOf("\\");
                if (i>=0)
                {
                    dstStr = dstStr.Substring(i + 1);
                }
                
                if (dstStr.Equals("effect.xml"))
                {
                    dstStr = "\"../../../" + dstResPath + "/effect.xml\"";
                    buffer = buffer.Replace("\"effect.xml\"", dstStr);
                }
                else if (dstStr.Equals("effect"))
                {
                    dstStr = "\"../../../" + dstResPath + "/effect.xml\"";
                    buffer = buffer.Replace("\"effect\"", dstStr);
                }
                else
                {
                    dstStr = dstResPath + @"/" + dstStr;
                    if (dstStr.IndexOf(".xml") < 0)
                    {
                        dstStr += ".xml";
                    }
                    if (str.Length > 0 && buffer.IndexOf(str) >= 0)
                    {
                        buffer = buffer.Replace(str, dstStr);
                    }
                }
            }
            foreach (Match match in Regex.Matches(buffer, @"<Entity type=\""(\S*)\"" url=\""(\S*)\"""))
            {
                string str = match.Value;

                str = str.Substring(str.IndexOf("url=\"") + "url=\"".Length);
                str = str.Substring(0, str.Length - 1);
                if (str.Equals("nil"))
                {
                    continue;
                }
                string dstStr = str;
                if (str.IndexOf(@"/") >= 0)
                {
                    dstStr = str.Replace(@"/", @"\");
                }
                int i = dstStr.LastIndexOf("\\");
                if (i>=0)
                {
                    dstStr = dstStr.Substring(i + 1);
                }
                if (dstStr.Equals("effect.xml"))
                {
                    dstStr = "\"../../../" + dstResPath + "/effect.xml\"";
                    buffer = buffer.Replace("\"effect.xml\"", dstStr);
                }
                else if (dstStr.Equals("effect"))
                {
                    dstStr = "\"../../../" + dstResPath + "/effect.xml\"";
                    buffer = buffer.Replace("\"effect\"", dstStr);
                }
                else
                {
                    dstStr = dstResPath + @"/" + dstStr;
                    if (str.Length > 0 && buffer.IndexOf("/") >= 0)
                    {
                        buffer = buffer.Replace(str, dstStr);
                    }
                }
            }
            return buffer;
        }

        public string ModifyMaterialFilePath(string buffer, string dstPath)
        {
            foreach (Match match in Regex.Matches(buffer, @"\<TEXTURE DIRECTORY=\""(\S*.*?)"))
            {
                string str = match.Value;

                str = str.Substring(str.IndexOf("\"") + 1);
                str = str.Substring(0, str.Length - 1);
                string dstStr = str;
                if (str.IndexOf(@"/") >= 0)
                {
                    dstStr = dstStr.Replace(@"/", @"\");
                }

                int i = dstStr.LastIndexOf(@"\");
                if (i > 0)
                {
                    dstStr = dstStr.Substring(i + 1);
                }
                if (dstStr.Equals("effect.xml"))
                {
                    dstStr = "\"../../../" + dstPath + "/effect.xml\"";
                    buffer = buffer.Replace(@"\""effect.xml\", dstStr);
                }
                else if (dstStr.Equals("effect"))
                {
                    dstStr = "\"../../../" + dstPath + "/effect.xml\"";
                    buffer = buffer.Replace("\"effect\"", dstStr);
                }
                else
                {
                    dstStr = dstPath + @"/" + dstStr;
                    if (str.Length > 0 && buffer.IndexOf(str) >= 0)
                    {
                        buffer = buffer.Replace(str, dstStr);
                    }
                }
                
            }

            foreach (Match match in Regex.Matches(buffer, @"\<SHADERLIB Name=\""(\S*.*?)"))
            {
                string str = match.Value;

                str = str.Substring(str.IndexOf("\"") + 1);
                str = str.Substring(0, str.Length - 1);
                string dstStr = str;
                if (str.IndexOf(@"/") >= 0)
                {
                    dstStr = dstStr.Replace(@"/", @"\");
                }

                int i = dstStr.LastIndexOf(@"\");
                if (i > 0)
                {
                    dstStr = dstStr.Substring(i + 1);
                }

                dstStr = dstPath + @"/" + dstStr;
                if (str.Length > 0 && buffer.IndexOf(str) >= 0)
                {
                    buffer = buffer.Replace(str, dstStr);
                }
                
            }
            return buffer;
        }

        public List<ResFileInfo> SubFiles
        {
            get
            {
                if (m_subFiles == null)
                {
                    return GetSubFiles();
                }
                else
                {
                    return m_subFiles;
                }
            }
        }
        private List<ResFileInfo> GetSubFiles()
        {
            List<ResFileInfo> list = new List<ResFileInfo>();
            if (IsAtom)
            {
                list.Add(this);
                return list;
            }

            if (m_fullName.Length <= 0 || !File.Exists(m_fullName))
            {
                return list;
            }
            FileInfo fileInfo = new FileInfo(m_fullName);
            FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream, System.Text.Encoding.Default);
            string buffer = reader.ReadToEnd();
            List<string> subList = null;
            if (m_dataType == Utility.DataType.DT_DML)
            {
                subList = GetDmlSubFiles(buffer);
            }
            else if (m_dataType == Utility.DataType.DT_EFF)
            {
                subList = GetEffSubFiles(buffer);
            }
            else if (m_dataType == Utility.DataType.DT_CHR)
            {
                subList = GetChrSubFiles(buffer);
            }
            else if (m_dataType == Utility.DataType.DT_SPE)
            {
                subList = GetSpeSubFiles(buffer);
            }
            else if (m_dataType == Utility.DataType.DT_XML)
            {
                if (m_name.Equals("role.xml"))
                {
                    subList = GetRoleSubFiles();
                }
                else
                {
                    subList = GetXmlSubFiles(buffer);
                }
            }
            else if (m_dataType == Utility.DataType.DT_BP3)
            {
                subList = GetBp3SubFiles();
            }
            else if (m_dataType == Utility.DataType.DT_BP4)
            {
                subList = GetBp4SubFiles();
            }
            else if (m_dataType == Utility.DataType.DT_BP5)
            {
                subList = GetBp5SubFiles();
            }
            foreach (string name in subList)
            {
                ResFileInfo info = new ResFileInfo();
                info.FullName = name;
                if (!list.Contains(info))
                {
                    list.Add(info);
                }
            }
            reader.Close();
            stream.Close();

            m_subFiles = list;
            return list;
        }
    }
}
