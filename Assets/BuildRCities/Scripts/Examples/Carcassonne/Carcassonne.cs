using UnityEngine;
using System.Collections.Generic;
using BuildRCities;
using BuildRCities.csDelaunay;
using BuildRCities.DataGenerators;
using BuildRCities.Data;
using BuildRCities.Scripts.Mesh_Generators.Task_Queue;
using BuildRCities.Scripts.Utils;
using BuildRCities.ShapeSplitter;
using JSpace;
using Settings = BuildRCities.ShapeSplitter.Settings;

namespace BuildRCitiesExamples
{
    /// <summary>
    /// 
    /// Project history
    /// 
    /// 10th December 2017
    /// Worked on getting more consitant look and feel for the city
    /// Windows now have timber frames thanks to BuildR 2 update
    /// Roads and land parcels now generate basic geometry
    ///  
    /// 18th November 2017
    /// This is an example of large scale city generation using BuildR 2.0
    /// The work is based on Watabou's marvelous Medieval Fantasy City Generator https://watabou.itch.io/medieval-fantasy-city-generator
    /// This is an ongoing project and is currently unfinished but I feel it's a good example of creating buildings at runtime
    /// It is therefore a little buggy and these will be addressed in future releases
    /// Currently we're only generating the buildings. There are no walls, roads or terrain which will follow in a later release of BuildR
    /// If you have any comments, bugs, suggestions or questions don't hesitate to ask me via
    /// 
    /// 
    /// 
    ///     Email: email@jasperstocker.com
    /// or
    ///     Twitter: @jasperstocker
    /// 
    /// Thanks
    /// Jasper
    /// </summary>
    /// 
    /// Known issues
    /// Some buildings do not generate
    /// Some roofs do not generate
    /// There are issues with how the wall is generating
    /// 
    /// TODO
    /// Generate walls
    /// Texture roads
    /// Implement church buildings
    /// Implement keep(?)
    /// Add more facades
    /// Add more window textures
    /// Proper setup mode before generate of geomtry - allow uses to define city gen
    /// Look to started a basic BuildR LOD system - generate LOD levels. Also look into merging whole quarters for distance
    /// Generate interiors
    /// Generate colliders

    public class Carcassonne : MonoBehaviour
    {
        public uint seed = 1;
        public float mapSize = 1000;
        public int voronoiCount = 45;
        public bool queueGenerateBuildings = true;

        //this is a linked point map - each point key is linked to a list of value points as defined within the Voronoi
        private Dictionary<Vector2Fixed, List<Vector2Fixed>> pointMap = new Dictionary<Vector2Fixed, List<Vector2Fixed>>();
        //Regions are defined areas by the voronoi polygons - each region will contain subplot land parcels for buildings to occupy
        private List<CarcassonneQuarter> quarters = new List<CarcassonneQuarter>();
        private Dictionary<Vector2Fixed, List<CarcassonneQuarter>> pointQuarters = new Dictionary<Vector2Fixed, List<CarcassonneQuarter>>();
        //The defined outer perimeter wall of the city
        private CarcassonneWall wall = new CarcassonneWall();
        //The road network points dictionary valued with their Vector2Fixed position
        private List<CarcassonneRoadNetworkPoint> nodes = new List<CarcassonneRoadNetworkPoint>();
        private Dictionary<Vector2Fixed, CarcassonneRoadNetworkPoint> roadNodes = new Dictionary<Vector2Fixed, CarcassonneRoadNetworkPoint>();
        //
        private List<CarcassonneRoad> roads = new List<CarcassonneRoad>();

        private RandomGenerator _rGen;
        
        public BuildingConstraintAsset constraint;
        public WallSectionAtlasAsset wallSectionAtlas;
        // private MeshGeneratorItem item = null;
        public Settings splitSettings = new Settings(1);

