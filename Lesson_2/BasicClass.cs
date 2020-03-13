using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace MyGIS
{
    class GISVertex
    {
       public double x;
       public  double y;
        public GISVertex(double _x,double _y)
        {
            x = _x;
            y = _y;
        }
        public double Distance(GISVertex anothervertex)
        {
            return Math.Sqrt((x - anothervertex.x)*(x-anothervertex.x) + (y - anothervertex.y)*(y-anothervertex.y));
        }
    }
    class GISPoint:GISSpatial
    {
        public GISPoint(GISVertex onevertex)
        {
            centroid = onevertex;
            extent = new GISExtent(onevertex,onevertex);
        }
        public override void draw (Graphics graphics)
        {
            graphics.FillEllipse(new SolidBrush(Color.Red), new Rectangle((int)centroid.x - 3, (int)centroid.y - 3, 6, 6));
        }
        public double Distance(GISVertex anothervertex)
        {
            return centroid.Distance(anothervertex);
        }
    }
    class GISLine:GISSpatial
    {
        List<GISVertex> AllVertexs;
        public override void draw(Graphics graphics)
        {
            
        }
    }
    class GISPolygon:GISSpatial
    {
        List<GISVertex> AllVertexs;
        public override void draw(Graphics graphics)
        {
            
        }
    }
    class GISFeature
    {
        public GISSpatial spatialpart;
        public GISAttribute attributepart;
        public GISFeature(GISSpatial spatial, GISAttribute attribute)
        {
            spatialpart = spatial;
            attributepart = attribute;
        }
        public void draw(Graphics graphics, bool DrawAttributeOrNot, int index)
        {
            spatialpart.draw(graphics);
            if (DrawAttributeOrNot)
            {
                attributepart.draw(graphics,spatialpart.centroid,index);
            }
        }
        public object getAttribute(int index)
        {
            return attributepart.GetValue(index);
        }
    }
    class GISAttribute
    {
        ArrayList values = new ArrayList();
        public void AddValue(object o)
        {
            values.Add(o);
        }
        public object GetValue(int index)
        {
            return values[index];
        }
        public void draw(Graphics graphics,GISVertex location, int index)
        {
            graphics.DrawString(values[index].ToString(), new Font("宋体", 20), new SolidBrush(Color.Green), new PointF((int)location.x, (int)location.y));
        }
    }
    abstract class GISSpatial
    {
        public GISVertex centroid;
        public GISExtent extent;
        public abstract void draw(Graphics graphics);
    }
    class GISExtent
    {
        public GISVertex bottomleft;
        public GISVertex upright;
        public GISExtent(GISVertex _bottomleft, GISVertex _upright)
        {
            bottomleft = _bottomleft;
            upright = _upright;
        }
    }
}