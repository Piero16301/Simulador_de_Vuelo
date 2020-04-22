using System;
using System.Collections.Generic;
using BuildRCities;
using JSpace;
using UnityEngine;

namespace BuildRCitiesExamples {
    [Serializable]
    public class CarcassonneRoadNetworkPoint
    {
        public Vector2Fixed position;
        public List<CarcassonneRoadNetworkPoint> link = new  List<CarcassonneRoadNetworkPoint>();
        // public List<CarcassonneRoad> roads = new List<CarcassonneRoad>();
        public List<Vector2> shrunkPoints = new List<Vector2>();

        public bool generate = false;

        public RawMeshData GenerateRoad() {

            int pointSize = shrunkPoints.Count;
            int[] sortedIndexes = BuildrUtils.SortPointByAngle(shrunkPoints.ToArray(), position.vector3XZ);
            Vector2[] sortedPoints = new Vector2[pointSize];
            for(int i = 0; i < pointSize; i++) {
                sortedPoints[i] = shrunkPoints[sortedIndexes[i]];
            }
            bool clockwise = Clockwise(sortedPoints);

            int vertCount = pointSize + 1;
            int triCount = pointSize * 3;

            RawMeshData output = new RawMeshData(vertCount, triCount);
            for(int v = 0; v < vertCount - 1; v++) {
                output.vertices[v] = new Vector3(sortedPoints[v].x, 0, sortedPoints[v].y);
            }
            output.vertices[vertCount - 1] = position.vector3XZ;

            int vertIndex = 0;
            for(int t = 0; t < triCount; t+=3) {
                output.submeshes[0].triangles[t + 0] = vertCount - 1;
                if(clockwise) {
                    output.submeshes[0].triangles[t + 1] = vertIndex;
                    output.submeshes[0].triangles[t + 2] = (vertIndex < vertCount - 2) ? vertIndex + 1 : 0;
                }
                else {
                    output.submeshes[0].triangles[t + 1] = (vertIndex < vertCount - 2) ? vertIndex + 1 : 0;
                    output.submeshes[0].triangles[t + 2] = vertIndex;
                }
//                Debug.Log((triCount - 1)+" "+ (vertIndex)+" "+ (vertIndex+1));
                vertIndex++;
            }

            return output;
        }

        private static bool Clockwise(Vector2[] points) {
            int pointCount = points.Length;
            float value = 0;
            for (int p = 0; p < pointCount; p++) {
                Vector2 p0 = points[p];
                int pb = p + 1;
                if (pb == pointCount) pb = 0;
                Vector2 p1 = points[pb];
                value += (p1.x - p0.x) * (p1.y + p0.y);//(x2 − x1)(y2 + y1)
            }
            return value > 0;
        }
    }
}