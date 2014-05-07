using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
//using System.Data.SQLite;
using System.Xml;
using System.Xml.XPath;
using System.Collections;

namespace WindowsFormsApplication2
{
    public partial class frmMatome : Form
    {
        string svgPath; // SVGファイルのパス

        //static string basePath = @"\etc"; static string filePath = @"\01science_ base_out.svg";
        static string basePath = @"\rika\y-s4ri13"; static string filePath = @"\y-s4ri13.svg";
        //static string basePath = @"\kokugo"; static string filePath = @"\y-s2ko10.svg";

        uint fusenState = 0; // 付箋表示状況（付箋の表示・非表示を各ビットで管理）※最大32枚まで
        string[] baseStr = {"", "", "", ""};
        // TODO:3をN個対応にする
        string[,] fusenStr = { { "", "", "", "" }, { "", "", "", "" }, { "", "", "", "" } };
        int[] fx = new int[3];
        int[] fy = new int[3];
        int[] tx = new int[3];
        int[] ty = new int[3];
        float xRatio = 0; // 1920*1080のX座標を基準とした比率
        int yAdd = 0; // 画面上部の空白分の座標加算値
        Bitmap mapBase = null;

        ArrayList mapFusen = new ArrayList();
        ArrayList fusenFile = new ArrayList(); //付箋のファイル名を入れている
        //Bitmap[] mapFusen = { null, null, null };
        Bitmap mapNow = null;
        Rectangle drawRectangle; // 倍率変更後の画像のサイズと位置
        int zoomLevel = 0; // 倍率指定（0～3）※表示倍率のインデックス
        //double[] ViewRatio = { 1, 1.3, 1.7, 2.2 }; // 表示倍率
        double[] ViewRatio = { 1, 1.1, 1.2, 1 }; // 表示倍率
        double zoomRatio = 1;


        ArrayList fusen_left = new ArrayList();
        ArrayList fusen_top = new ArrayList();
        ArrayList fusen_right = new ArrayList();
        ArrayList fusen_bottom = new ArrayList();

        string[,] kakudaiStr = { { "", "", "", "" }, { "", "", "", "" }, { "", "", "", "" } };
        ArrayList mapKakudai = new ArrayList();
        static int image_header = 125;  //ヘッダー
        static int image_width_size = 150;
        static int image_height_size = 150;
        ArrayList picture_left = new ArrayList();
        ArrayList picture_top = new ArrayList();
        ArrayList picture_right = new ArrayList();
        ArrayList picture_bottom = new ArrayList();
        ArrayList kakudaiArr = new ArrayList();
        int kakudaiNum = -1;

        double aaa;
        ArrayList test_left = new ArrayList();
        ArrayList test_top = new ArrayList();
        ArrayList test_right = new ArrayList();
        ArrayList test_bottom = new ArrayList();

        public frmMatome()
        {
            InitializeComponent();
        }

        /// <summary>
        /// フォームロード時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            // 境界線設定（タイトルバーを消す）⇒プロパティウィンドウで設定しておかないと、サイズが微妙にズレる
            //this.FormBorderStyle = FormBorderStyle.None;
            
            // Window最大化⇒プロパティウィンドウで設定しておかないと、ちらつく
            //this.WindowState = FormWindowState.Maximized;

            // 設定情報取得
            svgPath = ConfigurationManager.AppSettings["SVG_Path"].ToString();

            // 画像座標を取得
            Get_PicturePos();

            // 表示用SVGの作成（ベース画像、付箋）
            SVG_Create();

            // 付箋座標を取得
            Get_FusenPos();

            // 付箋貼付状態（初期：すべて貼付）
            fusenState = 0xFFFFFFFF;

            // SVGの表示
            SVG_View();

            // 座標補正用比率の計算（1920*1080 が基準）
            Get_Ratio();

        }