        private void Execute()
        {
            //First we'll create a Voroonoi
            _rGen = new RandomGenerator(seed);
            Rectf map = new Rectf(0, 0, mapSize, mapSize);
            List<Vector2f> points = new List<Vector2f>();
            for (int i = 0; i < voronoiCount; i++)
                points.Add(new Vector2f(_rGen.Range(0, mapSize), _rGen.Range(0, mapSize)));
            Voronoi v = new Voronoi(points, map);
            v.LloydRelaxation(1);

            //With the Voronoi, we will then create the city quarters and city road layout
            List<List<Vector2f>> vQuarters = v.Regions();
            int numberOfQuarters = vQuarters.Count;
            for (int r = 0; r < numberOfQuarters; r++)
            {
                List<Vector2f> vQuarter = vQuarters[r];
                int quarterSize = vQuarter.Count;

                CarcassonneQuarter newQuarter = new CarcassonneQuarter();
                quarters.Add(newQuarter);
                for (int s = 0; s < quarterSize; s++)
                {
                    Vector2Fixed vip = new Vector2Fixed(vQuarter[s].x, vQuarter[s].y);
                    newQuarter.points.Add(vip);
                    if (!pointQuarters.ContainsKey(vip))
                        pointQuarters.Add(vip, new List<CarcassonneQuarter>());
                    pointQuarters[vip].Add(newQuarter);
                    if (!newQuarter.externalQuarter && PointOnEdge(vip)) newQuarter.externalQuarter = true;//
                }
                newQuarter.CalculateCenter();
            }
            
            for (int q = 0; q < numberOfQuarters; q++)
            {
                CarcassonneQuarter quarter = quarters[q];
                int quarterSize = quarter.points.Count;
                for (int s = 0; s < quarterSize; s++)
                {
                    Vector2Fixed vip = quarter.points[s];
                    int indexB = s < quarterSize - 1 ? s + 1 : 0;
                    int indexC = s > 0 ? s - 1 : quarterSize - 1;
                    Vector2Fixed vipb = quarter.points[indexB];
                    Vector2Fixed vipc = quarter.points[indexC];

                    //Link the road points up to create the network
                    CarcassonneRoadNetworkPoint roadNode;
                    if (!roadNodes.ContainsKey(vip))
                    {
                        roadNode = new CarcassonneRoadNetworkPoint();
                        roadNode.position = vip;
                        roadNodes.Add(vip, roadNode);
                        nodes.Add(roadNode);
                    }
                    else
                    {
                        roadNode = roadNodes[vip];
                    }

                    if (roadNodes.ContainsKey(vipb))
                    {
                        CarcassonneRoadNetworkPoint roadNodeb = roadNodes[vipb];
                        if (!roadNode.link.Contains(roadNodeb))
                        {
                            roadNode.link.Add(roadNodeb);
                            roadNodeb.link.Add(roadNode);

                            CarcassonneRoad newRoad = new CarcassonneRoad();
                            CarcassonneQuarter[] connectedQuarters = GetQuarters(vip, vipb);
                            newRoad.pointA = vip;
                            newRoad.pointB = vipb;
                            // newRoad.quarterA = connectedQuarters[0];
                            // newRoad.quarterB = connectedQuarters[1];

                            if (connectedQuarters[0] != null) connectedQuarters[0].roads.Add(newRoad);
                            if (connectedQuarters[1] != null) connectedQuarters[1].roads.Add(newRoad);

                            roads.Add(newRoad);
                        }
                    }

                    if (roadNodes.ContainsKey(vipc))
                    {
                        CarcassonneRoadNetworkPoint roadNodec = roadNodes[vipc];
                        if (!roadNode.link.Contains(roadNodec))
                        {
                            roadNode.link.Add(roadNodec);
                            roadNodec.link.Add(roadNode);

                            CarcassonneQuarter[] connectedQuarters = GetQuarters(vip, vipc);
                            CarcassonneRoad newRoad = new CarcassonneRoad();
                            newRoad.pointA = vip;
                            newRoad.pointB = vipc;
                            // newRoad.quarterA = connectedQuarters[0];
                            // newRoad.quarterB = connectedQuarters[1];

                            if (connectedQuarters[0] != null) connectedQuarters[0].roads.Add(newRoad);
                            if (connectedQuarters[1] != null) connectedQuarters[1].roads.Add(newRoad);

                            roads.Add(newRoad);
                        }
                    }
                }

                quarter.Shrink(_rGen.Range(2, 4));
            }

            //Build the road network map
            //make a note of how many connections each point connects to
            Dictionary<Vector2Fixed, int> pointCount = new Dictionary<Vector2Fixed, int>();
            for (int r = 0; r < numberOfQuarters; r++)
            {
                CarcassonneQuarter quarter = quarters[r];
                int quarterSize = quarter.points.Count;
                for (int p = 0; p < quarterSize; p++)
                {
                    Vector2Fixed vip = quarter[p];
                    roadNodes[vip].shrunkPoints.Add(quarter.shrunkPoints[p]);

                    if (!quarter.externalQuarter)
                    {
                        if (!pointCount.ContainsKey(quarter.points[p]))
                            pointCount.Add(quarter.points[p], 1);
                        else
                            pointCount[quarter.points[p]]++;

                        int indexB = p < quarterSize - 1 ? p + 1 : 0;
                        int indexC = p > 0 ? p - 1 : quarterSize - 1;
                        Vector2Fixed vipb = quarter[indexB];
                        Vector2Fixed vipc = quarter[indexC];

                        if (!pointMap.ContainsKey(vip))
                            pointMap.Add(vip, new List<Vector2Fixed>());
                        if (!pointMap[vip].Contains(vipb))
                            pointMap[vip].Add(vipb);
                        if (!pointMap[vip].Contains(vipc))
                            pointMap[vip].Add(vipc);

                        foreach (CarcassonneRoad road in quarter.roads)
                        {
                            road.generate = true;
                        }
                        roadNodes[vip].generate = true;
                    }
                }
            }

            //Find a point in the network that is on the city limits
            //This will be a point that has two connections
            //Internal city points will connect to three points
            //External points will connect to nothing
            Vector2Fixed startPoint = new Vector2Fixed();
            foreach (KeyValuePair<Vector2Fixed, int> var in pointCount)
            {
                if (var.Value == 2)
                {
                    startPoint = var.Key;
                    break;
                }
            }

            //Iterate through the points to build the external city wall point list
            if (pointMap.ContainsKey(startPoint))
            {
                List<Vector2Fixed> wallPoints = new List<Vector2Fixed>();
                Vector2Fixed currentPoint = startPoint;
                wallPoints.Add(currentPoint);
                while (true)
                {
                    List<Vector2Fixed> connections = pointMap[currentPoint];
                    int connectionCount = connections.Count;
                    for (int c = 0; c < connectionCount; c++)
                    {
                        Vector2Fixed connection = connections[c];
                        if (pointCount[connection] < 3)
                        {
                            if (!wallPoints.Contains(connection))
                            {
                                wallPoints.Add(connection);
                                break;
                            }
                        }
                    }
                    if (wallPoints.Count > 0 && wallPoints[wallPoints.Count - 1] == currentPoint) break;//no new connection found
                    if (wallPoints.Count > 1 && wallPoints[wallPoints.Count - 1] == wallPoints[0]) break;//wall circle isComplete
                    currentPoint = wallPoints[wallPoints.Count - 1];
                }

                wall.points.AddRange(wallPoints);
            }

            //Create sub plots from the main quarter shapes
            //Plot class will subdivide based on the defined variables
            List<CarcassonnePlot> gplots = new List<CarcassonnePlot>();
            for (int r = 0; r < numberOfQuarters; r++)
            {
                CarcassonneQuarter quarter = quarters[r];
                if (quarter.externalQuarter) continue;
                
                ShapeSplitter splitter = new ShapeSplitter();
                Shape[] shapes = splitter.Execute(quarter.shrunkPoints.ToArray(), _rGen.generateSeed, splitSettings);
                for(int s = 0; s < shapes.Length; s++)
                {
                    CarcassonnePlot newPlot = new CarcassonnePlot();
                    newPlot.AddShape(shapes[s]);
                    quarter.plots.Add(newPlot);
                    gplots.Add(newPlot);
                }
            }

            // MeshGeneratorManager meshGeneratorManager = MeshGeneratorManager.Instance;
            // BuildingGenerationManager buildingGenerationManager = BuildingGenerationManager.Instance;
            GenerationManager generationManager = GenerationManager.Instance;
            StaticRandom.GenerateNewSeed();
            
            int gplotCount = gplots.Count;
            for (int gp = 0; gp < gplotCount; gp++)
            {
                CarcassonnePlot plot = gplots[gp];
//                Debug.Log("Execute "+plot.data.area);
                if (plot.area > 1800)
                    continue;
                if (!plot.hasExternalAccess)
                    continue;
                if (plot.points.Count < 3)
                    continue;
                
                BuildingPlot bPlot = new BuildingPlot();
                bPlot.AddPointsRecenter(plot.points.ToArray());
                uint buildingSeed = StaticRandom.generateSeed;
                BuildingData building = BuildingGenerator.Create(buildingSeed, constraint.content, bPlot, "Test");
                building.position = bPlot.position;
                building.atlas = wallSectionAtlas.content;
                building.generatedBuildingData = new GeneratedBuildingData(buildingSeed, constraint.content, bPlot);
                // MeshGeneratorItem item = new BuildingGeneratorItem(building, true);
                // DebugBounds.Draw(building.bounds, Color.red, 30);
                // meshGeneratorManager.AddItem(item);
                generationManager.Add(building);
            }
            
            
//            int quartersCount = quarters.Count;
//            Debug.Log("quartersCount "+quartersCount);
//            for (int r = 0; r < quartersCount; r++)
//            {
//                CarcassonneQuarter quarter = quarters[r];
//                if (quarter.externalQuarter) continue;
//                int quarterSize = quarter.points.Count;
//
//                int plotCount = quarter.plots.Count;
//                Debug.Log("plotCount "+r+" "+plotCount);
//                Debug.Log("quarterSize "+r+" "+quarterSize);
//            }
//
//            int wallLength = wall.points.Count;
//            Debug.Log("wallLength "+wallLength);

            
            return;
            

            //Generate the buildings from the sub plot shapes and the building contstaints
//            settings = GenesisSettings.Get();
//            GRandom.seed = seed;
//            constraints.CheckFacades();
//            if (!queueGenerateBuildings)
//            {
//                int gplotCount = gplots.Count;
//                for (int gp = 0; gp < gplotCount; gp++)
//                {
//                    if (gplots[gp].area > 550)
//                        continue;
//                    BuildingRuntime building = BuildingGenerator.CreateRuntime(gplots[gp], constraints, settings.defaultGeneratedBuildingName);
//                    building.Place(gplots[gp].transform.position);
//                }
//            }
//
//            if (_roadMesh != null)
//                _roadMesh.Clear();
//            else
//                _roadMesh = VisualPartRuntime.GetPoolItem();

//            _roadMesh.name = "road mesh";
//            _roadMesh.transform.parent = null;
//            int roadCount = roads.Count;
//            for (int r = 0; r < roadCount; r++)
//            {
//                if (!roads[r].generate) continue;
//                RawMeshData meshData = roads[r].GenerateRoad();
//                if (meshData != null)
//                    _roadMesh.dynamicMesh.AddData(meshData);
//            }
//            int roadPointCount = nodes.Count;
//            for (int rn = 0; rn < roadPointCount; rn++)
//            {
//                if (!nodes[rn].generate) continue;
//                RawMeshData meshData = nodes[rn].GenerateRoad();
//                if (meshData != null)
//                    _roadMesh.dynamicMesh.AddData(meshData);
//            }
//            int quarterCount = quarters.Count;
//            for (int q = 0; q < quarterCount; q++)
//            {
//                CarcassonneQuarter quarter = quarters[q];
//                int plotCount = quarter.plots.Count;
//                for (int p = 0; p < plotCount; p++)
//                {
//                    if (quarter.plots[p].data == null) continue;
//                    RawMeshData meshData = Plot.GeneratePlot(quarter.plots[p].data);
//                    if (meshData != null)
//                        _roadMesh.dynamicMesh.AddData(meshData);
//                }
//            }
//            _roadMesh.GenerateFromDynamicMesh();
        }

