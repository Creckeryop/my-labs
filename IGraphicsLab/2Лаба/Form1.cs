using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
namespace _2Лаба
{
    public partial class Form1 : Form
    {
        public class DirectBitmap : IDisposable
        {
            public Bitmap Bitmap { get; private set; }
            public Int32[] Bits { get; private set; }
            public bool Disposed { get; private set; }
            public int Height { get; private set; }
            public int Width { get; private set; }

            protected GCHandle BitsHandle { get; private set; }

            public DirectBitmap(int width, int height)
            {
                Width = width;
                Height = height;
                Bits = new Int32[width * height];
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            }

            public void SetPixel(int x, int y, Color colour)
            {
                int index = x + (y * Width);
                int col = colour.ToArgb();

                Bits[index] = col;
            }

            public Color GetPixel(int x, int y)
            {
                int index = x + (y * Width);
                int col = Bits[index];
                Color result = Color.FromArgb(col);

                return result;
            }

            public void Dispose()
            {
                if (Disposed) return;
                Disposed = true;
                Bitmap.Dispose();
                BitsHandle.Free();
            }
        }
        //Vector3[] Cube = new Vector3[] { new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1), new Vector3(1, -1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1) };
        //Vector3[] Cube2 = new Vector3[] { new Vector3(1, 1, 1), new Vector3(2, 1, 1), new Vector3(2, 2, 1), new Vector3(1, 2, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 2), new Vector3(2, 1, 2), new Vector3(2, 1, 1), new Vector3(2, 1, 2), new Vector3(2, 2, 2), new Vector3(2, 2, 1), new Vector3(2, 2, 2), new Vector3(1, 2, 2), new Vector3(1, 2, 1), new Vector3(1, 2, 2), new Vector3(1, 1, 2) };
        public class Plane
        {
            public Vector3[] Vertexes = null;
            public float[] abcd = null;
            public Vector3 Center = null;
            public Matrix Matrix = null;
            public Plane(Vector3[] vectors)
            {
                Vertexes = vectors;
                abcd = ABCD();
                Center = new Vector3();
                for (int i = 0; i < vectors.Length; i++)
                    Center = Center + vectors[i];
                Center = Center * (1.0f / vectors.Length);
            }
            private float[] ABCD()
            {
                var a = .0f;
                var b = .0f;
                var c = .0f;
                for (int i = 0; i < Vertexes.Length; i++)
                {
                    var j = (i == (Vertexes.Length-1)) ? 0 : (i + 1);
                    a += (Vertexes[i].Y - Vertexes[j].Y) * (Vertexes[i].Z + Vertexes[j].Z);
                    b += (Vertexes[i].Z - Vertexes[j].Z) * (Vertexes[i].X + Vertexes[j].X);
                    c += (Vertexes[i].X - Vertexes[j].X) * (Vertexes[i].Y + Vertexes[j].Y);
                }
                var d = -(a * Vertexes[0].X + b * Vertexes[0].Y + c * Vertexes[0].Z);
                Matrix = new Matrix(4, 1, new[,] { { a }, { b }, { c }, { d } });
                return new[] { a, b, c, d };
            }
        }
        Plane[] CreateCube(Vector3 O, float w)
        {
            var a = new Plane(new[] { O + new Vector3(-w, -w, -w), O + new Vector3(w, -w, -w), O + new Vector3(w, -w, w), O + new Vector3(-w, -w, w) });
            var b = new Plane(new[] { O + new Vector3(-w, w, -w), O + new Vector3(-w, w, w), O + new Vector3(w, w, w), O + new Vector3(w, w, -w) });
            var c = new Plane(new[] { O + new Vector3(-w, -w, w), O + new Vector3(w, -w, w), O + new Vector3(w, w, w), O + new Vector3(-w, w, w) });
            var d = new Plane(new[] { O + new Vector3(-w, -w, -w), O + new Vector3(-w, w, -w), O + new Vector3(w, w, -w), O + new Vector3(w, -w, -w)});
            var e = new Plane(new[] { O + new Vector3(-w, w, -w), O + new Vector3(-w, -w, -w), O + new Vector3(-w, -w, w), O + new Vector3(-w, w, w)   });
            var f = new Plane(new[] { O + new Vector3(w, w, -w), O + new Vector3(w, w, w), O + new Vector3(w, -w, w), O + new Vector3(w, -w, -w) });

            return new[] { a, b, c, d, e, f };
        }