        /// <summary>
        /// 画像座標の取得
        /// </summary>
        private void Get_PicturePos()
        {
            string path = svgPath + basePath + filePath;
            double kakudai_width = 0;
            double kakudai_height = 0;
            double width = 0;
            double height = 0;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            string xp = @"/*[local-name()='svg' and namespace-uri()='http://www.w3.org/2000/svg']/*[local-name()='image' and namespace-uri()='http://www.w3.org/2000/svg']";
            //string xp = @"/*[local-name()='svg' and namespace-uri()='http://www.w3.org/2000/svg']/*[local-name()='g' and namespace-uri()='http://www.w3.org/2000/svg']/*[local-name()='image' and namespace-uri()='http://www.w3.org/2000/svg']";
            XmlNodeList nodeList = xmlDocument.SelectNodes(xp);
            for (int i = 0; i < nodeList.Count; i++)
            {
                string href = nodeList[i].Attributes["xlink:href"].Value;
                string image_width = nodeList[i].Attributes["width"].Value;
                string image_height = nodeList[i].Attributes["height"].Value;
                string image_transform = nodeList[i].Attributes["transform"].Value;

                //width,heightが指定の大きさ以上でなければ判定しない
                if (int.Parse(image_width) >= image_width_size && int.Parse(image_height) >= image_height_size)
                {
                    // 判定を各ArrayListに追加
                    string[] arr = image_transform.Replace(")", "").Split(' ');
                    picture_left.Add(double.Parse(arr[4]));                              //left
                    picture_top.Add(double.Parse(arr[5]));                              //top
                    picture_right.Add(double.Parse(image_width) + double.Parse(arr[4]) - 1);     //right
                    picture_bottom.Add(double.Parse(image_height) + double.Parse(arr[5]) - 1);    //bottom

                    double kakudai_size = (1080 - image_header) - double.Parse(image_height);
                    kakudai_width = double.Parse(image_width) + kakudai_size;
                    kakudai_height = double.Parse(image_height) + kakudai_size;

                    double center_x = (double.Parse(arr[4]) + (double.Parse(image_width) / 2));
                    //double center_x = (double.Parse(arr[4]) + (double.Parse(image_width)/2));
                    //double center_x = (double.Parse(arr[4]) + (double.Parse(arr[4]) + double.Parse(image_width))) / 2;
                    double center_y = (double.Parse(arr[5]) + double.Parse(image_height)) / 2;
                    if (i == 15)
                    {
                        aaa = center_x;
                    }

                    double position_x = center_x - (kakudai_width / 2) + image_header/2;
                    double position_y = image_header;
                    if (center_x - (kakudai_width / 2) < 0)
                    {
                        position_x = 0;
                    }
                    else if (center_x - (kakudai_width / 2) > 1920)
                    {
                        position_x = 1920 - kakudai_width;
                    }

                    string mat = "matrix(1 0 0 1 " + position_x + " " + position_y + ")";

                    //Xml作成
                    XmlDocument xml = new XmlDocument();

                    // XML宣言を設定する
                    System.Xml.XmlDeclaration xmlDecl = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xml.AppendChild(xmlDecl);

                    XmlElement elem = xml.CreateElement("svg");
                    //elem.SetAttribute("version", "1.2");
                    //elem.SetAttribute("baseProfile", "tiny");
                    //elem.SetAttribute("id", "レイヤー_1");
                    elem.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
                    elem.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
                    //elem.SetAttribute("x", "0px");
                    //elem.SetAttribute("y", "0px");
                    elem.SetAttribute("width", "1920px");
                    elem.SetAttribute("height", "1080px");
                    elem.SetAttribute("viewBox", "0 0 1920 1080");
                    //elem.SetAttribute("xml:space", "preserve");
                    xml.AppendChild(elem);

                    /*
                    XmlElement rect_elem = xml.CreateElement("rect");
                    rect_elem.SetAttribute("fill", "#FFFFFF");
                    rect_elem.SetAttribute("width", "1920");
                    rect_elem.SetAttribute("height", "1080");
                    elem.AppendChild(rect_elem);
                    */

                    XmlElement image_elem = xml.CreateElement("image");
                    image_elem.SetAttribute("x", position_x.ToString());
                    image_elem.SetAttribute("y", position_y.ToString());
                    image_elem.SetAttribute("width", kakudai_width.ToString());
                    image_elem.SetAttribute("height", kakudai_height.ToString());
                    image_elem.SetAttribute("xlink:href", nodeList[i].Attributes["xlink:href"].Value);
                    //image_elem.SetAttribute("transform", mat);
                    image_elem.SetAttribute("preserveAspectRatio", "xMinYMin meet");
                    elem.AppendChild(image_elem);


                    test_left.Add(position_x);
                    test_top.Add(position_y);
                    test_right.Add(kakudai_width);
                    test_bottom.Add(kakudai_height);
                    xml.Save(i+".svg");

                    //作成したXmlをArrayListに追加(xlinkがうまく指定できないので置換で対応)
                    kakudaiArr.Add(xml.OuterXml.Replace("href","xlink:href"));
                }
            }
        }

