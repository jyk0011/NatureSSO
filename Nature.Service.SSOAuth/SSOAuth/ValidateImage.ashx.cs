using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Nature.Service.Ashx;

namespace Nature.Service.SSOAuth
{
    /// <summary>
    /// 生成验证码
    /// </summary>
    /// user:jyk
    /// time:2013/1/30 17:07
    public class ValidateImage : IHttpHandler, IRequiresSessionState
    {
        private const int intLength = 5; //长度
        public static string strIdentify = "Identify"; //随机字串存储键值，以便存储到Session中

        /// <summary>
        /// 生成验证图片核心代码
        /// </summary>
        /// <param name="context"><see cref="T:System.Web.HttpContext"/> 对象，它提供对用于为 HTTP 请求提供服务的内部服务器对象（如 Request、Response、Session 和 Server）的引用。</param>
        /// user:jyk
        /// time:2013/1/28 14:46
        public void ProcessRequest(HttpContext context)
        {
            //base.ProcessRequest(context);
           
            //设置输出流图片格式
            context.Response.ContentType = "image/gif";

            Bitmap b = new Bitmap(200, 60);
            Graphics g = Graphics.FromImage(b);
            g.FillRectangle(new SolidBrush(Color.SlateBlue), 0, 0, 200, 60);
            Font font = new Font(FontFamily.GenericSerif, 48, FontStyle.Bold, GraphicsUnit.Pixel);
            Random r = new Random();
            StringBuilder s = new StringBuilder();

                //合法随机显示字符列表
                const string strLetters = "abcdefhkmnpqrstuvwxyACDGHJKMNPQRSTUVWXY34567";

                //将随机生成的字符串绘制到图片上
                for (int i = 0; i < intLength; i++)
                {
                    s.Append(strLetters.Substring(r.Next(0, strLetters.Length - 1), 1));
                    g.DrawString(s[s.Length - 1].ToString(CultureInfo.InvariantCulture), font, new SolidBrush(Color.White), i * 38, r.Next(0, 15));
                }

            //生成干扰线条
            Pen pen = new Pen(new SolidBrush(Color.Yellow), 2);
            for (int i = 0; i < 3; i++)
            {
                g.DrawLine(pen, new Point(r.Next(0, 199), r.Next(0, 59)), new Point(r.Next(0, 199), r.Next(0, 59)));
            }
            b.Save(context.Response.OutputStream, ImageFormat.Gif);

            context.Session[strIdentify] = s.ToString();
            //StaticOp.thisData(strIdentify, s.ToString()); //先保存在Session中，验证与用户输入是否一致
            context.Response.End();

             
        }

        /// <summary>
        /// 获取一个值，该值指示其他请求是否可以使用 <see cref="T:System.Web.IHttpHandler"/> 实例。
        /// </summary>
        public bool IsReusable
        {
            // 如果无法为其他请求重用托管处理程序，则返回 false。
            // 如果按请求保留某些状态信息，则通常这将为 false。
            get { return true; }
        }
        
    }
}