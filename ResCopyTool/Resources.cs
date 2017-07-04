//Copyright ?2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace ResCopyTool
{
    /// <summary>
    /// Filenames for standard game icons. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {      

        /// <summary>
        /// Backface icon name</summary>
        /// 
        [ImageResource("ASE.png", "ASE.png", "ASE.png")]
        public static readonly string ASEImage;

        [ImageResource("LOD.png", "LOD.png", "LOD.png")]
        public static readonly string LODImage;

        [ImageResource("ISC.png", "ISC.png", "ISC.png")]
        public static readonly string ISCImage;

        [ImageResource("AC6.png", "AC6.png", "AC6.png")]
        public static readonly string AC6Image;

        [ImageResource("AT6.png", "AT6.png", "AT6.png")]
        public static readonly string AT6Image;

        [ImageResource("DDS.png", "DDS.png", "DDS.png")]
        public static readonly string DDSImage;

        [ImageResource("TAG.png", "TAG.png", "TAG.png")]
        public static readonly string TAGImage;

        [ImageResource("PNG.png", "PNG.png", "PNG.png")]
        public static readonly string PNGImage;

        [ImageResource("JPG.png", "JPG.png", "JPG.png")]
        public static readonly string JPGImage;

        [ImageResource("BMP.png", "BMP.png", "BMP.png")]
        public static readonly string BMPImage;

        [ImageResource("PSD.png", "PSD.png", "PSD.png")]
        public static readonly string PSDImage;

        [ImageResource("XML.png", "XML.png", "XML.png")]
        public static readonly string XMLImage;

        [ImageResource("BP3.png", "BP3.png", "BP3.png")]
        public static readonly string BP3Image;

        [ImageResource("BP4.png", "BP4.png", "BP4.png")]
        public static readonly string BP4Image;

        [ImageResource("BP5.png", "BP5.png", "BP5.png")]
        public static readonly string BP5Image;

        [ImageResource("CHR1.png", "CHR1.png", "CHR1.png")]
        public static readonly string CHRImage;

        [ImageResource("DML1.png", "DML1.png", "DML1.png")]
        public static readonly string DMLImage;

        [ImageResource("EFF1.png", "EFF1.png", "EFF1.png")]
        public static readonly string EFFImage;

        [ImageResource("SPE1.png", "SPE1.png", "SPE1.png")]
        public static readonly string SPEImage;

        private const string ResourcePath = "ResCopyTool.resources.";

        static Resources()
        {
            ResourceUtil.Register(typeof(Resources), ResourcePath);
        }
    }
}