        /// <summary>
        /// SVGの作成
        /// </summary>
        private void SVG_Create() {

            // ベース画像
            // SVG(string)は全比率分作成しておく
            using (StreamReader sr = new StreamReader(svgPath + basePath + filePath, Encoding.UTF8))
            {
                string buffStr = sr.ReadToEnd();
                for (int j = 0; j < baseStr.Length; j++)
                {
                    baseStr[j] = buffStr.Replace("1920px", (this.Size.Width * ViewRatio[j]).ToString()); // サイズ変換（X）
                    baseStr[j] = baseStr[j].Replace("1080px", (this.Size.Height * ViewRatio[j]).ToString()); // サイズ変換（Y）
                }
            }
            // ビットマップはメモリを大量に消費するので拡大時に作成する（最初は倍率１のみ作成）
            using (var magickImage = new ImageMagick.MagickImage(
                Encoding.UTF8.GetBytes(baseStr[0]),
                new ImageMagick.MagickReadSettings()
                {
                    Density = new ImageMagick.MagickGeometry(90, 90) // 90でぴったりの大きさ（16:9で全画面の場合）
                }))
            {
                mapBase = magickImage.ToBitmap(); // ビットマップに変換（ベース画像は透過にしない）
            }

            //付箋の数を取得
            string[] files = System.IO.Directory.GetFiles(svgPath + basePath, "*.svg", System.IO.SearchOption.TopDirectoryOnly);
            foreach (string s in files)
            {
                if (s.IndexOf("f-") >= 0)
                {
                    fusenFile.Add(s);
                }
            }

            for (int i = 0; i < fusenFile.Count; i++)
            {
                int node_check = 0; //SVGファイルにx,y,width,height全て存在するかどうかのフラグ
                double x = 0;
                double y = 0;
                double width = 0;
                double height = 0;
                string path = (string)fusenFile[i];
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(path);
                string xp = @"/*[local-name()='svg' and namespace-uri()='http://www.w3.org/2000/svg']/*[local-name()='rect' and namespace-uri()='http://www.w3.org/2000/svg']";
                XmlNodeList nodeList = xmlDocument.SelectNodes(xp);
                for (int j = 0; j < nodeList.Count; j++)
                {
                    if(nodeList[j].Attributes["x"] != null
                        && nodeList[j].Attributes["y"] != null
                        && nodeList[j].Attributes["width"] != null
                        && nodeList[j].Attributes["height"] != null) { 
                        x = double.Parse(nodeList[j].Attributes["x"].Value);
                        y = double.Parse(nodeList[j].Attributes["y"].Value);
                        width = double.Parse(nodeList[j].Attributes["width"].Value) + x;
                        height = double.Parse(nodeList[j].Attributes["height"].Value) + y;
                        node_check = 1;
                    }
                }

                if(node_check == 1)
                {
                    //付箋のあたり判定の取得
                    fusen_left.Add(x);
                    fusen_top.Add(y);
                    fusen_right.Add(width);
                    fusen_bottom.Add(height);
                }
                else
                {
                    //存在しなかった場合そのファイルを読み込まないように
                    fusenFile.RemoveAt(i);
                }
            }
            
            // 付箋
            for (int i = 0; i < fusenFile.Count; i++)
            {
                // SVG(string)は全比率分作成しておく
                string svgName = (string)fusenFile[i];
                //string svgName = String.Format(@"\02science_ f-{0:00}_out.svg", i + 1);

                string[,] fusenStr_temporary = new string[fusenFile.Count,4];
                using (StreamReader sr = new StreamReader(svgName, Encoding.UTF8))
                {
                    string buffStr = sr.ReadToEnd();
                    for (int j = 0; j < baseStr.Length; j++) {
                        fusenStr_temporary[i, j] = buffStr.Replace("1920px", (this.Size.Width * ViewRatio[j]).ToString()); // サイズ変換（X）
                        fusenStr_temporary[i, j] = fusenStr_temporary[i, j].Replace("1080px", (this.Size.Height * ViewRatio[j]).ToString()); // サイズ変換（Y）
                        fusenStr_temporary[i, j] = fusenStr_temporary[i, j].Replace("#FFFFFF", "#FFFFFE"); // 真っ白だと「？」が透けてしまうので、微妙に違う色にする
                        /*
                        fusenStr[i, j] = buffStr.Replace("1920px", (this.Size.Width * ViewRatio[j]).ToString()); // サイズ変換（X）
                        fusenStr[i, j] = fusenStr[i, j].Replace("1080px", (this.Size.Height * ViewRatio[j]).ToString()); // サイズ変換（Y）
                        fusenStr[i,j] = fusenStr[i,j].Replace("#FFFFFF", "#FFFFFE"); // 真っ白だと「？」が透けてしまうので、微妙に違う色にする
                        */
                    }
                }
                fusenStr = (string[,])fusenStr_temporary.Clone();
                //fusenStr_temporary.CopyTo(fusenStr, 0);
                //Array.Copy(fusenStr_temporary, fusenStr, fusenStr_temporary.Length); //配列のコピー

                // ビットマップはメモリを大量に消費するので拡大時に作成する（最初は倍率１のみ作成）
                using (var magickImage = new ImageMagick.MagickImage(
                    Encoding.UTF8.GetBytes(fusenStr[i,0]),
                    new ImageMagick.MagickReadSettings()
                    {
                        Density = new ImageMagick.MagickGeometry(90, 90) // 90でぴったりの大きさ（16:9で全画面の場合）
                    }))
                {
                    Bitmap map = null;
                    map = magickImage.ToBitmap(); // ビットマップに変換
                    map.MakeTransparent(); // 透過（白が透明になる）
                    mapFusen.Add(map);
                    //mapFusen[i] = magickImage.ToBitmap(); // ビットマップに変換
                    //mapFusen[i].MakeTransparent(); // 透過（白が透明になる）
                }
            }

            // 画像
            for (int i = 0; i < kakudaiArr.Count; i++)
            {
                string buffStr = (string)kakudaiArr[i];
                string[,] kakudaiStr_temporary = new string[kakudaiArr.Count, 4];
                for (int j = 0; j < baseStr.Length; j++)
                {
                    kakudaiStr_temporary[i,j] = buffStr.Replace("1920px", (this.Size.Width * ViewRatio[j]).ToString()); // サイズ変換（X）
                    kakudaiStr_temporary[i, j] = kakudaiStr_temporary[i, j].Replace("1080px", (this.Size.Height * ViewRatio[j]).ToString()); // サイズ変換（Y）
                    kakudaiStr_temporary[i, j] = kakudaiStr_temporary[i, j].Replace("#FFFFFF", "#FFFFFE"); // 真っ白だと「？」が透けてしまうので、微妙に違う色にする
                }
                kakudaiStr = (string[,])kakudaiStr_temporary.Clone();
                // ビットマップはメモリを大量に消費するので拡大時に作成する（最初は倍率１のみ作成）
                using (var magickImage = new ImageMagick.MagickImage(
                    Encoding.UTF8.GetBytes(kakudaiStr[i,0]),
                    new ImageMagick.MagickReadSettings()
                    {
                        Density = new ImageMagick.MagickGeometry(90, 90) // 90でぴったりの大きさ（16:9で全画面の場合）
                    }))
                {
                    Bitmap map = null;
                    map = magickImage.ToBitmap(); // ビットマップに変換
                    map.MakeTransparent(); // 透過（白が透明になる）
                    mapKakudai.Add(map);
                }
            }
        }

