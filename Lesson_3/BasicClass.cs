using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace MyGIS
{
    class GISVertex
    {
        public double x;
        public double y;
        public GISVertex(double _x, double _y)
        {
            x = _x;
            y = _y;
        }
        public double Distance(GISVertex anothervertex)
        {
            return Math.Sqrt((x - anothervertex.x) * (x - anothervertex.x) + (y - anothervertex.y) * (y - anothervertex.y));
        }
    }
    class GISPoint : GISSpatial
    {
        public GISPoint(GISVertex onevertex)
        {
            centroid = onevertex;
            extent = new GISExtent(onevertex, onevertex);
        }
        public override void draw(Graphics graphics, GISView view)
        {
            Point screenpoint = view.ToScreenPoint(centroid);
            graphics.FillEllipse(new SolidBrush(Color.Red), new Rectangle(screenpoint.X - 3, screenpoint.Y - 3, 6, 6));
        }
        public double Distance(GISVertex anothervertex)
        {
            return centroid.Distance(anothervertex);
        }
    }
    class GISLine : GISSpatial
    {
        List<GISVertex> AllVertexs;
        public override void draw(Graphics graphics, GISView view)
        {

        }
    }
    class GISPolygon : GISSpatial
    {
        List<GISVertex> AllVertexs;
        public override void draw(Graphics graphics, GISView view)
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
        public void draw(Graphics graphics, GISView view, bool DrawAttributeOrNot, int index)
        {
            spatialpart.draw(graphics, view);
            if (DrawAttributeOrNot)
            {
                attributepart.draw(graphics, view, spatialpart.centroid, index);
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
        public void draw(Graphics graphics, GISView view, GISVertex location, int index)
        {
            Point screenpoint = view.ToScreenPoint(location);
            graphics.DrawString(values[index].ToString(), new Font("宋体", 20), new SolidBrush(Color.Green), new PointF(screenpoint.X, screenpoint.Y));
        }
    }
    abstract class GISSpatial
    {
        public GISVertex centroid;//空间实体的中心点
        public GISExtent extent;//空间范围（最小外接矩形）
        public abstract void draw(Graphics graphics, GISView view);
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
        public GISExtent(double x1, double x2, double y1,double y2)
        {
            upright = new GISVertex(Math.Max(x1, x2), Math.Max(y1, y2));
            bottomleft = new GISVertex(Math.Min(x1, x2), Math.Min(y1,y2));
        }
        public double getMinX()
        {
            return bottomleft.x;
        }
        public double getMaxX()
        {
            return upright.x;
        }
        public double getMinY()
        {
            return bottomleft.y;
        }
        public double getMaxY()
        {
            return upright.y;
        }
        public double getWidth()
        {
            return upright.x - bottomleft.x;
        }
        public double getHeight()
        {
            return upright.y - bottomleft.y;
        }
    }
    class GISView
    {
        GISExtent CurrentMapExtent;//显示的地图范围
        Rectangle MapWindowSize;//绘图窗口的大小
        double MapMinX;
        double MapMinY;
        int WinW;
        int WinH;
        double MapW;
        double MapH;
        double ScaleX;
        double ScaleY;
        public GISView(GISExtent _extent, Rectangle _rectangle)
        {
            Update(_extent, _rectangle);
        }
        public void Update(GISExtent _extent, Rectangle _rectangle)
        {
            CurrentMapExtent = _extent;
            MapWindowSize = _rectangle;
            MapMinX = CurrentMapExtent.getMinX();
            MapMinY = CurrentMapExtent.getMinY();
            WinW = MapWindowSize.Width;
            WinH = MapWindowSize.Height;
            MapW = CurrentMapExtent.getWidth();
            MapH = CurrentMapExtent.getHeight();
            ScaleX = MapW / WinW;
            ScaleY = MapH / WinH;
        }
        public Point ToScreenPoint(GISVertex onevertex)
        {
            double ScreenX = (onevertex.x - MapMinY) / ScaleX;
            double ScreenY = WinH - (onevertex.y - MapMinY) / ScaleY;
            return new Point((int)ScreenX, (int)ScreenY);
        }
        public GISVertex ToMapVertex(Point point)
        {
            double MapX = ScaleX * point.X + MapMinX;
            double MapY = ScaleY * (WinH - point.Y) + MapMinY;
            return new GISVertex(MapX, MapY);
        }
    }
}