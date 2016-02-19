﻿using System.Collections;
using System.Collections.Generic;
using Assets.Examples.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Examples.Mazes
{
    public class MazeGeneratorUI : UIBase
    {
        public RectTransform leftPanel;
        public ToggleGroup algorithmsGroup;
        public RawImage mazeImage;

        private Texture2D texture;
        private int textureWidth = 256;
        private int textureHeight = 256;
        private bool useGradient = true;
        private MazeGenerator mazeGenerator;
        private MazeGenerator.Algorithm generatorAlgorithm = MazeGenerator.Algorithm.RandomTraversal;
        private int cellSize = 2;
        private int wallSize = 1;
        private float hue;
        private float gradientLength = 30;

        private MazeGenerator.Algorithm[] algorithms = new[]
        {
            MazeGenerator.Algorithm.None,
            MazeGenerator.Algorithm.RandomTraversal,
            MazeGenerator.Algorithm.RandomDepthFirstTraversal,
            MazeGenerator.Algorithm.RandomBreadthFirstTraversal,
        };

        private Dictionary<MazeGenerator.Algorithm, string> algorithmToString =
            new Dictionary<MazeGenerator.Algorithm, string>
            {
                {MazeGenerator.Algorithm.None, "None"},
                {MazeGenerator.Algorithm.RandomTraversal, "Random traversal"},
                {MazeGenerator.Algorithm.RandomDepthFirstTraversal, "Random depth-first traversal"},
                {MazeGenerator.Algorithm.RandomBreadthFirstTraversal, "Random breadth-first traversal"}
            };

        private void Awake()
        {
            var header = InstantiateControl<TextControl>(algorithmsGroup.transform.parent);
            header.Initialize("Generator algorithm");
            header.transform.SetAsFirstSibling();
            for (int i = 0; i < algorithms.Length; i++)
            {
                MazeGenerator.Algorithm algorithm = algorithms[i];
                var toggle = InstantiateControl<ToggleControl>(algorithmsGroup.transform);
                toggle.Initialize(
                    header: algorithmToString[algorithm],
                    value: algorithm == generatorAlgorithm,
                    onValueChanged: isOn =>
                    {
                        if (isOn)
                        {
                            generatorAlgorithm = algorithm;
                            Generate();
                        }
                    },
                    toggleGroup: algorithmsGroup);
            }

            InstantiateControl<SliderControl>(leftPanel).Initialize("Cell Size", 1, 10, cellSize, value =>
            {
                cellSize = value;
                Generate();
            });

            InstantiateControl<SliderControl>(leftPanel).Initialize("Wall Size", 1, 10, wallSize, value =>
            {
                wallSize = value;
                Generate();
            });

            InstantiateControl<ToggleControl>(leftPanel).Initialize("Use gradient", useGradient, value =>
            {
                useGradient = value;
                Generate();
            });

            InstantiateControl<ButtonControl>(leftPanel).Initialize("Generate new maze", Generate);

            Generate();
        }

        private void Generate()
        {
            StopAllCoroutines();

            texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false, true)
            {
                filterMode = FilterMode.Point
            };
            texture.Clear(Color.black);
            texture.Apply();
            mazeImage.texture = texture;

            mazeGenerator = new MazeGenerator(textureWidth, textureHeight, cellSize, wallSize);

            StartCoroutine(GenerateCoroutine());
        }

        private IEnumerator GenerateCoroutine()
        {
            var algorithm = generatorAlgorithm;
            if (algorithm == MazeGenerator.Algorithm.None)
            {
                algorithm = RandomE.GetRandom(MazeGenerator.Algorithm.RandomTraversal,
                    MazeGenerator.Algorithm.RandomDepthFirstTraversal,
                    MazeGenerator.Algorithm.RandomBreadthFirstTraversal);
            }

            hue = Random.value;

            switch (algorithm)
            {
                case MazeGenerator.Algorithm.RandomTraversal:
                    yield return StartCoroutine(mazeGenerator.RandomTraversal(DrawEdge, texture.Apply));
                    break;
                case MazeGenerator.Algorithm.RandomDepthFirstTraversal:
                    yield return StartCoroutine(mazeGenerator.RandomDepthFirstTraversal(DrawEdge, texture.Apply));
                    break;
                case MazeGenerator.Algorithm.RandomBreadthFirstTraversal:
                    yield return StartCoroutine(mazeGenerator.RandomBreadthFirstTraversal(DrawEdge, texture.Apply));
                    break;
            }
            texture.Apply();
        }

        private void DrawEdge(Edge edge)
        {
            int x, y, width, height;
            if (edge.origin.direction == Directions.Left || edge.origin.direction == Directions.Down)
            {
                x = Translate(edge.exit.x);
                y = Translate(edge.exit.y);
            }
            else
            {
                x = Translate(edge.origin.x);
                y = Translate(edge.origin.y);
            }

            if (edge.origin.direction == Directions.Left || edge.origin.direction == Directions.Right)
            {
                width = cellSize*2 + wallSize;
                height = cellSize;
            }
            else
            {
                width = cellSize;
                height = cellSize*2 + wallSize;
            }

            Color color;
            if (useGradient)
            {
                float gradient = Mathf.Abs((Mathf.Repeat(edge.origin.depth/gradientLength, 1) - 0.5f)*2);
                color = new ColorHSV(hue, gradient, gradient).ToColor();
            }
            else
            {
                color = Color.white;
            }
            texture.DrawRect(x, y, width, height, color);
        }

        private int Translate(int x)
        {
            return wallSize + x*(cellSize + wallSize);
        }
    }
}