        #region 拡大処理（未使用）
        //private void SVG_ReCreate(int eX, int eY, Rectangle rect)
        //{
        //    // 表示領域の座標を算出
        //    int xPos = (int)((eX / (double)this.Size.Width) * (this.Size.Width * ViewRatio[zoomLevel]) - this.Size.Width / 2);
        //    int yPos = (int)((eY / (double)this.Size.Height) * (this.Size.Height * ViewRatio[zoomLevel]) - this.Size.Height / 2);
            
        //    // ベース画像
        //    using (var magickImage = new ImageMagick.MagickImage(
        //        Encoding.UTF8.GetBytes(baseStr[zoomLevel]),
        //        new ImageMagick.MagickReadSettings()
        //        {
        //            Density = new ImageMagick.MagickGeometry(90, 90) // 90でぴったりの大きさ（16:9で全画面の場合）
        //        }))
        //    {
        //        //magickImage.
        //        mapBase = magickImage.ToBitmap(); // ビットマップに変換（ベース画像は透過にしない）

        //        if (zoomLevel > 0)
        //        {
        //            Bitmap canvas = new Bitmap(this.Size.Width, this.Size.Height);
        //            Graphics g = Graphics.FromImage(canvas);

        //            //切り取る部分の範囲を決定する。ここでは、位置(10,10)、大きさ100x100
        //            Rectangle srcRect = new Rectangle(xPos, yPos, 1920, 1080);
        //            //描画する部分の範囲を決定する。ここでは、位置(10,10)、大きさ100x100で描画する
        //            Rectangle desRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
        //            //画像の一部を描画する
        //            g.DrawImage(mapBase, desRect, srcRect, GraphicsUnit.Pixel);

