using MyTool.Commons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyTool.Tools
{
    public class ImageTool
    {
        /// <summary>
        /// 等比縮圖、裁圖範圍置中
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="thumbPath">The thumb path.</param>
        /// <param name="width">縮圖寬度</param>
        /// <param name="height">縮圖高度</param>
        public static CiResult Resize(string filePath, string thumbPath, int width, int height)
        {
            var result = new CiResult();

            //不縮圖 return
            if (width == 0 && height == 0)
            {
                result.Message = SystemMessage.ImageResizeNull;
                return result;
            }

            //原圖
            Image image = Image.FromFile(HttpContext.Current.Server.MapPath(filePath));

            float ratW, ratH, rat;
            int picWidth = image.Width;   //原圖寬度
            int picHeight = image.Height; //原圖高度
            int saveWidth = 0;            //新圖寬
            int saveHeight = 0;           //新圖高
            int pointX = 0;
            int pointY = 0;

            // 是否縮圖: 原圖長寬皆大於縮圖尺寸 return
            if (!(width <= picWidth && height <= picHeight))
            {
                result.Message = SystemMessage.ImageResizeSmall;
                return result;
            }
            
            //只設一邊比例: 等比縮放
            if (width == 0)
            {
                rat = (float)picHeight / height;
            }
            else if (height == 0)
            {
                rat = (float)picWidth / width;
            }
            //比例不失針: 依較短的邊
            else
            {
                ratW = (float)picWidth / width;
                ratH = (float)picHeight / height;
                rat = (ratH > ratW ? ratW : ratH);
            }

            saveWidth = (int)(picWidth / rat);
            saveHeight = (int)(picHeight / rat);

            //加入原來未設定的長寬
            if (width == 0) { width = saveWidth; }
            else if (height == 0) { height = saveHeight; }


            //是否裁切: 長寬皆有指定
            if (width != 0 && height != 0)
            {
                if (saveWidth > width) { pointX = saveWidth / 2 - width / 2; }
                if (saveHeight > height) { pointY = saveHeight / 2 - height / 2; }
            }

            //製作縮圖          
            var bmPhoto = new Bitmap(width, height);
            var gbmPhoto = Graphics.FromImage(bmPhoto);

            gbmPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            gbmPhoto.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gbmPhoto.Clear(Color.Transparent);


            gbmPhoto.DrawImage(image, new Rectangle(0, 0, width, height),
                                      new Rectangle(pointX * image.Width / saveWidth,
                                                    pointY * image.Height / saveHeight,
                                                    width * image.Width / saveWidth,
                                                    height * image.Height / saveHeight), GraphicsUnit.Pixel);
            bmPhoto.Save(HttpContext.Current.Server.MapPath(thumbPath), System.Drawing.Imaging.ImageFormat.Jpeg);

            //Image thumb = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
            //thumb.Save(HttpContext.Current.Server.MapPath(thumbPath));

            result.IsSuccess = true;
            bmPhoto.Dispose();
            image.Dispose();

            return result;
        }
    }
}
