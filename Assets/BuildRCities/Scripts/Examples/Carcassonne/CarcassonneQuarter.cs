using System;
using UnityEngine;
using System.Collections.Generic;
using BuildRCities;
using BuildRCities.PlotSplitter;
using BuildRCities.ShapeSplitter;

namespace BuildRCitiesExamples
{
    [Serializable]
    public class CarcassonneQuarter
    {
        public List<Vector2Fixed> points = new List<Vector2Fixed>();
        public List<Vector2> shrunkPoints = new List<Vector2>();
        public List<CarcassonnePlot> plots = new List<CarcassonnePlot>();
        public List<CarcassonneRoad> roads = new List<CarcassonneRoad>();
        public List<IPlot> iplots = new List<IPlot>();
        public Vector2Fixed center;

        public bool externalQuarter = false;

        public Vector2Fixed this[int index] {get {return points[index];}}

        public void Shrink(float amount)
        {
            int pointCount = points.Count;
            Vector2[] v2points = new Vector2[pointCount];
            for(int p = 0; p < pointCount; p++)
                v2points[p] = points[p].vector2;
            shrunkPoints.AddRange(PolyOffset.Execute(v2points, amount));
//            shrunkPoints.AddRange(QuickPolyOffset.Execute(v2points, amount));
        }

        public void CalculateCenter()
        {
            AABBox bounds = new AABBox(points[0].vector2, Vector2.zero);
            for(int p = 0; p < points.Count; p++)
                bounds.Encapsulate(points[p].vector2);
            center = new Vector2Fixed(bounds.center);
        }

        public bool Contains(Vector2Fixed pointA, Vector2Fixed pointB)
        {
            if(!points.Contains(pointA)) return false;
            if(!points.Contains(pointB)) return false;
            return true;
        }
    }

    public class CarcassonnePlot
    {
        public List<Vector2Fixed> points = new List<Vector2Fixed>();
        public List<bool> access = new List<bool>();
        public IPlot data = null;
        public float area;
        public bool hasExternalAccess;

        public void AddPoints(Vector2[] newPoints, bool[] access)
        {
            int pointCount = newPoints.Length;
            for(int p = 0; p < pointCount; p++)
            {
                points.Add(new Vector2Fixed(newPoints[p]));
                this.access.Add(access[p]);
            }
        }

        public void AddPlot(IPlot plot)
        {
            int pointCount = plot.numberOfEdges;
            for(int p = 0; p < pointCount; p++)
            {
                points.Add(new Vector2Fixed(plot[p]));
                access.Add(plot.externals[p]);
            }

            data = plot;
        }

        public void AddShape(Shape shape)
        {
            int pointCount = shape.size;
            area = JMath.PolyArea(shape.points);
            hasExternalAccess = shape.HasExternalAccess();
            for(int p = 0; p < pointCount; p++)
            {
                
                points.Add(new Vector2Fixed(shape[p]));
                access.Add(shape.externals[p]);
            }
        }
    }
}