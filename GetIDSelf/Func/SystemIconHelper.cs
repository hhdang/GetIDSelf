using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BackgroundManager.Func
{
    public class SystemIconHelper
    {
        [DllImport("shell32.dll")]
        public static extern uint ExtractIconEx( string lpszFile, int nIconIndex, int[] phiconLarge, int[] phiconSmall, uint nIcons );
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public static Dictionary<String, Icon> IconList = new Dictionary<String, Icon>();

        /// <summary>
        /// Image转换为Icon
        /// </summary>
        /// <param name = "orgImg"></param>
        /// <returns></returns>
        public static Icon ImageToIcon( Image orgImg ) {
            var bmp = new Bitmap( orgImg );
            IntPtr h = bmp.GetHicon();
            Icon icon = Icon.FromHandle(h);
            //释放 IntPtr
            DeleteObject(h);
            return icon;
        }
        /// <summary>
        /// Image转换为 字节流
        /// </summary>
        /// <param name = "image"></param>
        /// <param name = "Format"></param>
        /// <returns></returns>
        public static byte[] ImageToByteArray( Image image, ImageFormat Format )
        {
            var ms = new MemoryStream();
            image.Save( ms, ImageFormat.Png );
            return ms.ToArray();
        }
        /// <summary>
        /// 字节流 转换为 Image
        /// </summary>
        /// <param name = "btArray"></param>
        /// <returns></returns>
        public static Image ImageFromByteArray( byte[] btArray ) {
            var ms = new MemoryStream( btArray );
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
        /// <summary>
        /// 获取文件内容的类型
        /// </summary>
        /// <param name = "fileName"></param>
        /// <returns></returns>
        public static string GetContentType( string fileName ) {
            string contentType = "application/octetstream";
            try {
                string ext = Path.GetExtension(fileName).ToLower();
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(ext);
                if( registryKey != null && registryKey.GetValue("Content Type") != null ){
                    contentType = registryKey.GetValue("Content Type").ToString();
                }
            }catch( Exception ){

            }
            return contentType;
        }
        /// <summary>
        /// 根据文件的类型获取 Icon
        /// </summary>
        /// <param name = "sFileExt"></param>
        /// <returns></returns>
        public static Icon GetIconByFileType( String sFileExt )
        {
            string sProg =
                Registry.ClassesRoot.OpenSubKey(Registry.ClassesRoot.OpenSubKey(sFileExt).GetValue(String.Empty).ToString())
                    .OpenSubKey("shell")
                    .OpenSubKey("open")
                    .OpenSubKey("command")
                    .GetValue(String.Empty)
                    .ToString();
            sProg = sProg.Substring(0, 1) == Convert.ToChar(34).ToString()
                ? sProg.Substring(1, sProg.IndexOf(Convert.ToChar(34), 2) - 1)
                : sProg.Substring( 0, sProg.IndexOf( " ", 2 ) );
            sProg = sProg.Replace( "%1", String.Empty );
            Icon oIcon = Icon.ExtractAssociatedIcon(sProg);
            
            return oIcon;
        }
        /// <summary>
        /// 根据文件名获得图片数组下标
        /// </summary>
        /// <param name = "fileName"></param>
        /// <param name = "isLarge"></param>
        /// <returns></returns>
        public static Icon GetIconByFileName( String fileName, bool isLarge ) {
            String GetIcon = new FileInfo( fileName ).Extension;
            if( IconList.ContainsKey(GetIcon) ){
                return IconList[GetIcon];
            }

            Icon mIcon = GetIconByFileType( GetIcon, isLarge );
            if( mIcon != null ){
                IconList.Add( "GetIcon", mIcon );
                return mIcon;
            }

            return null;
        }
        /// <summary>
        ///     给出文件扩展名(.*),返回相应图标
        ///     若不以 "." 开头则返回文件夹的图标。
        /// </summary>
        /// <param name = "fileType"></param>
        /// <param name = "isLarge"></param>
        /// <returns></returns>
        public static Icon GetIconByFileType(string fileType, bool isLarge)
        {
            if( !string.IsNullOrEmpty( fileType ) ){
                string regIconString = null;
                //默认指定为文件夹图标
                string systemDirectory = Environment.SystemDirectory + "\\shell32.dll,3";

                if( fileType[0] == '.' ){
                    //读系统注册表中文件类型信息
                    RegistryKey regVersion = Registry.ClassesRoot.OpenSubKey( fileType, true );
                    if( regVersion != null ){
                        string regFileType = regVersion.GetValue(String.Empty) as String;
                        regVersion.Close();
                        regVersion = Registry.ClassesRoot.OpenSubKey(regFileType + @"\DefaultIcon", true);
                        if (regVersion != null) {
                            regIconString = regVersion.GetValue(String.Empty) as String;
                            regVersion.Close();
                        }
                    }

                    if( regIconString == null ){
                        //没有读取到文件类型注册信息，指定为未知文件类型的图标
                        regIconString = systemDirectory + "shell32.dll,0";
                    }
                }

                String[] fileIcon = regIconString.Split( new[]{ ',' } );
                if( fileIcon.Length != 2 ){
                    //系统注册表中注册的图标不能直接提取，则返回可执行文件的通用图标
                    fileIcon = new[] { systemDirectory + "shell32.dll", "2" };
                }
                Icon resultIcon = null;
                try
                {
                    //调用API方法读取图标
                    var phiconLarge = new int[1];
                    var phiconSmall = new int[1];
                    ExtractIconEx(fileIcon[0], Int32.Parse(fileIcon[1]), phiconLarge, phiconSmall, 1);

                    var IconHnd = new IntPtr( isLarge ? phiconLarge[0] : phiconSmall[0] );
                    resultIcon = Icon.FromHandle( IconHnd );
                }
                catch {
                    try
                    {
                        //第二方案
                        resultIcon = GetIconByFileType( fileType );
                    }
                    catch { 
                        //默认方案
                        regIconString = systemDirectory + "shell32.dll,0";
                        fileIcon = regIconString.Split( new[]{ ',' } );
                        resultIcon = null;
                        //调用API方法读取图标
                        var phiconLarge = new int[1];
                        var phiconSmall = new int[1];
                        ExtractIconEx(fileIcon[0], Int32.Parse(fileIcon[1]), phiconLarge, phiconSmall, 1);

                        var IconHnd = new IntPtr( isLarge ? phiconLarge[0] : phiconSmall[0] );
                        resultIcon = Icon.FromHandle( IconHnd );
                    }
                }
                return resultIcon;
            }
            return null;
        }
    }
}
