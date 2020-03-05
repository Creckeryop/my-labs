using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Лаба
{
    public class Vector3
    {
        public float X, Y, Z;
        public Vector3()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3 Copy()
        {
            return new Vector3(X, Y, Z);
        }
        public Matrix M1
        {
            get => new Matrix(1, 4, new[,] { { X, Y, Z, 1 } });
        }
        public Matrix M
        {
            get => new Matrix(1, 3, new[,] { { X, Y, Z } });
        }
        public Matrix M0
        {
            get => new Matrix(1, 4, new[,] { { X, Y, Z, 0 } });
        }
        public Vector3 NewBase(Vector3 i, Vector3 j, Vector3 k)
        {
            return Load(M * new Matrix(3, 3, new float[3,3] { { i.X, i.Y, i.Z }, { j.X, j.Y, j.Z }, { k.X, k.Y, k.Z } }).A_1());
        }
        public double Length
        {
            get => Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        public Vector3 Normalize()
        {
            float L = (float)Length;
            return new Vector3(X / L, Y / L, Z / L);
        }
        public Vector3 RotZ(float rad)
        {
            Matrix A = new Matrix(3, 3, new[,] { { Math.Cos(rad), -Math.Sin(rad), 0 }, { Math.Sin(rad), Math.Cos(rad), 0 }, { 0, 0, 1 } });
            return Load(M * A);
        }
        public Vector3 RotX(float rad)
        {
            Matrix A = new Matrix(3, 3, new[,] { {1, 0, 0 }, { 0,Math.Cos(rad), -Math.Sin(rad)}, { 0, Math.Sin(rad), Math.Cos(rad) } });
            return Load(M * A);
        }
        public Vector3 RotAround(Vector3 i, float rad)
        {
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            var R = new Matrix(3, 3, new[,] {
                {cos+Math.Pow(i.X,2)*(1-cos),i.X*i.Y*(1-cos)-i.Z*sin,i.X*i.Z*(1-cos)+i.Y*sin },
                {i.X*i.Y*(1-cos)+i.Z*sin, cos + Math.Pow(i.Y,2)*(1-cos), i.Y*i.Z*(1-cos)-i.X*sin},
                {i.X*i.Z*(1-cos)-i.Y*sin, i.Z*i.Y*(1-cos)+i.X*sin, cos + Math.Pow(i.Z,2)*(1-cos)} });
            return Load(M * R);
        }
        static public Vector3[] RotateAll(Vector3[] vsin, Vector3 i, float rad)
        {
            var total = new Vector3[vsin.Length];
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            var R = new Matrix(3, 3, new[,] {
                {cos+Math.Pow(i.X,2)*(1-cos),i.X*i.Y*(1-cos)-i.Z*sin,i.X*i.Z*(1-cos)+i.Y*sin },
                {i.X*i.Y*(1-cos)+i.Z*sin, cos + Math.Pow(i.Y,2)*(1-cos), i.Y*i.Z*(1-cos)-i.X*sin},
                {i.X*i.Z*(1-cos)-i.Y*sin, i.Z*i.Y*(1-cos)+i.X*sin, cos + Math.Pow(i.Z,2)*(1-cos)} });
            for (int n = 0; n < vsin.Length; n++)
                total[n] = Load(vsin[n].M * R);
            return total;
        }
        static public Vector3 Load(Matrix a) => new Vector3((float)a[0, 0], (float)a[0, 1], (float)a[0, 2]);
        static public Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        static public Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        static public Vector3 operator *(Vector3 a, float b) => new Vector3(a.X * b, a.Y * b, a.Z * b);
        static public Vector3 operator *(float a, Vector3 b) => new Vector3(b.X * a, b.Y * a, b.Z * a);
        static public double operator *(Vector3 a, Vector3 b) => (a.X * b.X + a.Y * b.Y + a.Z * b.Z) / (a.Length * b.Length);
        static public Vector3 operator ^(Vector3 a, Vector3 b) => new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    }
}
