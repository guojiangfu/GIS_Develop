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

namespace Lesson_2
{
    public partial class Form1 : Form
    {
        List<GISFeature> features = new List<GISFeature>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {   //获取空间信息
            double x = Convert.ToDouble(textBox1.Text);
            double y = Convert.ToDouble(textBox2.Text);
            GISVertex onevertex = new GISVertex(x, y);
            GISPoint onepoint = new GISPoint(onevertex);
            //获取属性信息
            string attribute = textBox3.Text;
            GISAttribute oneattribute = new GISAttribute();
            oneattribute.AddValue(attribute);
            //新建一个GISFeature，并添加到数组features中；
            GISFeature onefeature = new GISFeature(onepoint, oneattribute);
            features.Add(onefeature);
            //把这个新的GISFeature画出来
            Graphics graphics = this.CreateGraphics();
            onefeature.draw(graphics, true, 0);
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            GISVertex onevertex = new GISVertex((double)e.X, (double)e.Y);
            double mindistance = Double.MaxValue;
            int findid = -1;
            //计算点击位置与features数组中哪一个元素的中心点最近
            for (int i = 0; i < features.Count; i++)
            {
                double distance = features[i].spatialpart.centroid.Distance(onevertex);
                if (distance < mindistance)
                {
                    mindistance = distance;
                    findid = i;
                }
            }
            if (mindistance > 5 || findid == -1)
            {
                MessageBox.Show("没有点实体或者鼠标点击位置不准确！");
            }
            else
                MessageBox.Show(features[findid].getAttribute(0).ToString());
        }
    }
}
