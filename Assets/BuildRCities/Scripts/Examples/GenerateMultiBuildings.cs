using System;
using BuildRCities;
using BuildRCities.Data;
using BuildRCities.DataGenerators;
using BuildRCities.Scripts.Mesh_Generators.Task_Queue;
using JSpace;
using UnityEngine;

namespace BuildRCitiesExamples
{
    public class GenerateMultiBuildings : MonoBehaviour
    { 
        [NonSerialized]
        private BuildingData[] _buildings;
        public BuildingConstraintAsset constraint;
        public WallSectionAtlasAsset wallSectionAtlas;
        public ProjectAssets projectAssets;
        public float startTime = 1.0f;
        public float generationTime = 10.0f;
        public uint seed = 1;
        private RandomGenerator _rgen;
        public Vector2Int gridSize = new Vector2Int(4, 4);
        public Vector2 plotSize = new Vector2Int(10, 10);

        private void Start()
        {
            if(projectAssets == null)
                Debug.LogError("Data Manager not included in scene - data references will not work at runtime");

            int buildingCount = gridSize.x * gridSize.y;
            _buildings = new BuildingData[buildingCount];
            _rgen = new RandomGenerator(seed);
//            Invoke("Generate", 2);
            Generate();
            //            InvokeRepeating("Generate", startTime, generationTime);
        }

//        private void Update()
//        {
//            Generate();
//        }

        private void Generate()
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    int index = x + y * gridSize.x;
                    Vector2 basePos = new Vector2(x * plotSize.x, y * plotSize.y);
                    Vector2[] footPrint = { basePos + new Vector2(_rgen.output * 3, _rgen.output * 3), basePos + new Vector2(plotSize.x - _rgen.output, _rgen.output * 3), basePos + new Vector2(plotSize.x - _rgen.output, plotSize.y - _rgen.output), basePos + new Vector2(_rgen.output * 3, plotSize.y - _rgen.output) };
                    BuildingPlot plot = new BuildingPlot();
                    plot.AddPointsRecenter(footPrint);
                    plot.density = _rgen.output;
                    plot.landUse = _rgen.output;
                    _buildings[index] = BuildingGenerator.Create(_rgen.generateSeed, constraint.content, plot, "Test", footPrint);
                    _buildings[index].atlas = wallSectionAtlas.content;

                    // GenerationManager.Instance.Add(_buildings[index]);
                    // BuildingGenerationManager.Instance.AddBuilding(_buildings[index]);
                }
            }

            GenerationManager.Instance.Add(_buildings);
        }
    }
}