        private void Start() 
        {
//            constraints.CheckFacades();
//            Invoke("ExecuteTimeMonitored", 0.93f);
            Execute();
        }

        private void ExecuteTimeMonitored()
        {
            Execute();
        }

        private void Update()
        {
//            Display(); //Uncomment to see debug scene output
//
//            if (queueGenerateBuildings)
////            if (queueGenerateBuildings && Input.GetMouseButtonDown(0))
//            {
//                int gplotCount = unbuiltPlots.Count;
//
//                if (gplotCount > 0)
//                {
//                    float t = Time.realtimeSinceStartup;
//                    while (Time.realtimeSinceStartup - t < 0.030f)
//                    {
//                        GenesisPlot plot = unbuiltPlots[0];
//                        unbuiltPlots.RemoveAt(0);
//
//                        if (plot.area < 700 && plot.numberOfPoints > 3)
//                        {
//                            BuildingRuntime building = BuildingGenerator.CreateRuntime(plot, constraints, settings.defaultGeneratedBuildingName);
//                            building.Place(plot.transform.position + Vector3.up * 0.1f);
//                        }
//
//                        gplotCount--;
////                        Debug.Log(gplotCount +" "+ DateTime.Now.ToString("HH:mm:ss tt"));
//                        if (gplotCount == 0)
//                            break;
//                    }
//                }
//            }
        }

