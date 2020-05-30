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

namespace Lesson_12
{
    public partial class Form1 : Form
    {
        GISLayer layer = null;
        GISView view = null;
        Bitmap backwindow;
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
                e.Graphics.DrawImage(backwindow,0,0);
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            UpdateMap();
        }
    }
}
