using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2Лаба
{
    public enum MatrixPresets
    {
        IdentityMatrix = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Matrix
    {
        public int width { get; private set; } = 0;
        public int height { get; private set; } = 0;
        private double[,] m { get; set; } = null;
        private void init(int rows, int columns)
        {
            width = columns;
            height = rows;
            m = new double[height, width];
        }
        public Matrix(int rows, int columns)
        {
            init(rows, columns);
        }
        public Matrix(int rows, int columns, double[,] nums)
        {
            init(rows, columns);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    m[i, j] = nums[i, j];
        }
        public Matrix(int rows, int columns, float[,] nums)
        {
            init(rows, columns);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    m[i, j] = nums[i, j];
        }
        public Matrix(int rows, int columns, int[,] nums)
        {
            init(rows, columns);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    m[i, j] = nums[i, j];
        }
        public Matrix(int rows, int columns, MatrixPresets preset)
        {
            init(rows, columns);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    switch (preset)
                    {
                        case MatrixPresets.IdentityMatrix:
                            if (i == j) m[i, j] = 1;
                            else m[i, j] = 0;
                            break;
                        default:
                            break;
                    }
        }
        public void _swapRows(int r1, int r2)
        {
            for (int i = 0; i < width; i++)
            {
                double tmp = m[r1, i];
                m[r1, i] = m[r2, i];
                m[r2, i] = tmp;
            }
        }
        public void _addRow(double a, int row)
        {
            for (int i = 0; i < width; i++)
                m[row, i] += a;
        }
        public void _divRow(double a, int row)
        {
            for (int i = 0; i < width; i++)
                m[row, i] /= a;
        }
        public void _sumRow(int r1, int r2, double a)
        {
            for (int i = 0; i < width; i++)
                m[r1, i] += a * m[r2, i];
        }
        public Matrix copy()
        {
            Matrix tmp = new Matrix(height, width);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    tmp.m[i, j] = m[i, j];
            return tmp;
        }
        public bool detA(out double det)
        {
            det = 0;
            if (width == 1 && height == 1)
            {
                det = m[0, 0];
                return true;
            }
            if (width == 2 && height == 2)
            {
                det = m[1, 1] * m[0, 0] - m[0, 1] * m[1, 0];
                return true;
            }
            if (width == 3 && height == 3)
            {
                det = m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[0, 1] * m[1, 0] * m[2, 2] + m[0, 1] * m[1, 2] * m[2, 0] + m[0, 2] * m[1, 0] * m[2, 1] - m[0, 2] * m[1, 1] * m[2, 0];
                return true;
            }
            if (width>3 && height > 3)
            {
                A_1(out det);
                return true;
            }
            return false;
        }
        public double this[int i, int j]
        {
            get
            {
                return m[i, j];
            }
            set
            {
                m[i, j] = value;
            }
        }
        public override string ToString()
        {
            string finished = "";
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    finished += (m[i, j].ToString() + " ");
                if (i != height - 1)
                    finished += "\r\n";
            }
            return finished;
        }
        public Matrix A_1()
        {
            Matrix e = new Matrix(height, width, MatrixPresets.IdentityMatrix);
            Matrix t = copy();
            double DET = 1;
            for (int i = 0; i < height; i++)
            {
                if (DET == 0) return e;
                double max = 0;
                int col = -1;
                for (int j = i; j < width; j++)
                {
                    if (Math.Abs(t.m[j, i]) > Math.Abs(max))
                    {
                        max = t.m[j, i];
                        col = j;
                    }
                }
                if (max != t.m[i, i])
                {
                    t._swapRows(i, col);
                    e._swapRows(i, col);
                    DET *= -1;
                }
                if (t.m[i, i] != 1)
                {
                    DET *= t.m[i, i];
                    e._divRow(t.m[i, i], i);
                    t._divRow(t.m[i, i], i);
                }
                for (int j = i + 1; j < width; j++)
                {
                    e._sumRow(j, i, -t.m[j, i]);
                    t._sumRow(j, i, -t.m[j, i]);
                }
            }
            for (int i = height - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    e._sumRow(j, i, -t.m[j, i]);
                    t._sumRow(j, i, -t.m[j, i]);
                }
            }
            return e;
        }
        public Matrix A_1(out double det)
        {
            Matrix e = new Matrix(height, width, MatrixPresets.IdentityMatrix);
            Matrix t = copy();
            double DET = 1;
            det = 0;
            for (int i = 0; i < height; i++)
            {
                if (DET == 0) return e;
                double max = 0;
                int col = -1;
                for (int j = i; j < width; j++)
                {
                    if (Math.Abs(t.m[j, i]) > Math.Abs(max))
                    {
                        max = t.m[j, i];
                        col = j;
                    }
                }
                if (max != t.m[i, i])
                {
                    t._swapRows(i, col);
                    e._swapRows(i, col);
                    DET *= -1;
                }

                if (t.m[i, i] != 1)
                {
                    DET *= t.m[i, i];
                    e._divRow(t.m[i, i], i);
                    t._divRow(t.m[i, i], i);
                }
                for (int j = i + 1; j < width; j++)
                {
                    e._sumRow(j, i, -t.m[j, i]);
                    t._sumRow(j, i, -t.m[j, i]);
                }
            }
            for (int i = height - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    e._sumRow(j, i, -t.m[j, i]);
                    t._sumRow(j, i, -t.m[j, i]);
                }
            }
            det = DET;
            return e;
        }
        public static Matrix operator +(Matrix a, Matrix b)
        {
            var a_c = a.copy();
            for (int i = 0; i < Math.Min(a.width, b.width); i++)
                for (int j = 0; j < Math.Min(a.height, b.height); j++)
                    a_c.m[j, i] += b.m[j, i];
            return a_c;
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            var a_c = a.copy();
            for (int i = 0; i < Math.Min(a.width, b.width); i++)
                for (int j = 0; j < Math.Min(a.height, b.height); j++)
                    a_c.m[j, i] -= b.m[j, i];
            return a_c;
        }
        public static Matrix operator *(Matrix a, double b)
        {
            var a_c = a.copy();
            for (int i = 0; i < a.width; i++)
                for (int j = 0; j < a.height; j++)
                    a_c.m[j, i] *= b;
            return a_c;
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.width != b.height)
                throw new ArgumentException("Width != Height", "Error");
            Matrix c = new Matrix(a.height, b.width);
            for (int i = 0; i < c.height; i++)
                for (int j = 0; j < c.width; j++)
                {
                    c.m[i, j] = 0;
                    for (int k = 0; k < a.width; k++)
                        c.m[i, j] += a.m[i, k] * b.m[k, j];
                }
            return c;
        }
    }
}