        //            g.Dispose();
        //            mapBase = canvas;
        //            mapNow = canvas;
        //        }
        //    }

        //    // 付箋
        //    for (int i = 0; i < 3; i++)
        //    {
        //        using (var magickImage = new ImageMagick.MagickImage(
        //            Encoding.UTF8.GetBytes(fusenStr[i, zoomLevel]),
        //            new ImageMagick.MagickReadSettings()
        //            {
        //                Density = new ImageMagick.MagickGeometry(90, 90) // 90でぴったりの大きさ（16:9で全画面の場合）
        //            }))
        //        {
        //            mapFusen[i] = magickImage.ToBitmap(); // ビットマップに変換
        //            mapFusen[i].MakeTransparent(); // 透過（白が透明になる）
        //        }
        //    }

        //    // 表示
        //    SVG_View();
        //}
        #endregion

        /// <summary>
        /// SVG画像表示
        /// </summary>
        private void SVG_View() {
            // Bitmap合成
            Bitmap resultPic = new Bitmap(mapBase);
            Graphics grap = Graphics.FromImage(resultPic);
            for (int i = 0; i < fusenFile.Count; i++)
            {
                if ((fusenState & ((int)Math.Pow(2, i))) == ((int)Math.Pow(2, i))) // 貼付状態
                {
                    Bitmap map = (Bitmap)mapFusen[i];
                    grap.DrawImage(map, 0, 0, map.Width, map.Height);
                    //grap.DrawImage(mapFusen[i], 0, 0, mapFusen[i].Width, mapFusen[i].Height);
                }
            }

            for (int i = 0; i < mapKakudai.Count; i++)
            {
                if (kakudaiNum == i)
                {
                    //拡大画像の表示
                    Bitmap map = (Bitmap)mapKakudai[i];
                    grap.DrawImage(map, 0, 0, map.Width, map.Height);
                }
            }

            //ボックス表示
            Pen p = new Pen(Color.Red, 1);
            for (int i = 0; i < kakudaiArr.Count; i++)
            {
                grap.DrawRectangle(p, (int)((double)test_left[i] * xRatio * ViewRatio[zoomLevel])
                    , (int)((double)test_top[i] * xRatio * ViewRatio[zoomLevel] + yAdd)
                    , (int)((double)test_right[i] * xRatio * ViewRatio[zoomLevel])
                    , (int)((double)test_bottom[i] * xRatio * ViewRatio[zoomLevel] + yAdd));
            }
            grap.DrawRectangle(p, (int)aaa,100,1,100);
            //grap.DrawRectangle(p, (float)(0 * xRatio * ViewRatio[zoomLevel]), (float)(125 * xRatio * ViewRatio[zoomLevel]+yAdd), 1920, 1080);
            //リソースを解放する
            p.Dispose();
            
            grap.Dispose();
            mapNow = resultPic;
            picSVG.Image = mapNow;
        }

