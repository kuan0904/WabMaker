using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Tools
{
    public class VerifyCodeTool
    {
        public string RandomPassword(int length = 4)
        {
            Random random = new Random();
            var result = random.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length) - 1);
            return result.ToString();
        }

        /// <summary>
        /// 產生驗證碼
        /// </summary>
        /// <returns></returns>
        public byte[] Create(string validateStr)
        {    
            //生成Bitmap圖像
            var steam = new MemoryStream();
            Bitmap image = new Bitmap(100, 45);
            Graphics g = Graphics.FromImage(image);

            //生成隨機生成器
            Random random = new Random();

            //清空圖片背景色-淺色 //Color.White
            var backColor = Color.FromArgb(random.Next(200, 256), random.Next(200, 256), random.Next(200, 256));
            g.Clear(backColor);

            int i, r, x0, x1, y0, y1;
          
            //畫噪音點
            for (i = 0; i < 200; i++)
            {
                x0 = random.Next(image.Width);
                y0 = random.Next(image.Height);

                backColor = Color.FromArgb(random.Next(200, 256), random.Next(200, 256), random.Next(200, 256));
                image.SetPixel(x0, y0, backColor);
            }
            
            //畫橢圓         
            var pen = new Pen(Color.Yellow);
            for (i = 1; i < 6; i++)
            {
                pen.Color = Color.FromArgb((random.Next(0, 255)), (random.Next(0, 255)), (random.Next(0, 255)));

                r = random.Next(image.Width) / random.Next(1, 2);
                x0 = random.Next(image.Width) - random.Next(r);
                y0 = random.Next(image.Height) - random.Next(r);

                g.DrawEllipse(pen, x0, y0, r, r);
            }

            //畫文字
            r = random.Next(1, 6);
            int r2 = random.Next(0, 4);
            int fR = (r == 1 || r == 4 || r == 6) ? random.Next(0, 50) : r2 == 1 ? 180 : 100;
            int fG = (r == 2 || r == 4 || r == 5) ? random.Next(0, 50) : r2 == 2 ? 180 : 100;
            int fB = (r == 3 || r == 5 || r == 6) ? random.Next(0, 50) : r2 == 3 ? 180 : 100;
            var fontColor = Color.FromArgb(fR, fG, fB);           

            int x = random.Next(5, 10), y = 0;
            for (i = 0; i < validateStr.Length; i++)
            {
                //字型大小               
                Font font = new System.Drawing.Font("Arial", random.Next(16, 24), (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic));
                var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), fontColor, fontColor, 1.2f, true);
                //旋轉與距離
                g.RotateTransform(random.Next(-5, 5));
                y = random.Next(10, 13);
                g.DrawString(validateStr.Substring(i, 1), font, brush, x, y);
                x += random.Next(15, 20);
            }

            //畫噪音點
            for (i = 0; i < 50; i++)
            {
                x0 = random.Next(image.Width);
                y0 = random.Next(image.Height);

                image.SetPixel(x0, y0, Color.FromArgb(random.Next()));
            }

            //畫噪音線
            for (i = 0; i < 20; i++)
            {
                x0 = random.Next(image.Width);
                x1 = random.Next(image.Width);
                y0 = random.Next(image.Height);
                y1 = random.Next(image.Height);

                //Color.Silver
                g.DrawLine(new Pen(Color.FromArgb(random.Next())), x0, y0, x1, y1);
            }

            //畫圖片的邊框線
            g.RotateTransform(0);
            g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

            image.Save(steam, System.Drawing.Imaging.ImageFormat.Jpeg);

            g.Dispose();
            image.Dispose();

            return steam.GetBuffer();

        }
    }
}