        public class Camera
        {
            public Bitmap vladFace;
            public static void Swap<T>(IList<T> list, int indexA, int indexB)
            {
                T tmp = list[indexA];
                list[indexA] = list[indexB];
                list[indexB] = tmp;
            }
            public Vector3 Point { get; set; } = null;
            public Vector3 PointS { get; set; } = null;
            public Vector3 N { get; set; } = null;
            public Vector3 N2 { get; set; } = null;
            public Vector3 PointS2 { get; set; } = null;
            public Vector3 i;
            public Vector3 j;
            public Vector3 k;
            public float[,] zmap = new float[640, 640];
            public DirectBitmap colormap = new DirectBitmap(640, 640);
            public float Rotation = 0;
            public float D { get => -N.X*PointS.X - N.Y * PointS.Y - N.Z * PointS.Z; }
            public float A2 { get => N2.X; }
            public float B2 { get => N2.Y; }
            public float C2 { get => N2.Z; }
            public float D2 { get => -N2.X * PointS2.X - N2.Y * PointS2.Y - N2.Z * PointS2.Z; }
            public Matrix Base = null;
            public Matrix Matrix = null;
            public Camera(Vector3 Start, float A, float B, float C)
            {
                Point = Start;
                N = new Vector3(A, B, C);
                i = new Vector3(0, 0, 1);
                j = new Vector3(0, 1, 0);
                k = new Vector3(-1, 0, 0);
                N2 = N.Normalize() * -0.01f;
                PointS = Start + N;
                PointS2 = Start + N2;
                Base = new Matrix(3, 3, new[,] { { k.X, k.Y, k.Z }, { i.X, i.Y, i.Z }, { j.X, j.Y, j.Z } });
                Matrix = new Matrix(4, 1, new[,] { { A }, { B }, { C }, { D } });
                Rotation = 0;
                vladFace = new Bitmap("texture.png");
            }
            public void Move(Vector3 dV)
            {
                Point += dV;
                PointS = N + Point;
                PointS2 = N2 + Point;
            }
            public void Angle(Vector3 dV)
            {
                N += dV;
                PointS = N + Point;
                UpdateMAX();
            }
            public float sgn(float a) => a > 0 ? 1 : -1;
            public PointF OnCamera(Vector3 p)
            {
                var V = Point - p;
                var t = -(p.M1 * Matrix)[0, 0] / (V.M0 * Matrix)[0, 0];
                var cross = p + V * (float)t;
                var ab = cross.M * Base.A_1();
                return new PointF((float)ab[0, 0], (float)ab[0, 1]);
            }
            public PointF OnCamera(Vector3 p, float ox, float oy)
            {
                var V = Point - p;
                var t = -(p.M1 * Matrix)[0, 0] / (V.M0 * Matrix)[0, 0];
                var cross = p + V * (float)t;
                var ab = cross.M * Base.A_1();
                return new PointF((float)ab[0, 0] + ox, (float)ab[0, 1] + oy);
            }
            public Vector3 FromCamera(PointF p)
            {
                var newP = PointS.M * Base.A_1();
                var newN = N.M * Base.A_1();
                var a = newN[0, 0];
                var b = newN[0, 1];
                var c = newN[0, 2];
                var d = -newP[0, 0] * a - newP[0, 1] * b - newP[0, 2] * c;
                var z = -(a * p.X + b * p.Y + d) / c;
                return Vector3.Load(new Matrix(1, 3, new[,] { { p.X, p.Y, z } }) * Base);
            }
            public Vector3 FromCameraToPlane(Plane a, PointF c)
            {
                var Matrix = new Matrix(4, 1, new[,] { { a.abcd[0] }, { a.abcd[1] }, { a.abcd[2] }, { a.abcd[3] } });
                var p = FromCamera(c);
                var V = (Point - p).Normalize();
                var tmp = -(p.M1 * Matrix)[0, 0] / (V.M0 * Matrix)[0, 0];
                var cross = p + V * (float)tmp;
                return cross;
            }
            public void drawLine(PaintEventArgs e,float ox,float oy, Vector3 a, Vector3 b, Pen brush)
            {
                var A = new Matrix(4, 1, new[,] { { A2 }, { B2 }, { C2 }, { D2 } });
                bool is_a = (a.M1 * A)[0, 0] > 0, is_b = (b.M1 * A)[0, 0] > 0;
                if (is_a || is_b)
                {
                    if (is_a && !is_b)
                    {
                        var V = b - a;
                        var t = -(a.M1 * A)[0, 0] / (V.M0 * A)[0, 0];
                        b = a + V * (float)t;
                    }
                    if (is_b && !is_a)
                    {
                        var V = a - b;
                        var t = -(b.M1 * A)[0, 0] / (V.M0 * A)[0, 0];
                        a = b + V * (float)t;
                    }
                    var p1 = OnCamera(a);
                    var p2 = OnCamera(b);
                    e.Graphics.DrawLine(brush, ox + p1.X, oy + p1.Y, ox + p2.X, oy + p2.Y);
                }
                //var p = OnCamera(Point+N.Normalize());
                //e.Graphics.DrawEllipse(Pens.Black, ox + p.X - 5, oy + p.Y - 5, 10, 10);
            }
            public void drawLines(PaintEventArgs e, float ox, float oy, Vector3[] AA, Vector3[] BB, Pen[] pens)
            {
                var A = new Matrix(4, 1, new[,] { { A2 }, { B2 }, { C2 }, { D2 } });
                for (int i = 0; i < AA.Length; i++)
                {
                    var a = AA[i];
                    var b = BB[i];
                    bool is_a = (a.M1 * A)[0, 0] > 0, is_b = (b.M1 * A)[0, 0] > 0;
                    if (is_a || is_b)
                    {
                        if (is_a && !is_b)
                        {
                            var V = b - a;
                            var t = -(a.M1 * A)[0, 0] / (V.M0 * A)[0, 0];
                            b = a + V * (float)t;
                        }
                        if (is_b && !is_a)
                        {
                            var V = a - b;
                            var t = -(b.M1 * A)[0, 0] / (V.M0 * A)[0, 0];
                            a = b + V * (float)t;
                        }
                        var p1 = OnCamera(a);
                        var p2 = OnCamera(b);
                        e.Graphics.DrawLine(pens[i], ox + p1.X, oy + p1.Y, ox + p2.X, oy + p2.Y);
                    }
                }
            }
            public void drawPlane(PaintEventArgs e, float ox, float oy, Plane plane)
            {
                var pointsA = new Vector3[plane.Vertexes.Length];
                var pointsB = new Vector3[plane.Vertexes.Length];
                var pens = new Pen[plane.Vertexes.Length];
                for (int i = 0; i < plane.Vertexes.Length; i++)
                {
                    pointsA[i] = plane.Vertexes[i];
                    pointsB[i] = plane.Vertexes[(i+1)%plane.Vertexes.Length];
                    pens[i] = Pens.Black;
                }
                drawLines(e, ox, oy, pointsA, pointsB, pens);
            }
            public void drawPlanes(PaintEventArgs e, float ox, float oy, Plane[] planes)
            {
                foreach (var plane in planes)
                {
                    drawPlane(e, ox, oy, plane);
                }
            }
            public Vector3[] FixLine(Vector3 a, Vector3 b)
            {
                var A = new Matrix(4, 1, new[,] { { A2 }, { B2 }, { C2 }, { D2 } });
                bool is_a = (a.M1 * A)[0, 0] > 0, is_b = (b.M1 * A)[0, 0] > 0;
                if (is_a || is_b)
                {
                    if (is_a && !is_b)
                    {
                        var V = b - a;
                        var t = -(a.M1 * A)[0, 0] / (V.M0 * A)[0, 0];
                        b = a + V * (float)t;
                    }
                    if (is_b && !is_a)
                    {
                        var V = a - b;
                        var t = -(b.M1 * A)[0, 0] / (V.M0 * A)[0, 0];
                        a = b + V * (float)t;
                    }
                    return new[] { a, b };
                }
                return null;
            }
            public void SuperDrawPlanes(PaintEventArgs e, float ox, float oy, Plane[] planes)
            {
                var polys = new List<Vector3[]>();
                var zs = new List<float>();
                foreach (var plane in planes)
                {
                    var Polygon = new Vector3[plane.Vertexes.Length];
                    var avg = new Vector3();
                    int i = 0;
                    foreach (var p in plane.Vertexes)
                    {
                        avg += p;
                        var ps = OnCamera(p);
                    }
                    avg = avg * (1.0f / plane.Vertexes.Length);
                    polys.Add(plane.Vertexes);
                    zs.Add((float)(avg.M1 * Matrix)[0, 0]);
                }
                for(int i = 0; i < zs.Count; i++)
                {
                    for (int j=i; j < zs.Count; j++)
                    {
                        if (zs[i] > zs[j])
                        {
                            Swap(zs, j, i);
                            Swap(polys, j, i);
                        }
                    }
                }
                var Polygons = new List<PointF[]>();
                for (int i=0;i<polys.Count;i++)
                {
                    var Polygon = new List<PointF>();
                    for (int j = 0; j < polys[i].Length; j++) {
                        var a = polys[i][j];
                        var b = polys[i][(j + 1) % polys[i].Length];
                        var n = FixLine(a, b);
                        if (n != null)
                        {
                            var t = OnCamera(n[0]);
                            var t2 = OnCamera(n[1]);
                            Polygon.AddRange(new[]{new PointF(t.X + ox, t.Y + oy), new PointF(t2.X + ox, t2.Y + oy) });
                        }
                    }
                    if (Polygon.Count > 2)
                        Polygons.Add(Polygon.ToArray());
                }
                foreach (var p in Polygons)
                {
                    e.Graphics.DrawPolygon(Pens.Black, p);
                    e.Graphics.FillPolygon(Brushes.White, p);
                }
            }
            public void SuperDrawPlane(PaintEventArgs e, float ox, float oy, Plane plane)
            {
                var Polygon = new List<PointF>();
                for (int j = 0; j < plane.Vertexes.Length; j++)
                {
                    var a = plane.Vertexes[j];
                    var b = plane.Vertexes[(j + 1) % plane.Vertexes.Length];
                    var n = FixLine(a, b);
                    if (n != null)
                    {
                        var t = OnCamera(n[0]);
                        var t2 = OnCamera(n[1]);
                        Polygon.AddRange(new[] { new PointF(t.X + ox, t.Y + oy), new PointF(t2.X + ox, t2.Y + oy) });
                    }
                }
                if (Polygon.Count < 2) return;
                e.Graphics.DrawPolygon(Pens.Black, Polygon.ToArray());
                e.Graphics.FillPolygon(Brushes.White, Polygon.ToArray());
            }
            public Point[] MinsMaxs(PointF[] points)
            {
                var min_y = points[0].Y;
                var max_y = points[0].Y;
                var min_x = points[0].X;
                var max_x = points[0].X;
                foreach(var p in points)
                {
                    if (max_x < p.X)
                        max_x = p.X;
                    else
                        if (min_x > p.X)
                        min_x = p.X;
                    if (max_y< p.Y)
                        max_y = p.Y;
                    else
                        if (min_y > p.Y)
                        min_y = p.Y;
                }
                return new[] {new Point((int)min_x, (int)min_y), new Point((int)max_x, (int)max_y) };
            }
            public bool inside(float x, float y, PointF[] points)
            {
                
                var inside = false;
                for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                {
                    var xi = points[i].X;
                    var yi = points[i].Y;
                    var xj = points[j].X;
                    var yj = points[j].Y;
                    
                    if (((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi)) inside = !inside;
                }
                return inside;
            }
            public int[] ScanLineY(int y, PointF[] points)
            {
                List<int> intersections = new List<int>();
                for (int i = 0; i < points.Length; i++)
                {
                    int j = i == points.Length - 1 ? 0 : i + 1;
                    var a = points[i];
                    var b = points[j];
                    if (a.Y >= y == b.Y >= y) continue;
                    double r = a.Y - b.Y;
                    var q = a.X - (a.Y - y) * (a.X - b.X) / r;
                    var e = (a.Y - y) / r;
                    if (e >= 0 && e <= 1) 
                        intersections.Add((int)q);
                }
                intersections.Sort();
                return intersections.ToArray();
            }
            public float getFov(int left_x, int right_x)
            {
                var pl = Point-FromCamera(new PointF(left_x, 10));
                var pr = Point-FromCamera(new PointF(right_x, 10));
                return (float)(Math.Acos(pl*pr)*180/Math.PI);
            }
            public void SuperDrawPlanezB(PaintEventArgs e, float ox, float oy, Plane plane,Color color,int mode,int scale)
            {
                var Polygon = new List<PointF>();
                for (int i = 0; i < plane.Vertexes.Length; i++)
                {
                    int j = (i == plane.Vertexes.Length - 1) ? 0 : i + 1;
                    var a = plane.Vertexes[i];
                    var b = plane.Vertexes[j];
                    var n = FixLine(a, b);
                    if (n != null)
                    {
                        Polygon.Add(OnCamera(n[0],ox,oy));
                        Polygon.Add(OnCamera(n[1],ox,oy));
                    }
                }
                if (Polygon.Count < 2) return;
                var pol = Polygon.ToArray();
                var get = MinsMaxs(pol);
                var Matrix = new Matrix(4, 1, new[,] { { plane.abcd[0] }, { plane.abcd[1] }, { plane.abcd[2] }, { plane.abcd[3] } });
                var newP = PointS.M * Base.A_1();
                var newN = N.M * Base.A_1();
                {
                    int q = scale;
                    var a = newN[0, 0];
                    var b = newN[0, 1];
                    var c = newN[0, 2];
                    var d = -newP[0, 0] * a - newP[0, 1] * b - newP[0, 2] * c;
                    get[0].X = Math.Max(0,get[0].X);
                    get[1].X = Math.Min(640, get[1].X);
                    get[0].Y = Math.Max(0, get[0].Y);
                    get[1].Y = Math.Min(480, get[1].Y);
                    
                    Vector3 to_center = null;
                    Vector3 to_center_image = null;
                    Vector3 from_center = null;
                    Vector3 from_center_image = null;
                    Vector3 N_center = null;
                    Vector3 N_center_image = null;
                    Matrix newBase = null;
                    Matrix newBase_image = null;
                    Vector3 center = null;
                        to_center = plane.Center - plane.Vertexes[0];
                        to_center_image = new Vector3(vladFace.Width / 2, vladFace.Height / 2, 0);
                        from_center = plane.Vertexes[1] - plane.Vertexes[0];
                        from_center_image = new Vector3(vladFace.Width, 0, 0);
                        N_center = to_center ^ from_center;
                        N_center_image = to_center_image ^ from_center_image;
                        newBase = new Matrix(3, 3, new[,] { { to_center.X, to_center.Y, to_center.Z }, { from_center.X, from_center.Y, from_center.Z }, { N_center.X, N_center.Y, N_center.Z } });
                        newBase_image = new Matrix(3, 3, new[,] { { to_center_image.X, to_center_image.Y, to_center_image.Z }, { from_center_image.X, from_center_image.Y, from_center_image.Z }, { N_center_image.X, N_center_image.Y, N_center_image.Z } });
                        center = Vector3.Load((plane.Center.M * newBase.A_1()) * newBase_image);

                    var to_center_screen = new Vector3(ox, oy, 0);
                    var to_right_screen = new Vector3(2 * ox, 0, 0);
                    var N_center_screen = to_center_screen ^ to_right_screen;
                    var newBase_screen = new Matrix(3, 3, new[,] { { to_center_screen.X, to_center_screen.Y, to_center_screen.Z }, { to_right_screen.X, to_right_screen.Y, to_right_screen.Z }, { N_center_screen.X, N_center_screen.Y, N_center_screen.Z } });
                    for (int i = get[0].X - 1; i <= get[1].X; i+=q)
                    {
                        for (int j = get[0].Y - 1; j <= get[1].Y; j+=q)
                        {
                            if (i < 640 && i >= 0 && j < 480 && j >= 0 && inside(i, j, pol))
                            {
                                var z_oz = -(a * (i - ox) + b * (j - oy) + d) / c;
                                var p = Vector3.Load(new Matrix(1, 3, new[,] { { i - ox, j - oy, z_oz } }) * Base);
                                var V = Point - p;
                                var tmp = -(p.M1 * Matrix)[0, 0] / (V.M0 * Matrix)[0, 0];
                                var cross = p + V * (float)tmp;
                                var z = (float)(Point - cross).Length;
                                if (zmap[i, j] < z) continue;
                                Vector3 crossTex;
                                int t = 0, K = 0;
                                if (mode == 4)
                                {
                                    crossTex = Vector3.Load((cross - plane.Vertexes[0]).M * newBase.A_1());
                                    crossTex = Vector3.Load(crossTex.M * newBase_image);
                                    t = (int)(crossTex.X);
                                    K = (int)(crossTex.Y);
                                }
                                for (int k = i; k < i + q; k++) { 
                                    for (int l = j; l < j + q; l++)
                                    {
                                        if (k < 640 && k >= 0 && l < 480 && l >= 0 && zmap[k, l] > z)
                                        {
                                            zmap[k, l] = z;
                                            if (mode == 4 && t >= 0 && K >= 0 && t < vladFace.Width && K < vladFace.Height)
                                            {
                                                var clr = vladFace.GetPixel(t, K);
                                                colormap.SetPixel(k, l, clr);//Color.FromArgb(Math.Min(255, (int)(clr.R * 3 / z)), Math.Min(255, (int)(clr.G * 3 / z)), Math.Min(255, (int)(clr.B * 3 / z))));
                                            }
                                            else
                                                colormap.SetPixel(k, l, Color.FromArgb(Math.Min(255, (int)(color.R * 3 / z)), Math.Min(255, (int)(color.G * 3 / z)), Math.Min(255, (int)(color.B * 3 / z))));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            public bool CheckIntersection(Vector3 p, Vector3 Point, Plane C)
            {
                var V = Point - p;
                
                var t = -(p.M1 * C.Matrix)[0, 0] / (V.M0 * C.Matrix)[0, 0];
                
                var cross = p + V * (float)t;
                if (t < 0 || t > 1) return false;
                var V1 = C.Vertexes[1] - C.Vertexes[0];
                var V2 = C.Vertexes[2] - C.Vertexes[0];
                var V3 = V1 ^ V2;
                var newBase = new Matrix(3, 3, new[,] { { V1.X, V1.Y, V1.Z }, { V2.X, V2.Y, V2.Z }, { V3.X, V3.Y, V3.Z } }).A_1();
                var newCross = Vector3.Load(cross.M * newBase);
                PointF[] points = new PointF[C.Vertexes.Length];
                //Console.WriteLine("Point:" + newCross.X + " " + newCross.Y);
                for (int i = 0; i < C.Vertexes.Length; i++)
                {
                    var np = Vector3.Load(C.Vertexes[i].M * newBase);
                    points[i] = new PointF(np.X, np.Y);
                   // Console.WriteLine(np.X + " " + np.Y);
                }
                //Console.WriteLine(inside(newCross.X, newCross.Y, points));
                return inside(newCross.X, newCross.Y, points);
            }
            public void SuperDrawPlanez(PaintEventArgs e, float ox, float oy,Plane[] planes, Plane plane, Color color, int mode, int scale,Vector3 Light)
            {
                var Polygon = new List<PointF>();
                for (int i = 0; i < plane.Vertexes.Length; i++)
                {
                    int j = (i == plane.Vertexes.Length - 1) ? 0 : i + 1;
                    var a = plane.Vertexes[i];
                    var b = plane.Vertexes[j];
                    var n = FixLine(a, b);
                    if (n != null)
                    {
                        Polygon.Add(OnCamera(n[0], ox, oy));
                        Polygon.Add(OnCamera(n[1], ox, oy));
                    }
                }
                if (Polygon.Count < 2) return;
                var pol = Polygon.ToArray();
                var get = MinsMaxs(pol);
                var Matrix = plane.Matrix;
                var newP = PointS.M * Base.A_1();
                var newN = N.M * Base.A_1();
                {
                    int q = scale;
                    var a = newN[0, 0];
                    var b = newN[0, 1];
                    var c = newN[0, 2];
                    var d = -newP[0, 0] * a - newP[0, 1] * b - newP[0, 2] * c;
                    get[0].X = Math.Max(0, get[0].X);
                    get[1].X = Math.Min(640, get[1].X);
                    get[0].Y = Math.Max(0, get[0].Y);
                    get[1].Y = Math.Min(480, get[1].Y);

                    Vector3 to_center = null;
                    Vector3 to_center_image = null;
                    Vector3 from_center = null;
                    Vector3 from_center_image = null;
                    Vector3 N_center = null;
                    Vector3 N_center_image = null;
                    Matrix newBase = null;
                    Matrix newBase_image = null;
                    Vector3 center = null;
                    if (mode == 4)
                    {
                        to_center = plane.Center - plane.Vertexes[0];
                        to_center_image = new Vector3(vladFace.Width / 2, vladFace.Height / 2, 0);
                        from_center = plane.Vertexes[1] - plane.Vertexes[0];
                        from_center_image = new Vector3(vladFace.Width, 0, 0);
                        N_center = to_center ^ from_center;
                        N_center_image = to_center_image ^ from_center_image;
                        newBase = new Matrix(3, 3, new[,] { { to_center.X, to_center.Y, to_center.Z }, { from_center.X, from_center.Y, from_center.Z }, { N_center.X, N_center.Y, N_center.Z } });
                        newBase_image = new Matrix(3, 3, new[,] { { to_center_image.X, to_center_image.Y, to_center_image.Z }, { from_center_image.X, from_center_image.Y, from_center_image.Z }, { N_center_image.X, N_center_image.Y, N_center_image.Z } });
                        center = Vector3.Load((plane.Center.M * newBase.A_1()) * newBase_image);
                    }

                    for (int i = get[0].Y - 1; i <= get[1].Y; i += q)
                    {
                        if (i < 480 && i >= 0) {
                            int[] line = (mode == 6 || mode == 5 || mode == 4) ? ScanLineY(i, pol) : new int[2] { get[0].X, get[1].X };
                           
                            for (int n = 0; n < line.Length - 1; n++)
                            {
                                if (line[n] < 0) line[n] = 0;
                                if (line[n+1] < 0) continue;
                                if (line[n] > 640) continue;
                                if (line[n + 1] > 640) line[n + 1] = 640;
                                if (line[n + 1] - line[n] <= 0 || line[n+1]<line[n]) continue;
                                //Console.WriteLine(line[n + 1] - line[n]);
                                var left = line[n];
                                var right = line[n + 1];
                                var z_ozl = -(a * (left - ox) + b * (i - oy) + d) / c;
                                var z_ozr = -(a * (right - ox) + b * (i - oy) + d) / c;
                                var pL = Vector3.Load(new Matrix(1, 3, new[,] { { left - ox, i - oy, z_ozl } }) * Base);
                                var pr = Vector3.Load(new Matrix(1, 3, new[,] { { right - ox, i - oy, z_ozr } }) * Base);
                                var Vl = Point - pL;
                                var Vr = Point - pr;
                                var tmpl = -(pL.M1 * Matrix)[0, 0] / (Vl.M0 * Matrix)[0, 0];
                                var tmpr = -(pr.M1 * Matrix)[0, 0] / (Vr.M0 * Matrix)[0, 0];
                                var cross = pL + Vl * (float)tmpl;
                                var crossr = pr + Vr * (float)tmpr;
                                //drawLine(e, ox, oy, cross, crossr, Pens.Yellow);
                                
                                var plus = (crossr - cross) * (1.0f / (line[n + 1] - line[n]));
                                cross -= plus;
                                for (int j = line[n] - 1; j < line[n + 1] + 1; j += q)
                                {
                                    cross += plus;
                                    if (j < 640 && j >= 0 && (mode == 6 && j > line[n] && j < line[n + 1] - 1 || inside(j, i, pol)))
                                    {
                                       
                                        var z = (float)(Point - cross).Length;
                                        if (zmap[j, i] < z)
                                            continue;
                                        Vector3 crossTex;
                                        int t = 0, K = 0;
                                        if (mode == 4)
                                        {
                                            crossTex = Vector3.Load((cross - plane.Vertexes[0]).M * newBase.A_1() * newBase_image);
                                            t = (int)crossTex.X;
                                            K = (int)crossTex.Y;
                                        }
                                        for (int k = i; k < i + q; k++)
                                        {
                                            for (int l = j; l < j + q; l++)
                                            {
                                                if (k < 480 && k >= 0 && l < 640 && l >= 0 && zmap[l, k] > z)
                                                {
                                                    zmap[l, k] = z;
                                                    if (mode == 4 && t >= 0 && K >= 0 && t < vladFace.Width && K < vladFace.Height)
                                                    {
                                                        var clr = vladFace.GetPixel(t, K);
                                                        colormap.SetPixel(l, k, clr);
                                                    }
                                                    else
                                                    {
                                                        var lig = 4 / z;
                                                        if (mode == 5)
                                                        {
                                                            var den = (Light - cross).Length;
                                                            lig = (float)(20 / (den * den + den + 1));
                                                            foreach (var pl in planes)
                                                            {
                                                                if (pl != plane)
                                                                {
                                                                    if (CheckIntersection(cross, Light, pl))
                                                                    {
                                                                        lig /= 5;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        colormap.SetPixel(l, k, Color.FromArgb(Math.Min(255, (int)(color.R * lig)), Math.Min(255, (int)(color.G * lig)), Math.Min(255, (int)(color.B * lig))));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                
                            }
                        }
                    }
                }
            }
            public void SuperDrawPlanez_s(PaintEventArgs e, float ox, float oy, Plane[] planes, Plane plane, Color color, int mode, int scale, Vector3 Light)
            {
                var Polygon = new List<PointF>();
                for (int i = 0; i < plane.Vertexes.Length; i++)
                {
                    int j = (i == plane.Vertexes.Length - 1) ? 0 : i + 1;
                    var a = plane.Vertexes[i];
                    var b = plane.Vertexes[j];
                    var n = FixLine(a, b);
                    if (n != null)
                    {
                        Polygon.Add(OnCamera(n[0], ox, oy));
                        Polygon.Add(OnCamera(n[1], ox, oy));
                    }
                }
                if (Polygon.Count < 2) return;
                var pol = Polygon.ToArray();
                var get = MinsMaxs(pol);
                var Matrix = new Matrix(4, 1, new[,] { { plane.abcd[0] }, { plane.abcd[1] }, { plane.abcd[2] }, { plane.abcd[3] } });
                var newP = PointS.M * Base.A_1();
                var newN = N.M * Base.A_1();
                {
                    int q = scale;
                    var a = newN[0, 0];
                    var b = newN[0, 1];
                    var c = newN[0, 2];
                    var d = -newP[0, 0] * a - newP[0, 1] * b - newP[0, 2] * c;
                    get[0].X = Math.Max(0, get[0].X);
                    get[1].X = Math.Min(640, get[1].X);
                    get[0].Y = Math.Max(0, get[0].Y);
                    get[1].Y = Math.Min(480, get[1].Y);

                    Vector3 to_center = null;
                    Vector3 to_center_image = null;
                    Vector3 from_center = null;
                    Vector3 from_center_image = null;
                    Vector3 N_center = null;
                    Vector3 N_center_image = null;
                    Matrix newBase = null;
                    Matrix newBase_image = null;
                    Vector3 center = null;
                    if (mode == 4)
                    {
                        to_center = plane.Center - plane.Vertexes[0];
                        to_center_image = new Vector3(vladFace.Width / 2, vladFace.Height / 2, 0);
                        from_center = plane.Vertexes[1] - plane.Vertexes[0];
                        from_center_image = new Vector3(vladFace.Width, 0, 0);
                        N_center = to_center ^ from_center;
                        N_center_image = to_center_image ^ from_center_image;
                        newBase = new Matrix(3, 3, new[,] { { to_center.X, to_center.Y, to_center.Z }, { from_center.X, from_center.Y, from_center.Z }, { N_center.X, N_center.Y, N_center.Z } });
                        newBase_image = new Matrix(3, 3, new[,] { { to_center_image.X, to_center_image.Y, to_center_image.Z }, { from_center_image.X, from_center_image.Y, from_center_image.Z }, { N_center_image.X, N_center_image.Y, N_center_image.Z } });
                        center = Vector3.Load((plane.Center.M * newBase.A_1()) * newBase_image);
                    }

                    for (int i = get[0].Y - 1; i <= get[1].Y; i += q)
                    {
                        int[] line = (mode == 6 || mode == 5 || mode == 4) ? ScanLineY(i, pol) : new int[2] { get[0].X, get[1].X };
                        for (int n = 0; n < line.Length - 1; n++)
                        {
                            for (int j = line[n] - 1; j < line[n + 1] + 1; j += q)
                            {
                                if (i < 480 && i >= 0 && j < 640 && j >= 0 && (mode == 6 && j > line[n] && j < line[n + 1] - 1 || inside(j, i, pol)))
                                {

                                    var z_oz = -(a * (j - ox) + b * (i - oy) + d) / c;
                                    var p = Vector3.Load(new Matrix(1, 3, new[,] { { j - ox, i - oy, z_oz } }) * Base);
                                    var V = Point - p;
                                    var tmp = -(p.M1 * Matrix)[0, 0] / (V.M0 * Matrix)[0, 0];
                                    var cross = p + V * (float)tmp;
                                    var z = (float)(Point - cross).Length;
                                    if (zmap[j, i] < z) continue;
                                    Vector3 crossTex;
                                    int t = 0, K = 0;
                                    if (mode == 4)
                                    {
                                        crossTex = Vector3.Load((cross - plane.Vertexes[0]).M * newBase.A_1() * newBase_image);
                                        t = (int)crossTex.X;
                                        K = (int)crossTex.Y;
                                    }
                                    for (int k = i; k < i + q; k++)
                                    {
                                        for (int l = j; l < j + q; l++)
                                        {
                                            if (k < 480 && k >= 0 && l < 640 && l >= 0 && zmap[l, k] > z)
                                            {
                                                zmap[l, k] = z;
                                                if (mode == 4 && t >= 0 && K >= 0 && t < vladFace.Width && K < vladFace.Height)
                                                {
                                                    var clr = vladFace.GetPixel(t, K);
                                                    colormap.SetPixel(l, k, clr);//Color.FromArgb(Math.Min(255, (int)(clr.R * 3 / z)), Math.Min(255, (int)(clr.G * 3 / z)), Math.Min(255, (int)(clr.B * 3 / z))));
                                                }
                                                else
                                                {
                                                    var lig = 4 / z;
                                                    if (mode == 5)
                                                    {
                                                        var den = (Light - cross).Length;
                                                        lig = (float)(20 / (den * den + den + 1));
                                                        foreach (var pl in planes)
                                                        {
                                                            if (pl != plane)
                                                            {
                                                                if (CheckIntersection(cross, Light, pl))
                                                                {
                                                                    lig /= 5;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    colormap.SetPixel(l, k, Color.FromArgb(Math.Min(255, (int)(color.R * lig)), Math.Min(255, (int)(color.G * lig)), Math.Min(255, (int)(color.B * lig))));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            public void SuperPuperDrawer(PaintEventArgs e, float ox, float oy, Plane[][] plane_s,Color[] colors,int mode,int scale, Vector3 Light)
            {
                List<Plane> planes2 = new List<Plane>();
                List<Plane[]> planes = new List<Plane[]>();
                List<float> ds = new List<float>();
                foreach (var figure in plane_s)
                {
                    var ps = new Vector3();
                    var l = 0;
                    foreach (var plane in figure)
                    {
                        foreach (var p in plane.Vertexes)
                        {
                            ps = ps + p;
                            l++;
                        }
                    }
                    ps = ps * (1.0f / l);
                    planes.Add(figure);
                    ds.Add((float)(Point-ps).Length);
                }
                for (int i = 0; i < ds.Count; i++)
                {
                    for (int j = i; j < ds.Count; j++)
                    {
                        if (ds[i] < ds[j])
                        {
                            Swap(ds, j, i);
                            Swap(planes, j, i);
                            Swap(colors, j, i);
                        }
                    }
                }
                List<Color> Colors = new List<Color>();
                for (int i = 0; i < planes.Count; i++)
                {
                    var p = planes[i];
                    foreach (var pl in p)
                    {
                        planes2.Add(pl);
                        Colors.Add(colors[i]);
                    }
                }
                if (mode == 1)
                {
                    SuperDrawPlanes(e, ox, oy, planes2.ToArray());
                    return;
                }
                if (mode == 2 || mode>3)
                {
                    for (int i = 0; i < 640; ++i)
                    {
                        for (int j = 0; j < 480; ++j)
                        {
                            zmap[i, j] = float.MaxValue;
                            colormap.SetPixel(i, j, Color.DarkBlue);
                        }
                    }
                }
                BackFaceCulling(e, ox, oy, planes2.ToArray(), Colors.ToArray(),mode,scale, Light);
                if (mode==2 || mode>3) e.Graphics.DrawImage(colormap.Bitmap, 0, 0);
            }
            public void BackFaceCulling(PaintEventArgs e, float ox, float oy, Plane[] planes,Color[] colors,int mode,int scale,Vector3 Light)
            {
                if (mode == 2 || mode>3)
                {
                    for (int i = 0; i < planes.Length; i++)
                    {
                        var plane = planes[i];
                        if ((Point.M1 * plane.Matrix)[0, 0] < 0) continue;
                        SuperDrawPlanez(e, ox, oy, planes, plane, colors[i], mode, scale, Light);
                    }
                }
                else
                {
                    for (int i = 0; i < planes.Length; i++)
                    {
                        var plane = planes[i];
                        if ((Point.M1 * plane.Matrix)[0, 0] < 0) continue;
                        SuperDrawPlane(e, ox, oy, plane);
                    }
                }
            }
            public Vector3 CrossLineWithSegm(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
            {
                if (A == null || B == null || C == null || D == null) return null;
                var V = (B - A).Normalize();
                var I = (C - D).Normalize();
                var N = I ^ V;
                if (N.X == 0 && N.Y == 0 && N.Z == 0)
                    return null;
                else
                {
                    N = N.Normalize();
                    var takeBase = new Matrix(3, 3, new[,] { { V.X, V.Y, V.Z }, { I.X, I.Y, I.Z }, { N.X, N.Y, N.Z } }).A_1();
                    var a = Vector3.Load(A.M * takeBase);
                    var b = Vector3.Load(B.M * takeBase);
                    var c = Vector3.Load(C.M * takeBase);
                    var d = Vector3.Load(D.M * takeBase);
                    var v = b - a;
                    var M = new Matrix(2, 2, new[,] { { v.X, v.Y }, { c.X - d.X, c.Y - d.Y } });
                    var defs = new Matrix(1, 2, new[,] { { c.X - a.X, c.Y - a.Y } }) * M.A_1();
                    if (defs[0, 1] < 0 || defs[0, 1] > 1)
                    {
                        return null;
                    }
                    Console.WriteLine(a.X + ":" + a.Y + " " + b.X + ":" + b.Y);
                    var p = (a + v * (float)defs[0, 0]).M * takeBase.A_1();
                    return new Vector3((float)p[0,0], (float)p[0,1], (float)p[0,2]);
                }
            }
            public Plane[] Cut(Plane A, Plane B, PaintEventArgs e, float ox, float oy)
            {
                if (A == null || B == null) return null;
                var planes = new List<Plane>();
                var N1 = new Vector3(A.abcd[0], A.abcd[1], A.abcd[2]);
                var N2 = new Vector3(B.abcd[0], B.abcd[1], B.abcd[2]);
                var N3 = N1 ^ N2;
                if (N3.X == 0 && N3.Y == 0 && N3.Z ==0) 
                    return new[] { A, B };
                else
                {
                    var p = ((A.abcd[3] * N1 - B.abcd[3] * N2) ^ N3) *(float)(1/(N3.Length*N3.Length));
                    
                    for (int i=0;i<B.Vertexes.Length;i++)
                    {
                        
                        int j = (i == B.Vertexes.Length - 1) ? 0 : (i + 1);
                        var c = B.Vertexes[i];
                        var d = B.Vertexes[j];
                        var ps = CrossLineWithSegm(p, p + N3, c, d);
                        if (ps != null)
                        {
                            //var f = OnCamera(ps);
                            //e.Graphics.DrawEllipse(Pens.Black, ox+ f.X - 5, oy+f.Y - 5, 10, 10);
                        }
                        drawLine(e, ox, oy, p, p + N3.Normalize(), Pens.Green);
                    }
                }
                return null;
            }
            public void UpdateMAX() => Matrix = new Matrix(4, 1, new[,] { { N.X }, { N.Y }, { N.Z }, { D } });
            public void UpdateBASE() => Base = new Matrix(3, 3, new[,] { { k.X, k.Y, k.Z }, { i.X, i.Y, i.Z }, { j.X, j.Y, j.Z } });
            public void Rot(float rad)
            {
                var rotZ = new Matrix(3, 3, new[,] { { Math.Cos(rad), -Math.Sin(rad), 0 }, { Math.Sin(rad), Math.Cos(rad), 0 }, { 0, 0, 1 } });
                N = Vector3.Load(N.M * rotZ);
                N2 = Vector3.Load(N2.M * rotZ);
                Point = Vector3.Load(Point.M * rotZ);
                i = Vector3.Load(i.M * rotZ);
                j = Vector3.Load(j.M * rotZ);
                k = Vector3.Load(k.M * rotZ);
                PointS = Point + N;
                PointS2 = Point + N2;
                UpdateMAX();
                UpdateBASE();
            }
            public void Rot1(float rad)
            {
                var tmp = Vector3.RotateAll(new[] { N, N2, i, j, k }, new Vector3(0, 0, 1), rad);
                N = tmp[0];
                N2 = tmp[1];
                i = tmp[2];
                j = tmp[3];
                k = tmp[4];
                UpdateMAX();
                UpdateBASE();
                PointS = Point + N;
                PointS2 = Point + N2;
            }
            public void Rot2(float rad)
            {
                var tmp = Vector3.RotateAll(new[] {N, N2, i, j, k }, k, rad);
                N = tmp[0];
                N2 = tmp[1];
                i = tmp[2];
                j = tmp[3];
                k = tmp[4];
                UpdateMAX();
                UpdateBASE();
                PointS = Point + N;
                PointS2 = Point + N2;
            }
        }
        bool EndGame = false;
        Camera Cam = new Camera(new Vector3(0, 4, 0), 0, 256, 0);
        Plane[] Qube = null;
        Plane[] Qube2 = null;
        Plane[] Qube3 = null;
        Plane[] Qube4 = null;
        Plane[] Qube5 = null;
        Plane[] Qube6 = null;
        Plane[] Plane1 = null;
        String[] Modes =
        {
            "",
            "Алгоритм художника",
            "ZBuffer",
            "Алгоритм BackFace Culling",
            "ZBuffer + Текстуры",
            "ZBuffer + Свет",
            "Улучшенный ZBuffer"
        };
        public Form1()
        {
            InitializeComponent();
            Qube = CreateCube(new Vector3(0, 0, 2), 0.5f);
            Qube2 = CreateCube(new Vector3(0, 0, 4), 1);
            Qube3 = CreateCube(new Vector3(0, 0, 6), 1);
            Qube4 = CreateCube(new Vector3(2, 0, 2), 1);
            Qube6 = CreateCube(new Vector3(0, 2, 2), 1);
            //Plane1 = new Plane[] { new Plane(new Vector3[] { new Vector3(5, -5, 0), new Vector3(5, 5, 0), new Vector3(-5, 5, 0), new Vector3(-5, -5, 0) } ) };
        }
        int scale = 1;
        private void screenBox_Paint(object sender, PaintEventArgs e)
        {
            if (!EndGame)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var LinesA = new List<Vector3>();
                var LinesB = new List<Vector3>();
                var Colors = new List<Pen>();
                int count = 5;
                for (int i = -count; i < count + 1; i++)
                {
                    Colors.Add(Pens.LightGray);
                    Colors.Add(Pens.LightGray);
                    LinesA.Add(new Vector3(-count, i, 0));
                    LinesA.Add(new Vector3(i, -count, 0));
                    LinesB.Add(new Vector3(count, i, 0));
                    LinesB.Add(new Vector3(i, count, 0));
                }

                Colors.AddRange(new[] { Pens.Green, Pens.Red, Pens.Blue });
                LinesA.AddRange(new[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) });
                LinesB.AddRange(new[] { new Vector3(0, 0, 3), new Vector3(0, 3, 0), new Vector3(3, 0, 0) });
                Cam.drawLines(e, e.ClipRectangle.Width / 2, e.ClipRectangle.Height / 2, LinesA.ToArray(), LinesB.ToArray(), Colors.ToArray());
                Cam.SuperPuperDrawer(e, screenBox.Width / 2, screenBox.Height / 2, new[] { Qube,Qube3,Qube4,Qube6 },new[] { Color.Green, Color.Red, Color.Blue, Color.Purple },move,scale,Light);
                //Cam.SuperPuperDrawer(e, screenBox.Width / 2, screenBox.Height / 2, new[] { Qube2,Qube}, new[] { Color.Green,Color.Red}, move,scale,Light);
                e.Graphics.DrawString("FOV: " + ((int)Cam.getFov(-e.ClipRectangle.Width / 2, e.ClipRectangle.Width / 2)).ToString(), new Font("Arial", 10f), Brushes.Black, 0, 20);
                e.Graphics.DrawString("Mode: " + Modes[move], new Font("Arial", 10f), Brushes.Black, 0, 0);
                e.Graphics.FillRectangle(Brushes.Black, 0, e.ClipRectangle.Height - 20, e.ClipRectangle.Width, 20);
                e.Graphics.DrawString("Лабораторная работа Хабирова Владислава Ильдаровича", new Font("Arial", 10f), Brushes.White, 5, e.ClipRectangle.Height - 18);
                var bulb = Cam.OnCamera(Light, screenBox.Width / 2, screenBox.Height / 2);
                var Size = 10/(float)(Cam.Point-Light).Length;
                e.Graphics.DrawEllipse(Pens.Black,bulb.X-2*Size,bulb.Y-2*Size,Size*4,Size*4);
                e.Graphics.FillEllipse(Brushes.White, bulb.X - 2 * Size, bulb.Y - 2 * Size, Size * 4, Size * 4);
            }
            else
            {
                e.Graphics.DrawString("ВВ", new Font("Arial", 10f), Brushes.Black, 0, 0);
            }
        }
        bool CheckIn(Vector3 dot, Plane[] fig)
        {
            foreach(var p in fig)
            {
                //Console.WriteLine(Cam.sgn((float)(dot.M1 * new Matrix(4, 1, new[,] { { abcd[0] }, { abcd[0] }, { abcd[1] }, { 1 } }))[0, 0]));
                if ((dot.M1 * new Matrix(4, 1, new[,] { { p.abcd[0] }, { p.abcd[1] }, { p.abcd[2] }, { p.abcd[3] } }))[0, 0] > 0)
                    return false;
            }
            return true;
        }
        Point old = new Point(0, 0);
        int move = 1;
        Vector3 Light = new Vector3(0, 0, 0);
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            float speed =6;
            if (e.KeyCode == Keys.Down) Light += new Vector3(0, 0, -0.05f);
            if (e.KeyCode == Keys.Up) Light += new Vector3(0, 0, 0.05f);
            if (e.KeyCode == Keys.Left) Light += new Vector3(-1, 0, 0);
            if (e.KeyCode == Keys.Right) Light += new Vector3(1, 0, 0);
            if (e.KeyCode == Keys.A)     Cam.Move(Cam.k.Normalize() * 0.05f * speed);
            if (e.KeyCode == Keys.D)     Cam.Move(Cam.k.Normalize() * -0.05f * speed);
            if (e.KeyCode == Keys.W)     Cam.Move(Cam.N.Normalize() * -0.05f * speed);
            if (e.KeyCode == Keys.S)     Cam.Move(Cam.N.Normalize() * 0.05f * speed);
            if (e.KeyCode == Keys.Q)     Cam.Rot((float)Math.PI / 42);
            if (e.KeyCode == Keys.E)     Cam.Rot(-(float)Math.PI / 42);
            if (e.KeyCode == Keys.Z)     Cam.Angle(Cam.N.Normalize() * -2f);
            if (e.KeyCode == Keys.X)     Cam.Angle(Cam.N.Normalize() * 2f);
            if (e.KeyCode == Keys.D1) move = 1;
            if (e.KeyCode == Keys.D2) move = 2;
            if (e.KeyCode == Keys.D3) move = 3;
            if (e.KeyCode == Keys.D4) move = 4;
            if (e.KeyCode == Keys.D5) move = 5;
            if (e.KeyCode == Keys.D6) move = 6;
            if (e.KeyCode == Keys.D7) scale++;
            if (e.KeyCode == Keys.D8) scale--;
            if (scale < 1) scale = 1;
            screenBox.Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //timer1.Stop();
            //timer1.Start();
            //screenBox.Refresh();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Hide();
            //if (CheckIn(Cam.Point, Qube2))
              // EndGame = true;
            if (Math.Abs(e.X - Width / 2) > 0)   Cam.Rot1(-(e.X - Width / 2) / 300.0f);
            if (Math.Abs(e.Y - Height / 2) > 0)  Cam.Rot2((e.Y - Height / 2) / 300.0f);
            Cursor.Position = new Point(Location.X + Width / 2 + 9, Location.Y + Height / 2 + 32);
            screenBox.Refresh();
        }
    }
}