        /// <summary>
        /// SVG画像再表示
        /// </summary>
        private void SVG_ReView(int fusenNum)
        {
            // 付箋の貼り・はがし
            int addNum = 0;
            addNum = (int)Math.Pow(2, fusenNum);
            if ((fusenState & addNum) == addNum) // 該当ビットの状態を確認
            {
                fusenState -= (uint)addNum; // 貼付→はがし
            } else { 
                fusenState += (uint)addNum; // なし→貼付
            }

            // SVG再表示
            SVG_View();
        }

        /// <summary>
        /// SVG画像再表示
        /// </summary>
        private void SVG_Enlarge(int pictureNum)
        {
            kakudaiNum = pictureNum;

            // SVG再表示
            SVG_View();
        }

        /// <summary>
        /// 付箋座標の取得
        /// </summary>
        private void Get_FusenPos() { 
            /*
            for (int i = 0; i < 3; i++) {
                // 座標箇所切り出し
                string str = fusenStr[i,0].Substring(fusenStr[i,0].IndexOf("<rect")); // "<rect" ～
                str = str.Substring(6, 60);
                string[] arr = str.Replace('/', ' ').Split(' ');
                arr[0] = arr[0].Split('=')[1].Replace('"', ' ');
                arr[1] = arr[1].Split('=')[1].Replace('"', ' ');
                arr[3] = arr[3].Split('=')[1].Replace('"', ' ');
                arr[4] = arr[4].Split('=')[1].Replace('"', ' ');
                
                // 数値変換
                fx[i] = int.Parse(arr[0]);
                fy[i] = int.Parse(arr[1]);
                tx[i] = int.Parse(arr[3]);
                ty[i] = int.Parse(arr[4]);
                tx[i] += fx[i] - 1; // X座標に変換（幅を加算）
                ty[i] += fy[i] - 1; // Y座標に変換（高さを加算）
            }
            */
        }

