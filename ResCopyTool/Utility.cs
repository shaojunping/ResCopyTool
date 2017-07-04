using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;


namespace ResCopyTool
{
    public class Utility
    {
        public enum DataType
        {
            DT_NULL,
            DT_ASE,
            DT_LOD,
            DT_ISC,
            DT_AC6,
            DT_AT6,
            DT_DDS,
            DT_TAG,
            DT_PNG,
            DT_JPG,
            DT_BMP,
            DT_PSD,
            DT_XML,
            DT_DML,
            DT_CHR,
            DT_EFF,
            DT_SPE,
            DT_BP3,
            DT_BP4,
            DT_BP5,
            DT_END
        };
        public static string[] m_imgType/*[DataType::DT_END]*/ = {"", Resources.ASEImage, Resources.LODImage, Resources.ISCImage, Resources.AC6Image, Resources.AT6Image, Resources.DDSImage, Resources.TAGImage, Resources.PNGImage, Resources.JPGImage, Resources.BMPImage, Resources.PSDImage, Resources.XMLImage
    ,Resources.DMLImage, Resources.CHRImage, Resources.EFFImage, Resources.SPEImage, Resources.BP3Image, Resources.BP4Image, Resources.BP5Image};

        public static DataType GetDataType(string srcString)
        {
            string suffix = GetExtensionName(srcString);
            if (m_extensionType[(int)DataType.DT_NULL].Equals(suffix.ToLower()))
            {
                return DataType.DT_NULL;
            }
            else if (m_extensionType[(int)DataType.DT_ASE].Equals(suffix.ToLower()))
            {
                return DataType.DT_ASE;
            }
            else if (m_extensionType[(int)DataType.DT_LOD].Equals(suffix.ToLower()))
            {
                return DataType.DT_LOD;
            }
            else if (m_extensionType[(int)DataType.DT_ISC].Equals(suffix.ToLower()))
            {
                return DataType.DT_ISC;
            }
            else if (m_extensionType[(int)DataType.DT_AC6].Equals(suffix.ToLower()))
            {
                return DataType.DT_AC6;
            }
            else if (m_extensionType[(int)DataType.DT_AT6].Equals(suffix.ToLower()))
            {
                return DataType.DT_AT6;
            }
            else if (m_extensionType[(int)DataType.DT_DDS].Equals(suffix.ToLower()))
            {
                return DataType.DT_DDS;
            }
            else if (m_extensionType[(int)DataType.DT_TAG].Equals(suffix.ToLower()))
            {
                return DataType.DT_TAG;
            }
            else if (m_extensionType[(int)DataType.DT_PNG].Equals(suffix.ToLower()))
            {
                return DataType.DT_PNG;
            }
            else if (m_extensionType[(int)DataType.DT_JPG].Equals(suffix.ToLower()))
            {
                return DataType.DT_JPG;
            }
            else if (m_extensionType[(int)DataType.DT_BMP].Equals(suffix.ToLower()))
            {
                return DataType.DT_BMP;
            }
            else if (m_extensionType[(int)DataType.DT_PSD].Equals(suffix.ToLower()))
            {
                return DataType.DT_PSD;
            }
            else if (m_extensionType[(int)DataType.DT_XML].Equals(suffix.ToLower()))
            {
                return DataType.DT_XML;
            }
            else if (m_extensionType[(int)DataType.DT_DML].Equals(suffix.ToLower()))
            {
                return DataType.DT_DML;
            }
            else if (m_extensionType[(int)DataType.DT_CHR].Equals(suffix.ToLower()))
            {
                return DataType.DT_CHR;
            }
            else if (m_extensionType[(int)DataType.DT_EFF].Equals(suffix.ToLower()))
            {
                return DataType.DT_EFF;
            }
            else if (m_extensionType[(int)DataType.DT_SPE].Equals(suffix.ToLower()))
            {
                return DataType.DT_SPE;
            }
            else if (m_extensionType[(int)DataType.DT_BP3].Equals(suffix.ToLower()))
            {
                return DataType.DT_BP3;
            }
            else if (m_extensionType[(int)DataType.DT_BP4].Equals(suffix.ToLower()))
            {
                return DataType.DT_BP4;
            }
            else if (m_extensionType[(int)DataType.DT_BP5].Equals(suffix.ToLower()))
            {
                return DataType.DT_BP5;
            }
            
            return DataType.DT_NULL;
        }

