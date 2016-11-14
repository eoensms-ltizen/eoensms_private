using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Point2D 
{
   
	    public int X;
        public int Y;       

        public Point2D()
        {
            X = 0;
            Y = 0;
        }

        public Point2D(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    
        public Point2D(Vector3 vec)
        {
            this.X = Mathf.FloorToInt(vec.x);
            this.Y = Mathf.FloorToInt(vec.y);
        }

        public Point2D(Point2D p1, Point2D p2)
        {
            this.X = p1.X + p2.X;
            this.Y = p1.Y + p2.Y;
        }

        public void Set(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vector2 GetVector(Point2D point)
        {
            return new Vector2(point.X, point.Y);
        }

        public int GetDistanceSquared(Point2D point)
        {
            int dx = this.X - point.X;
            int dy = this.Y - point.Y;
            return (dx * dx) + (dy * dy);            
        }

        public Point2D GetShortDistance(List<Point2D> points, int min)
        {
            if (points.Count == 1) return points[0];

            int index = 0;
            int distance = this.GetDistanceSquared(points[index]);
            int afterDistance;

            if (distance == min) return points[index];

            for (int i = index; i < points.Count; ++i)
            {
                afterDistance = this.GetDistanceSquared(points[i]);
                if (afterDistance < distance)
                {
                    distance = afterDistance;
                    index = i;

                    if (distance == min) return points[index];
                }
            }
            return points[index];
        }

        public override bool Equals(object obj)
        {
            if (obj is Point2D) return this.EqualsSS((Point2D)obj);
                
            throw new Exception("The method or operation is not implemented.");
        }

        public bool EqualsSS(Point2D p)
        {
            if (p == null) return false;
            return p.X == this.X && p.Y == this.Y;
        }

        public override int GetHashCode()
        {
            return (X + " " + Y).GetHashCode();
        }

        public override string ToString()
        {
            return X + ", " + Y ;
        }

        public int[] ToArray()
        {
			int[] arr = new [] {X, Y};
            return arr;
        }

        public static bool operator ==(Point2D one, Point2D two)
        {
            if (object.ReferenceEquals(one, null))
            {
                return object.ReferenceEquals(two, null);
            }
            return one.EqualsSS(two);
        }

        public static bool operator !=(Point2D one, Point2D two)
        {
            if (object.ReferenceEquals(one, null))
            {
                return !object.ReferenceEquals(two, null);
            }
            return !one.EqualsSS(two);
        }

        public static Point2D operator *(Point2D one, int d)
        {
            return new Point2D(one.X * d, one.Y * d);
        }

        public static Point2D operator *(Point2D one, float d)
        {
            return new Point2D((int)((float)one.X * d), (int)((float)one.Y * d));
        }

        public static Point2D operator +(Point2D one, Point2D two)
        {
            return new Point2D(one.X + two.X, one.Y + two.Y);
        }

        public static Point2D operator -(Point2D one, Point2D two)
        {
            return new Point2D(one.X - two.X, one.Y - two.Y);
        }

        
        public static Point2D zero = new Point2D(0, 0);
        public static Point2D one = new Point2D(1, 1); 

        
}
