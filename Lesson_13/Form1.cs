using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyGIS;

namespace Lesson_13
{
    public partial class Form1 : Form
    {
        GISLayer layer = null;
        GISView view = null;
        Bitmap backwindow;
        MOUSECOMMAND MouseCommand = MOUSECOMMAND.Select;
        int MouseStartX = 0;
        int MouseStartY = 0;
        int MouseMovingX = 0;
        int MouseMovingY = 0;
        bool MouseOnMap = false;
        public Form1()
        {
            InitializeComponent();
            view = new GISView(new GISExtent(new GISVertex(0,0), new GISVertex(100,100)),ClientRectangle);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string path1 = @"..\..\..\Data\Shapefile\shape1\jyg.shp";
            //string path2 = @"..\..\..\Data\Shapefile\shape2\country.shp";
            //GISShapefile sf = new GISShapefile();
            //layer = sf.ReadShapefile(path1);
            //layer.DrawAttributeOrNot = false;
            //MessageBox.Show("read " + layer.FeatureCount() + " point objects.");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Shapefile文件|*.shp";
            openFileDialog.RestoreDirectory = false;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            layer = GISShapefile.ReadShapefile(openFileDialog.FileName);
            layer.DrawAttributeOrNot = false;
            MessageBox.Show("read " + layer.FeatureCount() + " point objects.");
            view.UpdateExtent(layer.Extent);
            UpdateMap();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            view.UpdateExtent(layer.Extent);
            UpdateMap();
        }
        private void UpdateMap()
        {
            //如果窗口被最小化了，就不再进行绘制
            if (ClientRectangle.Width * ClientRectangle.Height == 0)
            {
                return;
            }
            //确保当前view的地图窗口尺寸是正确的
            view.UpdateRectangle(ClientRectangle);
            //根据最新的地图窗口尺寸建立背景窗口
            if (backwindow != null)
            {
                backwindow.Dispose();
            }
            backwindow = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            //再背景窗口上绘图
            Graphics g = Graphics.FromImage(backwindow);
            g.FillRectangle(new SolidBrush(Color.Black),ClientRectangle);
            layer.draw(g, view);
            //把背景窗口绘制到前景窗口上
            Graphics graphics = CreateGraphics();
            graphics.DrawImage(backwindow, 0, 0);
        }

        private void Form1_Click(object sender, EventArgs e)
        {

        }
        //统一的事件处理函数
        private void MapButtonClick(object sender, EventArgs e)
        {
            GISMapAction action = GISMapAction.zoomin;
            if ((Button)sender == button3) action = GISMapAction.zoomin;
            else if ((Button)sender == button4) action = GISMapAction.zoomout;
            else if ((Button)sender == button5) action = GISMapAction.moveup;
            else if ((Button)sender == button6) action = GISMapAction.movedown;
            else if ((Button)sender == button7) action = GISMapAction.moveleft;
            else if ((Button)sender == button8) action = GISMapAction.moveright;
            view.ChangeView(action);
            UpdateMap();

        } 

        private void button9_Click(object sender, EventArgs e)
        {
            Form2 myForm = new Form2(layer);
            myForm.Show();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            GISMyFile.WriteFile(layer, @"..\..\..\Data\MyGISFile\mygisfile.gisfile");
            MessageBox.Show("Done");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            layer = GISMyFile.ReadFile(@"..\..\..\Data\MyGISFile\mygisfile.gisfile");
            MessageBox.Show("read" + layer.FeatureCount() + "object");
            view.UpdateExtent(layer.Extent);
            UpdateMap();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (backwindow != null)
            {
                //是鼠标操作引起的窗口重绘
                if (MouseOnMap)
                {
                    //是由于移动地图造成的，就移动背景图片
                    if (MouseCommand == MOUSECOMMAND.Pan)
                    {
                        e.Graphics.DrawImage(backwindow, MouseMovingX - MouseStartX, MouseMovingY - MouseStartY);
                    }
                    //是由于选择或缩放操作造成的，就画一个框
                    else if (MouseCommand != MOUSECOMMAND.Unused)
                    {
                        e.Graphics.DrawImage(backwindow, 0, 0);
                        e.Graphics.FillRectangle(new SolidBrush(GISConst.ZoomSelectBoxColor), new Rectangle(Math.Min(MouseStartX, MouseMovingX), 
                                                                                                                                                                       Math.Min(MouseStartY, MouseMovingY), 
                                                                                                                                                                       Math.Abs(MouseStartX - MouseMovingX), 
                                                                                                                                                                       Math.Abs(MouseStartY - MouseMovingY)));
                    }
                }
                else
                    e.Graphics.DrawImage(backwindow,0,0);
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            UpdateMap();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseStartX = e.X;
            MouseStartY = e.Y;
            MouseOnMap = (e.Button == MouseButtons.Left && MouseCommand != MOUSECOMMAND.Unused);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMovingX = e.Y;
            MouseMovingY = e.X;
            if (MouseOnMap)
            {
                Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (layer == null)
                return;
            if (MouseOnMap == false)
                return;
            MouseOnMap = false;
            switch (MouseCommand)
            {
                case MOUSECOMMAND.Select:
                    if (Control.ModifierKeys != Keys.Control)
                        layer.ClearSelection();
                    SelectResult sr = SelectResult.UnknownType;
                    if(e.X == MouseStartX && e.Y == MouseStartY)
                    {
                        GISVertex v = view.ToMapVertex(new Point(e.X, e.Y));
                        sr = layer.Select(v, view);
                    }
                    else
                    {
                        GISExtent extent = view.RectToExtent(e.X,MouseStartX,e.Y,MouseStartY);
                        sr = layer.Select(extent);
                    }
                    if (sr == SelectResult.OK || Control.ModifierKeys != Keys.Control)
                    {
                        UpdateMap();
                        UpdateAttributeWindow();
                    }
                    break;
                case MOUSECOMMAND.ZoomIn:
                    break;
                case MOUSECOMMAND.ZoomOut:
                    break;
                case MOUSECOMMAND.Pan:
                    break;
            }
        }
    }
}