        private static string[] m_extensionType/*[DataType::DT_END]*/ = {"", "ase", "lod", "isc", "ac6", "at6", "dds", "tga", "png", "jpg", "bmp", "psd", "xml"
    ,"dml", "chr", "eff", "spe", "bp3", "bp4", "bp5"};
        public static bool isAtomFile(DataType type)
        {
            if (type == DataType.DT_NULL || type == DataType.DT_BMP || type == DataType.DT_JPG || type == DataType.DT_PNG
                || type == DataType.DT_TAG || type == DataType.DT_DDS || type == DataType.DT_LOD
                 || type == DataType.DT_ASE || type == DataType.DT_ISC || type == DataType.DT_AC6 || type == DataType.DT_AT6
                 || type == DataType.DT_PSD)
            {
                return true;
            }
            return false;
        }

        public static string GetExtensionName(string srcString)
        {
            string extension = "";
	        int index = srcString.IndexOf(".");
            if (srcString.Length > 0 && index >= 0)
            {
                extension = srcString.Substring(index + 1, srcString.Length - index - 1);
                extension.TrimEnd(' ');
            }
            return extension;
        }

        public static string GetNameByFullName(string fullName)
        {
            if (fullName.LastIndexOf(@"\") < 0)
            {
                return fullName;
            } 
            else
            {
                return fullName.Substring(fullName.LastIndexOf("\\") + 1);
            }
        }

        public static string GetOnlyNameByFullName(string fullName)
        {
            string name = fullName;
            if (fullName.LastIndexOf(@"\") >= 0)
            {
                name = fullName.Substring(fullName.LastIndexOf("\\") + 1);
                if (name.LastIndexOf(".") >= 0)
                {
                    name = name.Substring(0, name.LastIndexOf("."));
                }
            }
            return name;
        }

        public static string GetRelativePath(string srcPath, string dirPath)
        {
            string relativePath = "";
            if (srcPath.Length != 0)
            {
                relativePath = srcPath.Substring(dirPath.Length + 1);
            }
            return relativePath;
        }

        public static string GetDir(string value)
        {
            if (value.LastIndexOf("\\") > 0)
            {
                return value.Substring(0, value.LastIndexOf("\\"));
            }
            return "";
        }

        //根据源路径和目标dir获取目标决定路径
        public static string GetDstPath(string srcPath, string dstDir)
        {
            int index = (srcPath.IndexOf("resources") > 0) ? srcPath.IndexOf("resources") : srcPath.IndexOf("Resources");
            if (index < 0)
            {
                return "";
            }
            string dstPath = "";
            if (dstDir[dstDir.Length - 1].Equals('\\') && index >=0)
            {
                dstPath = dstDir + srcPath.Substring(index);
            }
            else
            {
                dstPath = dstDir + '\\' + srcPath.Substring(index);
            }
            return dstPath;
        }

        //源绝对路径+目标文件夹->最终绝对路径,将原有目录拷贝
        public static bool CopyFileByTiledMode(string srcPath, string dstDir)
        {
            string dstPath = GetDstPath(srcPath, dstDir);
            if (File.Exists(srcPath) && !File.Exists(dstPath))
            {
                string dir = GetDir(dstPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    //MessageBox.Show("创建目录" + dir + "成功");
                }
                File.Copy(srcPath, dstPath);
                return true;
            }
            return false;
        }

        //源文件名+目标文件夹->最终绝对路径,不用将原有目录拷贝,拷贝后，所有文件在一级目录下
        public static bool CopyFileNotByTiledMode(string srcPath, string dstDir)
        {
            string name = GetNameByFullName(srcPath);
            string dstPath = "";
            if (dstDir[dstDir.Length - 1].Equals('\\'))
            {
                dstPath = dstDir + name;
            }
            else
            {
                dstPath = dstDir + '\\' + GetNameByFullName(name);
            }
            if (File.Exists(srcPath) && !File.Exists(dstPath))
            {
                File.Copy(srcPath, dstPath);
                ResFileInfo info = new ResFileInfo();
                info.FullName = dstPath;
                string path = Utility.GetModifyFilePath(dstPath);
                info.ModifyFilePath(path);
                return true;
            }
            return false;
        }

        public static string GetModifyFilePath(string srcPath)
        {
            string path1 = srcPath;
            int index1 = path1.LastIndexOf(@"\");
            string path4 = path1.Substring(0, index1); //@"G:\321\chair"
            int index2 = path4.LastIndexOf(@"\");
            string path3 = path4.Substring(0, index2); //@"G:\321"
            int index3 = path3.LastIndexOf(@"\");
            string path2 = path3.Substring(0, index3); //@"G:\"
            path1 = "../" + path1.Substring(index3 + 1, index1 - index3 - 1);
            path1 = path1.Replace(@"\", @"/"); //../321/chair/chair.ase
            return path1;
        }
    }

}