        /// <summary>
        /// 座標補正用比率の算出、Y座標加算値の設定
        /// </summary>
        private void Get_Ratio() {
            // 1920*1080のX座標を基準とした比率（Y座標の比率は使用しない）
            xRatio = (float)this.Size.Width / 1920;
            // TODO:できれば計算で求めたい
            switch (this.Size.Width.ToString() + "*" + this.Size.Height.ToString())
            {
                case "1920*1080":
                case "1600*900":
                case "1366*768":
                case "1360*768":
                    yAdd = 0;
                    break;
                case "1680*1050":
                    yAdd = 53;
                    break;
                case "1440*900":
                    yAdd = 45;
                    break;
                case "1400*1050":
                    yAdd = 131;
                    break;
                case "1280*1024":
                    yAdd = 152;
                    break;
                case "1280*960":
                    yAdd = 121;
                    break;
                case "1280*800":
                    yAdd = 40;
                    break;
                case "1280*768":
                    yAdd = 24;
                    break;
                case "1152*864":
                    yAdd = 108;
                    break;
                case "1024*768":
                    yAdd = 95;
                    break;
            }
        }

        /// <summary>
        /// クリック操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picSVG_MouseDown(object sender, MouseEventArgs e)
        {
            // 「×」ボタン
            if (e.X >= 1829 * xRatio * ViewRatio[zoomLevel] && e.X <= 1899 * xRatio * ViewRatio[zoomLevel]
                && e.Y >= 24 * xRatio * ViewRatio[zoomLevel] + yAdd && e.Y <= 91 * xRatio * ViewRatio[zoomLevel] + yAdd)
            {
                //MessageBox.Show("「×」ボタン押下");
                this.Close();
            }

            //拡大画像が表示されている状態で×＜＞ボタンが押されたら
            if (kakudaiNum != -1)
            {
                //拡大画像を非表示に
                SVG_Enlarge(-1);
            }
            else
            {
                // 左右の三角ボタン
                if (e.X >= 20 * xRatio * ViewRatio[zoomLevel] && e.X <= 63 * xRatio * ViewRatio[zoomLevel]
                    && e.Y >= 422 * xRatio * ViewRatio[zoomLevel] + yAdd && e.Y <= 641 * xRatio * ViewRatio[zoomLevel] + yAdd)
                {
                    MessageBox.Show("「＜」ボタン押下");
                }

                if (e.X >= 1856 * xRatio * ViewRatio[zoomLevel] && e.X <= 1899 * xRatio * ViewRatio[zoomLevel]
                    && e.Y >= 422 * xRatio * ViewRatio[zoomLevel] + yAdd && e.Y <= 641 * xRatio * ViewRatio[zoomLevel] + yAdd)
                {
                    MessageBox.Show("「＞」ボタン押下");
                }

                // 付箋の貼り・はがし
                for (int i = 0; i < fusenFile.Count; i++)
                {
                    // クリックされた座標が付箋の四角形の中か？
                    if (e.X >= (double)fusen_left[i] * xRatio * ViewRatio[zoomLevel] && e.X <= (double)fusen_right[i] * xRatio * ViewRatio[zoomLevel]
                        && e.Y >= (double)fusen_top[i] * xRatio * ViewRatio[zoomLevel] + yAdd && e.Y <= (double)fusen_bottom[i] * xRatio * ViewRatio[zoomLevel] + yAdd)
                    {
                        // SVG再表示
                        //MessageBox.Show(i.ToString());
                        SVG_ReView(i);
                        break;
                    }
                }
                /*
                for (int i = 0; i < 3; i++)
                {
                    // クリックされた座標が付箋の四角形の中か？
                    if (e.X >= fx[i] * xRatio * ViewRatio[zoomLevel] && e.X <= tx[i] * xRatio * ViewRatio[zoomLevel]
                        && e.Y >= fy[i] * xRatio * ViewRatio[zoomLevel] + yAdd && e.Y <= ty[i] * xRatio * ViewRatio[zoomLevel] + yAdd)
                    {
                        // SVG再表示
                        SVG_ReView(i);
                        break;
                    }
                }
                */
                // 画像
                for (int i = 0; i < kakudaiArr.Count; i++)
                {
                    // クリックされた座標が付箋の四角形の中か？
                    if (e.X >= (double)picture_left[i] * xRatio * ViewRatio[zoomLevel] && e.X <= (double)picture_right[i] * xRatio * ViewRatio[zoomLevel]
                        && e.Y >= (double)picture_top[i] * xRatio * ViewRatio[zoomLevel] + yAdd && e.Y <= (double)picture_bottom[i] * xRatio * ViewRatio[zoomLevel] + yAdd)
                    {
                        // SVG拡大表示
                        SVG_Enlarge(i);
                        break;
                    }
                }
            }
        }

