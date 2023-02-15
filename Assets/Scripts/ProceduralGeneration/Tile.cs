using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.ProBuilder.Shapes;
using System;
using System.Linq;
using UnityEngine.InputSystem;

namespace RedButton.ProGen
{
    public class Tile : MonoBehaviour
    {
        private const float min = -2.5f;
        private const float max = 2.5f;
        private const float resolution = 0.5f;

        [SerializeField] private Tile allowedToNeighbour;

        [SerializeField] private Transform[] cubes;
        [SerializeField] private bool[] fillArray;
        [SerializeField] private float lengthWidth;
        [SerializeField] private float totalNodes;

        [SerializeField] private int2[] top;
        [SerializeField] private int2[] bottom;
        [SerializeField] private int2[] right;
        [SerializeField] private int2[] left;

        public readonly HashSet<int2> topStatus = new();
        public readonly HashSet<int2> bottomStatus = new();
        public readonly HashSet<int2> rightStatus = new();
        public readonly HashSet<int2> leftStatus = new();

        public readonly HashSet<Tile> doNotCompareTo= new();

        public HashSet<Tile> topValid = new();
        public HashSet<Tile> bottomValid = new();
        public HashSet<Tile> leftValid = new();
        public HashSet<Tile> rightValid = new();
        public HashSet<Tile> this[Directions dir] => dir switch
        {
            Directions.North => topValid,
            Directions.South => bottomValid,
            Directions.East => rightValid,
            Directions.West => leftValid,
            _ => null
        };
        [SerializeField]private Tile[] debug_topValid;
        [SerializeField] private Tile[] debug_bottomValid;
        [SerializeField]private Tile[] debug_leftValid;
        [SerializeField]private Tile[] debug_rightValid;

        private void Awake()
        {
            List<Transform> transforms = new(GetComponentsInChildren<Transform>());
            transforms.RemoveAt(0);
            transforms.RemoveAt(0);
            cubes = transforms.ToArray();
            CalculateEdges();

        }

        private void CalculateEdges()
        {
            lengthWidth = math.abs(min) + resolution + math.abs(max ) + resolution;
            totalNodes = lengthWidth * lengthWidth;

            fillArray = new bool[(int)totalNodes];
            for (int i = 0; i < cubes.Length; i++)
            {
                int2 coordinates = GetCoordinatesFromCube(cubes[i]);
                int index = GetTileIndexFromCoordinate(coordinates);
                fillArray[index] = true;
            }
            top = new int2[(int)lengthWidth];
            bottom = new int2[(int)lengthWidth];
            left = new int2[(int)lengthWidth];
            right = new int2[(int)lengthWidth];
            // edgeFillStatus
            for (int i = 0, x = 0, z = 0; x < lengthWidth; i++, x++,  z++)
            {
                int bottomIndex = GetTileIndexFromCoordinate(new int2(x, 0));
                bottom[i] = fillArray[bottomIndex] ? new int2(x, 1) : new int2(x, 0);

                int topIndex = GetTileIndexFromCoordinate(new int2(x, ((int)lengthWidth) - 1));
                top[i] = fillArray[topIndex] ? new int2(x, 1) : new int2(x, 0);

                int leftIndex = GetTileIndexFromCoordinate(new int2(0, z));
                left[i] = fillArray[leftIndex] ? new int2(z, 1) : new int2(z, 0);

                int rightIndex = GetTileIndexFromCoordinate(new int2(((int)lengthWidth) - 1, z));
                right[i] = fillArray[rightIndex] ? new int2(z, 1) : new int2(z, 0);
            }

            topStatus.UnionWith(top);
            bottomStatus.UnionWith(bottom);
            leftStatus.UnionWith(left);
            rightStatus.UnionWith(right);
        }

        private int2 GetCoordinatesFromCube(Transform cube)
        {
            float3 coordinate = cube.localPosition;
            coordinate.x += -resolution + (max + resolution);
            coordinate.z += -resolution + (max + resolution);
            int x = Mathf.Clamp(Mathf.RoundToInt(coordinate.x), 0, (int)lengthWidth-1);
            int y = Mathf.Clamp(Mathf.RoundToInt(coordinate.z), 0, (int)lengthWidth-1);
            return new int2(x, y);
        }

        private int GetTileIndexFromCoordinate(int2 coordinate)
        {
            // Get index in 1d: array[index], like 2d: Array[x][y]
            // index = y * width + x;
            return math.clamp(coordinate.y * (int)lengthWidth + coordinate.x, 0, (int)totalNodes-1);
        }

        private int2 GetCoordinatesFromindex(int index)
        {
            int x = index % (int)lengthWidth;
            int y = index / (int)lengthWidth;
            return new int2(x, y);
        }

        private float3 GetPositionFromCoordinates(int2 coordinates)
        {
            float3 pos = new() { x = coordinates.x, y = 0, z = coordinates.y };
            pos.x -= -resolution + (max + resolution);
            pos.z -= -resolution + (max + resolution);
            return pos;
        }

        public void GenerateDebuggingLists()
        {
            debug_topValid = topValid.ToArray();
            debug_bottomValid = bottomValid.ToArray();
            debug_leftValid = leftValid.ToArray();
            debug_rightValid = rightValid.ToArray();
        }
    }

    public enum Directions : int
    {
        North,
        South,
        West,
        East
    }

    public static class DirectionsExt
    {
        public static Directions Opposite(this Directions dir)
        {
            return dir switch
            {
                Directions.North => Directions.South,
                Directions.South => Directions.North,
                Directions.West => Directions.East,
                Directions.East => Directions.West,
                _ => dir
            };
        }
    }
}