        private bool PointOnEdge(Vector2Fixed point)
        {
            if (point.vx == 0) return true;
            if (point.vx == mapSize) return true;
            if (point.vy == 0) return true;
            if (point.vy == mapSize) return true;
            return false;
        }

        private CarcassonneQuarter[] GetQuarters(Vector2Fixed pointA, Vector2Fixed pointB)
        {
            CarcassonneQuarter[] output = new CarcassonneQuarter[2];
            output[0] = null;
            output[1] = null;

            List<CarcassonneQuarter> quartersA = pointQuarters[pointA];
            List<CarcassonneQuarter> quartersB = pointQuarters[pointB];

            int quartersACount = quartersA.Count;
            int quartersBCount = quartersB.Count;

            for (int a = 0; a < quartersACount; a++)
            {
                CarcassonneQuarter qa = quartersA[a];
                for (int b = 0; b < quartersBCount; b++)
                {
                    CarcassonneQuarter qb = quartersB[b];

                    if (qa == qb)
                    {
                        if (output[0] == null)
                            output[0] = qa;
                        else if (output[1] == null)
                            output[1] = qa;
                        break;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Display the data of the Voronoi/City at runtime
        /// </summary>
        private void Display()
        {
            int quartersCount = quarters.Count;

            for (int r = 0; r < quartersCount; r++)
            {
                CarcassonneQuarter quarter = quarters[r];
                if (quarter.externalQuarter) continue;
                int quarterSize = quarter.points.Count;

                int plotCount = quarter.plots.Count;
                for (int p = 0; p < plotCount; p++)
                {
                    CarcassonnePlot plot = quarter.plots[p];
                    if(!plot.data.HasExternalAccess())
                        continue;
                    int plotSize = plot.points.Count;
                    for (int pp = 0; pp < plotSize; pp++)
                    {
                        int indexB = pp < plotSize - 1 ? pp + 1 : 0;
                        Vector3 p0 = plot.points[pp].vector3XZ;
                        Vector3 p1 = plot.points[indexB].vector3XZ;
                        Debug.DrawLine(p0, p1, Color.blue);
                    }
                }

                for (int s = 0; s < quarterSize; s++)
                {
                    Vector3 p0 = quarter.shrunkPoints[s];
                    int indexB = s < quarterSize - 1 ? s + 1 : 0;
                    Vector3 p1 = quarter.shrunkPoints[indexB];

                    p0.z = p0.y;
                    p1.z = p1.y;

                    p0.y = 0;
                    p1.y = 0;

                    Debug.DrawLine(p0, p1, Color.red);
                }
            }

            int wallLength = wall.points.Count;
            for (int w = 0; w < wallLength; w++)
            {
                int indexB = w < wallLength - 1 ? w + 1 : 0;

                Vector3 p0 = wall.points[w].vector3XZ;
                Vector3 p1 = wall.points[indexB].vector3XZ;

                Debug.DrawLine(p0, p1, Color.green);
            }

//            foreach (KeyValuePair<Vector2Fixed, List<CarcassonneQuarter>> var in pointQuarters)
//            {
//                Color col = new Color(var.Key.vx % 1f, var.Key.vx * var.Key.vy % 1, var.Key.vy % 1f);
//                foreach (CarcassonneQuarter q in var.Value)
//                {
//                    Debug.DrawLine(var.Key.vector3XZ, q.center.vector3XZ, col);
//                }
//            }
        }
    }
}