        #region 拡大処理（未使用）
        ///// <summary>
        ///// ダブルクリック（拡大）
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void picSVG_DoubleClick(object sender, EventArgs e)
        //{
        //    int eX = ((MouseEventArgs)e).X;
        //    int eY = ((MouseEventArgs)e).Y;

        //    PictureBox pb = (PictureBox)sender;
        //    //クリックされた位置を画像上の位置に変換
        //    Point imgPoint = new Point(
        //        (int)Math.Round((eX - drawRectangle.X) / zoomRatio),
        //        (int)Math.Round((eY - drawRectangle.Y) / zoomRatio));

        //    //倍率を変更する
        //    zoomLevel = (zoomLevel + 1) % 4;
        //    zoomRatio = (double)(ViewRatio[zoomLevel]);

        //    //if (e.Button == MouseButtons.Left)
        //    //{
        //    //    zoomRatio *= 2d;
        //    //}
        //    //else if (e.Button == MouseButtons.Right)
        //    //{
        //    //    zoomRatio *= 0.5d;
        //    //}

        //    //倍率変更後の画像のサイズと位置を計算する
        //    drawRectangle.Width = (int)Math.Round(mapNow.Width * zoomRatio);
        //    drawRectangle.Height = (int)Math.Round(mapNow.Height * zoomRatio);
        //    drawRectangle.X = (int)Math.Round(pb.Width / 2d - imgPoint.X * zoomRatio);
        //    drawRectangle.Y = (int)Math.Round(pb.Height / 2d - imgPoint.Y * zoomRatio);

        //    //画像を表示する
        //    //picSVG.Refresh();

        //    //-----------------------------------------------------------------------------

        //    //drawRectangle = new Rectangle(0, 0, mapNow.Width, mapNow.Height);

        //    //int eX = ((MouseEventArgs)e).X;
        //    //int eY = ((MouseEventArgs)e).Y;

        //    //PictureBox pb = (PictureBox)sender;
        //    //// クリックされた位置を画像上の位置に変換
        //    //Point imgPoint = new Point(
        //    //    (int)Math.Round((eX - drawRectangle.X) / zoomRatio),
        //    //    (int)Math.Round((eY - drawRectangle.Y) / zoomRatio));

        //    // 倍率を変更する
        //    //zoomLevel = (zoomLevel + 1) % 4;
        //    //zoomRatio = (double)(ViewRatio[zoomLevel]);

        //    //// 倍率変更後の画像のサイズと位置を計算する
        //    //drawRectangle.Width = (int)Math.Round(mapNow.Width * zoomRatio);
        //    //drawRectangle.Height = (int)Math.Round(mapNow.Height * zoomRatio);
        //    //drawRectangle.X = (int)Math.Round(pb.Width / 2d - imgPoint.X * zoomRatio);
        //    //drawRectangle.Y = (int)Math.Round(pb.Height / 2d - imgPoint.Y * zoomRatio);

        //    // ビットマップの再作成（拡大版）
        //    SVG_ReCreate(eX, eY, drawRectangle);
        //}
        #endregion

        private void picSVG_Paint(object sender, PaintEventArgs e)
        {
            //画像を指定された位置、サイズで描画する
            //e.Graphics.DrawImage(mapNow, drawRectangle);
        }
    }
}

