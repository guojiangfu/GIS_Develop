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
    public partial class Form2 : Form
    {
        public Form2(GISLayer layer)
        {
            InitializeComponent();
            for (int i = 0; i < layer.Fields.Count; i++)
            {
                dataGridView1.Columns.Add(layer.Fields[i].name, layer.Fields[i].name);
            }
            for (int i = 0; i < layer.FeatureCount(); i++)
            {
                dataGridView1.Rows.Add();
                for (int j = 0; j < layer.Fields.Count; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = layer.GetFeature(i).getAttribute(j);
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
