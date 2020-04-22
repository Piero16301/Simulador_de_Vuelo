using System;
using BuildRCities;
using UnityEngine;
using JSpace;

namespace BuildRCitiesExamples {
    [Serializable]
    public class CarcassonneRoad
    {
        public Vector2Fixed pointA;
        public Vector2Fixed pointB;
        public Vector2Fixed position;

		// public CarcassonneQuarter quarterA;
		// public CarcassonneQuarter quarterB;

        public bool generate = false;

		// public RawMeshData GenerateRoad()
		// {
		// 	if(quarterA == null && quarterB == null)
		// 	{
		// 		Debug.Log("no quarter given!");
		// 		return null;
		// 	}
  //
		// 	Vector2Fixed[] roadPoints = new Vector2Fixed[4];
		// 	Vector2Fixed center = Vector2Fixed.Lerp(pointA, pointB, 0.5f);
  //
		// 	if(quarterA != null
		// 	){
		// 		int roadPointIndexA = quarterA.points.IndexOf(pointA);
		// 		int roadPointIndexB = quarterA.points.IndexOf(pointB);
  //
		// 		if(roadPointIndexA == -1 || roadPointIndexB == -1)
		// 			return null;
  //
		// 		roadPoints[0] = new Vector2Fixed(quarterA.shrunkPoints[roadPointIndexA]);
		// 		roadPoints[1] = new Vector2Fixed(quarterA.shrunkPoints[roadPointIndexB]);
		// 	}else
		// 	{
		// 		roadPoints[0] = pointA;
		// 		roadPoints[1] = pointB;
		// 	}
  //
		// 	if(quarterB != null
		// 	){
		// 		int roadPointIndexA = quarterB.points.IndexOf(pointA);
		// 		int roadPointIndexB = quarterB.points.IndexOf(pointB);
  //
		// 		if(roadPointIndexA == -1 || roadPointIndexB == -1)
		// 			return null;
  //
		// 		roadPoints[2] = new Vector2Fixed(quarterB.shrunkPoints[roadPointIndexA]);
		// 		roadPoints[3] = new Vector2Fixed(quarterB.shrunkPoints[roadPointIndexB]);
		// 	}else
		// 	{
		// 		roadPoints[2] = pointA;
		// 		roadPoints[3] = pointB;
		// 	}
  //
		//     Vector3 dirA = (roadPoints[1] - roadPoints[0]).vector3XZ.normalized;
  //           Vector3 dirB = (roadPoints[1] - roadPoints[2]).vector3XZ.normalized;
  //
  //           Vector3 crsA = Vector3.Cross(dirA, Vector3.up);
		//     float dot = Vector3.Dot(dirB, crsA);
  //
  //           Vector2Fixed[] sortedRoadPoints = new Vector2Fixed[4];
		//     if(dot > 0) {
		//         sortedRoadPoints[0] = roadPoints[0];
		//         sortedRoadPoints[1] = roadPoints[1];
		//         sortedRoadPoints[2] = roadPoints[3];
		//         sortedRoadPoints[3] = roadPoints[2];
		//     }
		//     else {
		//         sortedRoadPoints[0] = roadPoints[2];
		//         sortedRoadPoints[1] = roadPoints[3];
		//         sortedRoadPoints[2] = roadPoints[1];
		//         sortedRoadPoints[3] = roadPoints[0];
  //           }
  //
  //           position = center;
  //           RawMeshData output = new RawMeshData(4, 6);
  //
		//     output.vertices[0] = (sortedRoadPoints[0]).vector3XZ;
		//     output.vertices[1] = (sortedRoadPoints[1]).vector3XZ;
		//     output.vertices[2] = (sortedRoadPoints[2]).vector3XZ;
		//     output.vertices[3] = (sortedRoadPoints[3]).vector3XZ;
  //           
		//     output.submeshes[0].triangles[0] = 0;
		//     output.submeshes[0].triangles[1] = 1;
		//     output.submeshes[0].triangles[2] = 3;
		//     output.submeshes[0].triangles[3] = 3;
		//     output.submeshes[0].triangles[4] = 1;
		//     output.submeshes[0].triangles[5] = 2;
  //
		//     return output;
		// }
    }
}