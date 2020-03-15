using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

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
        public void CopyFrom(GISVertex v)
        {
            x = v.x;
            y = v.y;
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
        double ZoomingFactor = 2;
        double MovingFactor = 0.25;
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
        public void ChangeExtent(GISMapAction action)
        {
            double newminx = bottomleft.x;
            double newminy = bottomleft.y;
            double newmaxx = upright.x;
            double newmaxy = upright.y;
            switch (action)
            {
                case GISMapAction.zoomin:
                    newminx = ((getMinX() + getMaxX()) - getWidth() / ZoomingFactor) / 2;
                    newminy = ((getMinY() + getMaxY()) - getHeight() / ZoomingFactor) / 2;
                    newmaxx = ((getMinX() + getMaxX()) + getWidth() / ZoomingFactor) / 2;
                    newminx = ((getMinY() + getMaxY()) + getHeight() / ZoomingFactor) / 2;
                    break;
                case GISMapAction.zoomout:
                    newminx = ((getMinX() + getMaxX()) - getWidth() * ZoomingFactor) / 2;
                    newminy = ((getMinY() + getMaxY()) - getHeight() * ZoomingFactor) / 2;
                    newmaxx = ((getMinX() + getMaxX()) + getWidth() * ZoomingFactor) / 2;
                    newminx = ((getMinY() + getMaxY()) + getHeight() * ZoomingFactor) / 2;
                    break;
                case GISMapAction.moveup:
                    newminy = getMinY() - getHeight() * MovingFactor;
                    newmaxy = getMaxY() - getHeight() * MovingFactor;
                    break;
                case GISMapAction.movedown:
                    newminy = getMinY() + getHeight() * MovingFactor;
                    newmaxy = getMaxY() + getHeight() * MovingFactor;
                    break;
                case GISMapAction.moveleft:
                    newminx = getMinX() + getWidth() * MovingFactor;
                    newmaxy = getMaxX() + getWidth() * MovingFactor;
                    break;
                case GISMapAction.moveright:
                    newminx = getMinX() - getWidth() * MovingFactor;
                    newmaxy = getMaxX() - getWidth() * MovingFactor;
                    break;
            }
                upright.x = newmaxx;
                upright.y = newmaxy;
                bottomleft.x = newminx;
                bottomleft.y = newminy;
        }
        public void CopyFrom(GISExtent extent)
        {
            upright.CopyFrom(extent.upright);
            bottomleft.CopyFrom(extent.bottomleft);
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
        public void ChangeView(GISMapAction action)
        {
            CurrentMapExtent.ChangeExtent(action);
            Update(CurrentMapExtent, MapWindowSize);
        }
        public void UpdateExtent(GISExtent extent)
        {
            CurrentMapExtent.CopyFrom(extent);
            Update(CurrentMapExtent,MapWindowSize);
        }
    }
    enum GISMapAction
    {
        zoomin, zoomout, moveup, movedown, moveleft, moveright
    };
    class GISShapefile
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct ShapefileHeader
        {
            public int Unused1, Unused2, Unused3, Unused4;
            public int Unused5, Unused6, Unused7, Unused8;
            public int ShapeType;
            public double Xmin;
            public double Ymin;
            public double Xmax;
            public double Ymax;
            public double Unused9, Unused10, Unused11, Unused12;
        };
        ShapefileHeader ReadFileHeader(BinaryReader br)
        {
            byte[] buff = br.ReadBytes(Marshal.SizeOf(typeof(ShapefileHeader)));
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            ShapefileHeader header = (ShapefileHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ShapefileHeader));
            handle.Free();
            return header;
        }
        public GISLayer ReadShapefile(string shapefilename)
        {
            FileStream fsr = new FileStream(shapefilename, FileMode.Open);
            BinaryReader br = new BinaryReader(fsr);
            ShapefileHeader sfh = ReadFileHeader(br);
            SHAPETYPE ShapeType = (SHAPETYPE)Enum.Parse(typeof(SHAPETYPE), sfh.ShapeType.ToString());
            GISExtent extent = new GISExtent(sfh.Xmax, sfh.Xmin, sfh.Xmin, sfh.Ymin);
            GISLayer layer = new GISLayer(shapefilename, ShapeType, extent);
            while (br.PeekChar() != -1)
            {
                RecordHeader rh = ReadRecordHeader(br);
                int RecordLength = FromBigToLittle(rh.RecordLength) * 2 - 4;
                byte[] RecordContent = br.ReadBytes(RecordLength);
                if (ShapeType == SHAPETYPE.point)
                {
                    GISPoint onepoint = ReadPoint(RecordContent);
                    GISFeature onefeature = new GISFeature(onepoint, new GISAttribute());
                    layer.AddFeature(onefeature);
                }
            }
            br.Close();
            fsr.Close();
            return layer;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct RecordHeader
        {
            public int RecordNumber;
            public int RecordLength;
            public int ShapeType;
        }
        RecordHeader ReadRecordHeader(BinaryReader br)
        {
            byte[] buff = br.ReadBytes(Marshal.SizeOf(typeof(RecordHeader)));
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            RecordHeader header = (RecordHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(RecordHeader));
            handle.Free();
            return header;
        }
        int FromBigToLittle(int bigvalue)
        {
            byte[] bigbytes = new byte[4];
            GCHandle handle = GCHandle.Alloc(bigbytes, GCHandleType.Pinned);
            Marshal.StructureToPtr(bigvalue, handle.AddrOfPinnedObject(), false);
            handle.Free();
            byte b2 = bigbytes[2];
            byte b3 = bigbytes[3];
            bigbytes[3] = bigbytes[0];
            bigbytes[2] = bigbytes[1];
            bigbytes[1] = b2;
            bigbytes[0] = b3;
            return BitConverter.ToInt32(bigbytes, 0);
        }
        GISPoint ReadPoint(byte[] RecordContent)
        {
            double x = BitConverter.ToDouble(RecordContent, 0);
            double y = BitConverter.ToDouble(RecordContent, 8);
            return new GISPoint(new GISVertex(x, y));
        }
    }
    enum SHAPETYPE
    {
        point = 1,
        line = 3,
        polygon = 5
    };
    class GISLayer
    {
        public string Name;
        public GISExtent Extent;
        public bool DrawAttributeOrNot;
        public int LabelIndex;
        public SHAPETYPE ShapeType;
        List<GISFeature> Features = new List<GISFeature>();
        public GISLayer(string _name, SHAPETYPE _shapetype, GISExtent _extent)
        {
            Name = _name;
            ShapeType = _shapetype;
            Extent = _extent;
        }
        public void draw(Graphics graphics, GISView view)
        {
            for (int i = 0; i < Features.Count; i++)
            {
                Features[i].draw(graphics, view, DrawAttributeOrNot, LabelIndex);
            }
        }
        public void AddFeature(GISFeature feature)
        {
            Features.Add(feature);
        }
        public int FeatureCount()
        {
            return Features.Count;
        }
    }
}