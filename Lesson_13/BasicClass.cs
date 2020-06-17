using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGIS
{
    public class GISVertex
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
        public void WriteVertex(BinaryWriter bw)
        {
            bw.Write(x);
            bw.Write(y);
        }

        public GISVertex(BinaryReader br)
        {
            x = br.ReadDouble();
            y = br.ReadDouble();
        }
    }
    public class GISPoint : GISSpatial
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
    public class GISLine : GISSpatial
    {
        public List<GISVertex> Vertexs;
        public double Length;
        public GISLine(List<GISVertex> _vertexs)
        {
            Vertexs = _vertexs;
            centroid = GISTools.CalculateCentroid(_vertexs);
            extent = GISTools.CalculateExtent(_vertexs);
            Length = GISTools.CalculateLength(_vertexs);
        }
        public override void draw(Graphics graphics, GISView view)
        {
            Point[] points = GISTools.GetScreenPoints(Vertexs, view);
            graphics.DrawLines(new Pen(Color.Red, 2), points);
        }
        public GISVertex FromNode()
        {
            return Vertexs[0];
        }
        public GISVertex ToNode()
        {
            return Vertexs[Vertexs.Count - 1];
        }
    }
    public class GISPolygon : GISSpatial
    {
        public List<GISVertex> Vertexs;
        public double Area;
        public GISPolygon(List<GISVertex> _vertexs)
        {
            Vertexs = _vertexs;
            centroid = GISTools.CalculateCentroid(_vertexs);
            extent = GISTools.CalculateExtent(_vertexs);
            Area = GISTools.CalculateArea(_vertexs);
        }
        public override void draw(Graphics graphics, GISView view)
        {
            Point[] points = GISTools.GetScreenPoints(Vertexs, view);
            graphics.FillPolygon(new SolidBrush(Color.Yellow),points);
            graphics.DrawPolygon(new Pen(Color.White,2),points);
        }
    }
    public class GISFeature
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
    public class GISAttribute
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
        public int ValueCount()
        {
            return values.Count;
        }
    }
    public  abstract class GISSpatial
    {
        public GISVertex centroid;//空间实体的中心点
        public GISExtent extent;//空间范围（最小外接矩形）
        public abstract void draw(Graphics graphics, GISView view);
    }
    public class GISExtent
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
        public GISExtent(double x1, double x2, double y1, double y2)
        {
            upright = new GISVertex(Math.Max(x1, x2), Math.Max(y1, y2));
            bottomleft = new GISVertex(Math.Min(x1, x2), Math.Min(y1, y2));
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
                    newmaxy = ((getMinY() + getMaxY()) + getHeight() / ZoomingFactor) / 2;
                    break;
                case GISMapAction.zoomout:
                    newminx = ((getMinX() + getMaxX()) - getWidth() * ZoomingFactor) / 2;
                    newminy = ((getMinY() + getMaxY()) - getHeight() * ZoomingFactor) / 2;
                    newmaxx = ((getMinX() + getMaxX()) + getWidth() * ZoomingFactor) / 2;
                    newmaxy = ((getMinY() + getMaxY()) + getHeight() * ZoomingFactor) / 2;
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
                    newmaxx = getMaxX() + getWidth() * MovingFactor;
                    break;
                case GISMapAction.moveright:
                    newminx = getMinX() - getWidth() * MovingFactor;
                    newmaxx = getMaxX() - getWidth() * MovingFactor;
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
        public bool lnsertectOrNot(GISExtent extent)
        {
            return !(getMaxX() < extent.getMinX() || getMinX() > extent.getMaxX() || getMaxY() < extent.getMinY() || getMinY() > extent.getMaxY());
        }
        public GISVertex getCenter()
        {
            return new GISVertex((upright.x + bottomleft.x)/2 , (upright.y + bottomleft.y)/2);
        }
    }
    public class GISView
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
            ScaleX = Math.Max(ScaleX, ScaleY);
            ScaleY = ScaleX;
            MapW = MapWindowSize.Width * ScaleX;
            MapH = MapWindowSize.Height * ScaleY;
            GISVertex center = CurrentMapExtent.getCenter();
            MapMinX = center.x - MapW / 2;
            MapMinY = center.y - MapH / 2;
        }
        public Point ToScreenPoint(GISVertex onevertex)
        {
            double ScreenX = (onevertex.x - MapMinX) / ScaleX;
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
            Update(CurrentMapExtent, MapWindowSize);
        }
        public void UpdateRectangle(Rectangle rect)
        {
            MapWindowSize = rect;
            Update(CurrentMapExtent, MapWindowSize);
        }
        public GISExtent getRealExtent()
        {
            return new GISExtent(MapMinX,MapMinX+MapW,MapMinY, MapMinY+MapH);
        }
        public GISExtent RectToExtent(int x1, int x2,int y1, int y2)
        {
            GISVertex v1 = ToMapVertex(new Point(x1,y1));
            GISVertex v2 = ToMapVertex(new Point(x2, y2));
            return new GISExtent(v1.x, v2.x, v1.y, v2.y);
        }
    }
    public enum GISMapAction
    {
        zoomin, zoomout, moveup, movedown, moveleft, moveright
    };
    public class GISShapefile
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
        static ShapefileHeader ReadFileHeader(BinaryReader br)
        {
            return (ShapefileHeader)GISTools.FromBytes(br, typeof(ShapefileHeader));
        }
        public  static GISLayer ReadShapefile(string shapefilename)
        {
            FileStream fsr = new FileStream(shapefilename, FileMode.Open);
            BinaryReader br = new BinaryReader(fsr);
            ShapefileHeader sfh = ReadFileHeader(br);
            SHAPETYPE ShapeType = (SHAPETYPE)Enum.Parse(typeof(SHAPETYPE), sfh.ShapeType.ToString());
            GISExtent extent = new GISExtent(sfh.Xmax, sfh.Xmin, sfh.Xmin, sfh.Ymin);
            string dbffilename = shapefilename.Replace(".shp",".dbf");
            DataTable table = ReadDBF(dbffilename);
            GISLayer layer = new GISLayer(shapefilename, ShapeType, extent, ReadFields(table));
            int rowindex = 0;
            while (br.PeekChar() != -1)
            {
                RecordHeader rh = ReadRecordHeader(br);
                int RecordLength = FromBigToLittle(rh.RecordLength) * 2 - 4;
                byte[] RecordContent = br.ReadBytes(RecordLength);
                if (ShapeType == SHAPETYPE.point)
                {
                    GISPoint onepoint = ReadPoint(RecordContent);
                    GISFeature onefeature = new GISFeature(onepoint,ReadAtrribute(table, rowindex));
                    layer.AddFeature(onefeature);
                }
                if (ShapeType == SHAPETYPE.line)
                {
                    List<GISLine> lines = ReadLines(RecordContent);
                    for (int i = 0; i < lines.Count; i++)
                    {
                        GISFeature onefeature = new GISFeature(lines[i], ReadAtrribute(table, rowindex));
                        layer.AddFeature(onefeature);
                    }
                }
                if (ShapeType == SHAPETYPE.polygon)
                {
                    List<GISPolygon> polygons = ReadPolygon(RecordContent);
                    for (int i = 0; i < polygons.Count; i++)
                    {
                        GISFeature onefeature = new GISFeature(polygons[i], ReadAtrribute(table, rowindex));
                        layer.AddFeature(onefeature);
                    }
                }
                rowindex++;
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
        static RecordHeader ReadRecordHeader(BinaryReader br)
        {
            return (RecordHeader)GISTools.FromBytes(br, typeof(RecordHeader));
        }
        static int FromBigToLittle(int bigvalue)
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
        static GISPoint ReadPoint(byte[] RecordContent)
        {
            double x = BitConverter.ToDouble(RecordContent, 0);
            double y = BitConverter.ToDouble(RecordContent, 8);
            return new GISPoint(new GISVertex(x, y));
        }
        static List<GISLine> ReadLines(byte[] RecordContent)
        {
            int N = BitConverter.ToInt32(RecordContent, 32);
            int M = BitConverter.ToInt32(RecordContent,36);
            int[] parts = new int[N + 1];
            for (int i = 0; i < N; i++)
            {
                parts[i] = BitConverter.ToInt32(RecordContent,40 + i *4);
            }
            parts[N] = M;
            List<GISLine> lines = new List<GISLine>();
            for (int i = 0; i < N; i++)
            {
                List<GISVertex> vertexs = new List<GISVertex>();
                for (int j = parts[i]; j <parts[i + 1] ; j++)
                {
                    double x = BitConverter.ToDouble(RecordContent, 40 + N * 4 + j * 16);
                    double y = BitConverter.ToDouble(RecordContent, 40 + N * 4 + j * 16 + 8);
                    vertexs.Add(new GISVertex(x,y));
                }
                lines.Add(new GISLine(vertexs));
            }
            return lines;
        }
        static List<GISPolygon> ReadPolygon(byte[] RecordContent)
        {
            int N = BitConverter.ToInt32(RecordContent, 32);
            int M = BitConverter.ToInt32(RecordContent, 36);
            int[] parts = new int[N + 1];
            for (int i = 0; i < N; i++)
            {
                parts[i] = BitConverter.ToInt32(RecordContent, 40 + i * 4);
            }
            parts[N] = M;
            List<GISPolygon> polygons = new List<GISPolygon>();
            for (int i = 0; i < N; i++)
            {
                List<GISVertex> vertexs = new List<GISVertex>();
                for (int j = parts[i]; j < parts[i + 1]; j++)
                {
                    double x = BitConverter.ToDouble(RecordContent, 40 + N * 4 + j * 16);
                    double y = BitConverter.ToDouble(RecordContent, 40 + N * 4 + j * 16 + 8);
                    vertexs.Add(new GISVertex(x, y));
                }
                polygons.Add(new GISPolygon(vertexs));
            }
            return polygons;
        }
        static  DataTable ReadDBF(string dbffilename)
        {
           FileInfo f = new FileInfo(dbffilename);
            DataSet ds = null;
            string constr = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source="+f.DirectoryName + ";Extended Properties = DBASE III";
            using (OleDbConnection con = new OleDbConnection(constr))
            {
                var sql = "select * from " + f.Name;
                OleDbCommand cmd = new OleDbCommand(sql, con);
                con.Open();
                ds = new DataSet();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(ds);
            }
            return ds.Tables[0];
        }
        static List<GISField> ReadFields(DataTable table)
        {
            List<GISField> fields = new List<GISField>();
            foreach (DataColumn column in table.Columns)
            {
                fields.Add(new GISField(column.DataType, column.ColumnName));
            }
            return fields;
        }
        static GISAttribute ReadAtrribute(DataTable table, int RowIndex)
        {
            GISAttribute attribute = new GISAttribute();
            DataRow row = table.Rows[RowIndex];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                attribute.AddValue(row[i]);
            }
            return attribute;
        }
    }
    public enum SHAPETYPE
    {
        point = 1,
        line = 3,
        polygon = 5
    };
    public  class GISLayer
    {
        public string Name;
        public GISExtent Extent;
        public bool DrawAttributeOrNot = false;
        public int LabelIndex;
        public SHAPETYPE ShapeType;
        List<GISFeature> Features = new List<GISFeature>();
        public List<GISField> Fields;
        public GISLayer(string _name, SHAPETYPE _shapetype, GISExtent _extent, List<GISField> _fields)
        {
            Name = _name;
            ShapeType = _shapetype;
            Extent = _extent;
            Fields = _fields;
        }
        public GISLayer(string _name, SHAPETYPE _shapetype, GISExtent _extent)
        {
            Name = _name;
            ShapeType = _shapetype;
            Extent = _extent;
        }
        public void draw(Graphics graphics, GISView view)
        {
            GISExtent extent = view.getRealExtent();
            for (int i = 0; i < Features.Count; i++)
            {
                if(extent.lnsertectOrNot(Features[i].spatialpart.extent))
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
        public GISFeature GetFeature(int i)
        {
            return Features[i];
        }
        public SelectResult Select(GISExtent extent)
        {
            GISSelect gs = new GISSelect();
            SelectResult sr = gs.Select(extent, Features);
            if (sr == SelectResult.OK)
            {
                for (int i = 0; i < gs.SelectedFeatures.Count; i++)
                    if (gs.SelectedFeatures[i].Selected == false)
                    {
                        gs.SelectFeatures[i].Selected = true;
                        Selection.Add(gs.SelectedFeatures[i]);
                    }
            }
            return sr;
        }
    }
    public class GISTools
    {
        public static GISVertex CalculateCentroid(List<GISVertex> _vertexs)
        {
            if (_vertexs.Count == 0)
            {
                return null;
            }
            double x = 0;
            double y = 0;
            for (int i = 0; i < _vertexs.Count; i++)
            {
                x += _vertexs[i].x;
                y += _vertexs[i].y;
            }
            return new GISVertex(x / _vertexs.Count, y / _vertexs.Count);
        }
        public static GISExtent CalculateExtent(List<GISVertex> _vertexs)
        {
            if (_vertexs.Count == 0)
            {
                return null;
            }
            double minx = Double.MaxValue;
            double miny = Double.MaxValue;
            double maxx = Double.MinValue;
            double maxy = Double.MinValue;
            for (int i = 0; i < _vertexs.Count; i++)
            {
                if (_vertexs[i].x < minx) minx = _vertexs[i].x;
                if (_vertexs[i].x < maxx) maxx = _vertexs[i].x;
                if (_vertexs[i].y < miny) minx = _vertexs[i].y;
                if (_vertexs[i].y < maxy) maxy = _vertexs[i].y;
            }
            return new GISExtent(minx, maxx, miny, maxy);
        }
        public static double CalculateLength(List<GISVertex> _vertexs)
        {
            double length = 0;
            for (int i = 0; i < _vertexs.Count; i++)
            {
                length += _vertexs[i].Distance(_vertexs[i + 1]);
            }
            return length;
        }
        public static double CalculateArea(List<GISVertex> _vertexs)
        {
            double area = 0;
            for (int i = 0; i < _vertexs.Count - 1; i++)
            {
                area += VectorProduct(_vertexs[i], _vertexs[i + 1]);
            }
            area += VectorProduct(_vertexs[_vertexs.Count - 1], _vertexs[0]);
            return area / 2;
        }
        public static double VectorProduct (GISVertex v1, GISVertex v2)
        {
            return v1.x * v2.y - v1.y * v2.y;
        }
        public static Point[] GetScreenPoints(List<GISVertex> _vertexs, GISView view)
        {
            Point[] points = new Point[_vertexs.Count];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = view.ToScreenPoint(_vertexs[i]);
            }
            return points;
        }
        public static byte[] ToBytes(object c)
        {
            byte[] bytes = new byte[Marshal.SizeOf(c.GetType())];
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            Marshal.StructureToPtr(c,handle.AddrOfPinnedObject(),false);
            handle.Free();
            return bytes;
        }
        public static void WriteString(string s, BinaryWriter bw)
        {
            bw.Write(StringLength(s));
            byte[] sbytes = Encoding.Default.GetBytes(s);
            bw.Write(sbytes);
        }
        public static int StringLength(string s)
        {
            int ChineseCount = 0;
            byte[] bs = new ASCIIEncoding().GetBytes(s);
            foreach (byte b in bs)
            {
                if (b == 0X3F)
                  ChineseCount++;
            }          
            return ChineseCount + bs.Length;
        }
        public static int TypeToInt(Type type)
        {
            ALLTYPES onetype = (ALLTYPES)Enum.Parse(typeof(ALLTYPES), type.ToString().Replace(".", "_"));
            return (int)onetype;
        }
        public static Object FromBytes(BinaryReader br, Type type)
        {
            byte[] buff = br.ReadBytes(Marshal.SizeOf(type));
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            Object result = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
            handle.Free();
            return result;
        }
        public static string ReadString(BinaryReader br)
        {
            int length = br.ReadInt32();
            byte[] sbytes = br.ReadBytes(length);
            return Encoding.Default.GetString(sbytes);
        }
        public static Type IntToType(int index)
        {
            string typestring = Enum.GetName(typeof(ALLTYPES), index);
            typestring = typestring.Replace("_",".");
            return Type.GetType(typestring);
        }
    }
    public class GISField
    {
        public Type datatype;
        public string name;
        public GISField(Type _dt, string _name)
        {
            datatype = _dt;
            name = _name;
        }
    }
    public class GISMyFile
    {
        [StructLayout(LayoutKind.Sequential, Pack =4)]
        struct MyFileHeader
        {
            public double MinX, MinY, MaxX, MaxY;
            public int FeatureCount, ShapeType, FieldCount;
        };
        static void WriteFileHeader(GISLayer layer, BinaryWriter bw)
        {
            MyFileHeader mfh = new MyFileHeader();
            mfh.MinX = layer.Extent.getMinX();
            mfh.MinY = layer.Extent.getMinY();
            mfh.MaxX = layer.Extent.getMaxX();
            mfh.MaxY = layer.Extent.getMaxY();
            mfh.FeatureCount = layer.FeatureCount();
            mfh.ShapeType = (int)(layer.ShapeType);
            mfh.FieldCount = layer.Fields.Count;
            bw.Write(GISTools.ToBytes(mfh));
        }
        public static void WriteFile(GISLayer layer, string filename)
        {
            FileStream fsr = new FileStream(filename,FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fsr);
            WriteFileHeader(layer, bw);
            GISTools.WriteString(layer.Name, bw);
            WriteFields(layer.Fields, bw);
            WriteFeatures(layer, bw);
            bw.Close();
            fsr.Close();
        }
        static void WriteFields(List<GISField> fields, BinaryWriter bw)
        {
            for (int fieldindex = 0; fieldindex < fields.Count; fieldindex++)
            {
                GISField field = fields[fieldindex];
                bw.Write(GISTools.TypeToInt(field.datatype));
                GISTools.WriteString(field.name, bw);
            }
        }
       static void WriteMultipleVertexes(List<GISVertex> vs, BinaryWriter bw)
        {
            bw.Write(vs.Count);
            for (int vc = 0; vc < vs.Count; vc++)
            vs[vc].WriteVertex(bw);
        }
        static void WriteAttributes(GISAttribute attribute, BinaryWriter bw)
        {
            for (int i = 0; i < attribute.ValueCount(); i++)
            {
                Type type = attribute.GetValue(i).GetType();
                if (type.ToString() == "System.Boolean")
                    bw.Write((bool)attribute.GetValue(i));
                else if (type.ToString() == "System.Byte")
                    bw.Write((byte)attribute.GetValue(i));
                else if (type.ToString() == "System.Char")
                    bw.Write((char)attribute.GetValue(i));
                else if (type.ToString() == "System.Decimal")
                    bw.Write((decimal)attribute.GetValue(i));
                else if (type.ToString() == "System.Double")
                    bw.Write((double)attribute.GetValue(i));
                else if (type.ToString() == "System.Float")
                    bw.Write((float)attribute.GetValue(i));
                else if (type.ToString() == "System.Int")
                    bw.Write((int)attribute.GetValue(i));
                else if (type.ToString() == "System.Long")
                    bw.Write((long)attribute.GetValue(i));
                else if (type.ToString() == "System.Ushort")
                    bw.Write((ushort)attribute.GetValue(i));
                else if (type.ToString() == "System.Uint")
                    bw.Write((uint)attribute.GetValue(i));
                else if (type.ToString() == "System.Ulong")
                    bw.Write((ulong)attribute.GetValue(i));
                else if (type.ToString() == "System.Sbyte")
                    bw.Write((sbyte)attribute.GetValue(i));
                else if (type.ToString() == "System.Short")
                    bw.Write((short)attribute.GetValue(i));
                else if (type.ToString() == "System.String")
                    GISTools.WriteString((string)attribute.GetValue(i),bw);
            }
        }
        static void WriteFeatures(GISLayer layer, BinaryWriter bw)
        {
            for (int featureindex = 0; featureindex < layer.FeatureCount(); featureindex++)
            {
                GISFeature feature = layer.GetFeature(featureindex);
                 if (layer.ShapeType == SHAPETYPE.point)
                {
                    ((GISPoint)feature.spatialpart).centroid.WriteVertex(bw);
                }
                if (layer.ShapeType == SHAPETYPE.line)
                {
                    GISLine line = (GISLine)feature.spatialpart;
                    WriteMultipleVertexes(line.Vertexs, bw);
                }
                if (layer.ShapeType == SHAPETYPE.polygon)
                {
                    GISPolygon polygon = (GISPolygon)feature.spatialpart;
                    WriteMultipleVertexes(polygon.Vertexs, bw);
                }
                WriteAttributes(feature.attributepart, bw);
            }
       }
        static List<GISField> ReadFields(BinaryReader br, int FieldCount)
        {
            List<GISField> fields = new List<GISField>();
            for (int fieldindex = 0; fieldindex < FieldCount; fieldindex++)
            {
                Type fieldtype = GISTools.IntToType(br.ReadInt32());
                string fieldname = GISTools.ReadString(br);
                fields.Add(new GISField(fieldtype, fieldname));
            }
            return fields;
        }
        static List<GISVertex> ReadMultipleVertexes(BinaryReader br)
        {
            List < GISVertex > vs = new List<GISVertex>();
            int vcount = br.ReadInt32();
            for (int vc = 0; vc < vcount; vc++)
                vs.Add(new GISVertex(br));
            return vs;
        }
        static GISAttribute ReadAttributes(List<GISField> fs, BinaryReader br)
        {
            GISAttribute atrribute = new GISAttribute();
            for (int i = 0; i < fs.Count; i++)
            {
                Type type = fs[i].datatype;
                if (type.ToString() == "System.Boolean")
                    atrribute.AddValue(br.ReadBoolean());
                else if (type.ToString() == "System.Byte")
                    atrribute.AddValue(br.ReadByte());
                else if (type.ToString() == "System.Char")
                    atrribute.AddValue(br.ReadChar());
                else if (type.ToString() == "System.Decimal")
                    atrribute.AddValue(br.ReadDecimal());
                else if (type.ToString() == "System.Double")
                    atrribute.AddValue(br.ReadDouble());
                else if (type.ToString() == "System.Single")
                    atrribute.AddValue(br.ReadSingle());
                else if (type.ToString() == "System.Int32")
                    atrribute.AddValue(br.ReadInt32());
                else if (type.ToString() == "System.Int64")
                    atrribute.AddValue(br.ReadInt64());
                else if (type.ToString() == "System.Uint16")
                    atrribute.AddValue(br.ReadUInt16());
                else if (type.ToString() == "System.Uint32")
                    atrribute.AddValue(br.ReadUInt32());
                else if (type.ToString() == "System.Uint64")
                    atrribute.AddValue(br.ReadUInt64());
                else if (type.ToString() == "System.SByte")
                    atrribute.AddValue(br.ReadSByte());
                else if (type.ToString() == "System.Int16")
                    atrribute.AddValue(br.ReadInt16());
                else if (type.ToString() == "System.String")
                    atrribute.AddValue(GISTools.ReadString(br));
            }
            return atrribute;
        }
        static void ReadFeatures(GISLayer layer, BinaryReader br, int FeatureCount)
        {
            for (int featureindex = 0; featureindex < FeatureCount; featureindex++)
            {
                GISFeature feature = new GISFeature(null, null);
                if (layer.ShapeType == SHAPETYPE.point)
                    feature.spatialpart = new GISPoint(new GISVertex(br));
                else if(layer.ShapeType == SHAPETYPE.line)
                    feature.spatialpart = new GISLine(ReadMultipleVertexes(br));
                else if (layer.ShapeType == SHAPETYPE.polygon)
                    feature.spatialpart = new GISPolygon(ReadMultipleVertexes(br));
                feature.attributepart = ReadAttributes(layer.Fields, br);
                layer.AddFeature(feature);
            }
        }
        public static GISLayer ReadFile(string filename)
        {
            FileStream fsr = new FileStream(filename, FileMode.Open);
            BinaryReader br = new BinaryReader(fsr);
            MyFileHeader mfh = (MyFileHeader)(GISTools.FromBytes(br,typeof(MyFileHeader)));
            SHAPETYPE ShapeType = (SHAPETYPE)Enum.Parse(typeof(SHAPETYPE), mfh.ShapeType.ToString());
            GISExtent Extent = new GISExtent(mfh.MinX, mfh.MaxX, mfh.MinY, mfh.MaxY);
            string layename = GISTools.ReadString(br);
            List<GISField> Fields = ReadFields(br, mfh.FieldCount);
            GISLayer layer = new GISLayer(layename, ShapeType, Extent, Fields);
            ReadFeatures(layer, br, mfh.FeatureCount);
            br.Close();
            fsr.Close();
            return layer;
        }
    }
    public enum ALLTYPES
    {
        System_Boolean,
        System_Byte,
        System_Char,
        System_Decimal,
        System_Double,
        System_Single,
        System_Int32,
        System_int64,
        System_SByte,
        System_Int16,
        System_String,
        System_UInt32,
        System_UInt64,
        System_UInt16
    };
    public class GISConst
    {
        public static Color ZoomSelectBoxColor = Color.FromArgb(50, 0, 0, 0);
    }
    public enum MOUSECOMMAND
    {
        Unused,Select,ZoomIn,ZoomOut,Pan
    };
}