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

namespace Lesson_8
{
    public partial class Form1 : Form
    {
        GISLayer layer = null;
        GISView view = null;
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
            Graphics graphics = CreateGraphics();
            graphics.FillRectangle(new SolidBrush(Color.Pink), ClientRectangle);
            layer.draw(graphics, view);
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
    }
}
