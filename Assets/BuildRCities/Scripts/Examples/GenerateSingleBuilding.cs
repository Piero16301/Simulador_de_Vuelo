using BuildRCities;
using BuildRCities.Data;
using BuildRCities.DataGenerators;
using BuildRCities.Scripts.Mesh_Generators.Task_Queue;
using JSpace;
using UnityEngine;

namespace BuildRCitiesExamples
{
    public class GenerateSingleBuilding : MonoBehaviour
    {
        [Header("Click to generate a building!")]
        
        public uint seed = 1;
        public bool randomise;
        public BuildingPlot plot;
        public BuildingData building = null;
        private GenerativeItem _generatedBuildingItem = null;
        public BuildingConstraintAsset constraint;
        public WallSectionAtlasAsset wallSectionAtlas;

        private void Start()
        {
            Generate();
        }

        private void Update()
        {
            if(Input.GetMouseButtonUp(0))
            {
                Generate();
            }
        }

        private void Generate()
        {
            if(_generatedBuildingItem != null)
                GenerationManager.Instance.Remove(_generatedBuildingItem);
            
            StaticRandom.GenerateNewSeed();
            
            plot = new BuildingPlot();
            Vector2[] points = {new Vector2(-11.59f, 17.6f), new Vector2(-6.06f, -20.33f), new Vector2(11.59f, 11.72f), new Vector2(7.15f, 20.32f)};
            plot.AddPointsRecenter(points);

            if(randomise)
                seed = RandomGenerator.GenerateSeed();
            building = BuildingGenerator.Create(seed, constraint.content, plot, "Test");
            building.atlas = wallSectionAtlas.content;
            _generatedBuildingItem = GenerationManager.Instance.Add(building);
        }

        private void OnDrawGizmos()
        {
            plot.OnDrawGizmos();
        }
        
        private void OnGUI()
        {
            GUILayout.Box("Click left mouse button to generate a new building.");
        }
    }
}