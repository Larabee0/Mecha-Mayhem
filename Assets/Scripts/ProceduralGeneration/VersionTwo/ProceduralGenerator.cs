using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace RedButton.ProGen.VersionTwo
{
    public class ProceduralGenerator : MonoBehaviour
    {
        [SerializeField] private float tileSize = 6f;
        [SerializeField] private int gridSize = 10;

        [SerializeField] private TileCombCounter combCounterPrefab;

        [SerializeField] private Tile topLeftCorner;
        [SerializeField] private Tile topRightCorner;
        [SerializeField] private Tile bottomLeftCorner;
        [SerializeField] private Tile bottomRightCorner;


        [SerializeField] private Tile LeftSide;
        [SerializeField] private Tile RightSide;
        [SerializeField] private Tile TopSide;
        [SerializeField] private Tile bottomSide;

        [SerializeField] private Tile[] tilePrefabs;
        [SerializeField] private List<Tile> tileInstances;

        private TileCombCounter[] combCounters;
        private VirtualTile[] grid;
        private HashSet<int> resolvedTiles = new();

        [SerializeField, Min(1)] private uint seed = 1;
        private Unity.Mathematics.Random random;

        private void Start()
        {
            random = new Unity.Mathematics.Random(seed);
            GenerateTilePairs();

            for (int i = 0; i < tilePrefabs.Length; i++)
            {
                tilePrefabs[i].gameObject.SetActive(false);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                random = new Unity.Mathematics.Random(seed);
                //StopAllCoroutines();
                tileInstances.ForEach(tile => Destroy(tile.gameObject));
                tileInstances.Clear();
                resolvedTiles.Clear();
                GenerateVirtualGrid();
                StartCoroutine(GridResolver());
            }
        }

        private void GenerateTilePairs()
        {
            for (int i = 0; i < tilePrefabs.Length; i++)
            {
                for (int j = 0; j < tilePrefabs.Length; j++)
                {
                    if (tilePrefabs[i].topStatus.SetEquals(tilePrefabs[j].bottomStatus))
                    {
                        tilePrefabs[i].topValid.Add(tilePrefabs[j]);
                        tilePrefabs[j].bottomValid.Add(tilePrefabs[i]);
                    }

                    if (tilePrefabs[i].leftStatus.SetEquals(tilePrefabs[j].rightStatus))
                    {
                        tilePrefabs[i].leftValid.Add(tilePrefabs[j]);
                        tilePrefabs[j].rightValid.Add(tilePrefabs[i]);
                    }
                }
            }

            GeneratePairsEdges(topLeftCorner);
            GeneratePairsEdges(topRightCorner);
            GeneratePairsEdges(bottomLeftCorner);
            GeneratePairsEdges(bottomRightCorner);
            GeneratePairsEdges(LeftSide);
            GeneratePairsEdges(RightSide);
            GeneratePairsEdges(TopSide);
            GeneratePairsEdges(bottomSide);


            for (int i = 0; i < tilePrefabs.Length; i++)
            {
                tilePrefabs[i].GenerateDebuggingLists();
            }
        }

        private void GeneratePairsEdges(Tile edgeTile)
        {
            for (int i = 0; i < tilePrefabs.Length; i++)
            {
                if (edgeTile.topStatus.SetEquals(tilePrefabs[i].bottomStatus))
                {
                    edgeTile.topValid.Add(tilePrefabs[i]);
                }

                if (edgeTile.leftStatus.SetEquals(tilePrefabs[i].rightStatus))
                {
                    edgeTile.leftValid.Add(tilePrefabs[i]);
                }

                if (edgeTile.bottomStatus.SetEquals(tilePrefabs[i].topStatus))
                {
                    edgeTile.bottomValid.Add(tilePrefabs[i]);
                }

                if (edgeTile.rightStatus.SetEquals(tilePrefabs[i].leftStatus))
                {
                    edgeTile.rightValid.Add(tilePrefabs[i]);
                }
            }
            edgeTile.gameObject.SetActive(false);
            edgeTile.GenerateDebuggingLists();
        }

        private void GenerateVirtualGrid()
        {
            grid = new VirtualTile[gridSize * gridSize];
            combCounters = new TileCombCounter[grid.Length];
            for (int i = 0; i < grid.Length; i++)
            {
                combCounters[i] = Instantiate(combCounterPrefab, new Vector3((i % gridSize) * tileSize, 3.6f, (i / gridSize) * tileSize), Quaternion.Euler(90f,0,0), transform);
                grid[i] = new(gridSize,i, new int2(i % gridSize, i / gridSize), combCounters[i]);
                grid[i].potentialTiles.UnionWith(tilePrefabs);
                grid[i].UpdateComboCounter();
            }
        }

        private IEnumerator GridResolver()
        {
            int frameCount = 0;
            yield return null;
            frameCount++;
            while (resolvedTiles.Count != grid.Length)
            {
                VirtualTile firstTile = null;
                int firstTileIndex = GetNextTile(ref firstTile);
                try
                {
                    Resolve(firstTile);
                    tileInstances.Add(Instantiate(firstTile.resolvedTile, new Vector3(firstTile.coordinates.x * tileSize, 0, firstTile.coordinates.y * tileSize), Quaternion.identity, transform));
                    tileInstances[^1].gameObject.SetActive(true);
                    PropogateToNeighbours(firstTile);
                }
                catch
                {
                    break;
                }
                resolvedTiles.Add(firstTileIndex);
                yield return null;
            }
            Debug.LogFormat("Resolved Grid in {0} frames",frameCount);

        }

        public void Resolve(VirtualTile tile)
        {
            if (tile.resolvedTile != null)
            {
                Debug.Log("Trying to resolved already resolved tile!");
                return;
            }
            List<Tile> tiles = GetPotentialsFromNeighbours(tile);
            int resolvedTile = random.NextInt(0, tiles.Count);
            if (resolvedTile < 0 || resolvedTile > tiles.Count - 1)
            {
                throw new InvalidOperationException(string.Format("Index {0} out side collection range {1}", resolvedTile, tiles.Count));
            }
            tile.resolvedTile = tiles[resolvedTile];
            tile.potentialTiles.Clear();
            tile.UpdateComboCounter();
            tile.potentialTiles = null;
        }

        private List<Tile> GetPotentialsFromNeighbours(VirtualTile tile)
        {
            List<int> neighbours = tile.Neighbours;
            HashSet<Tile> allowedTiles = new(tilePrefabs);
            bool4 allResolved = false;
            // resolved tiles First
            for (Directions d = Directions.North; d <= Directions.East; d++)
            {
                if (neighbours[(int)d] >= 0)
                {
                    VirtualTile neighbour = grid[neighbours[(int)d]];
                    if (neighbour.Resolved)
                    {
                        allResolved[(int)d] = true;
                        allowedTiles.IntersectWith(neighbour.resolvedTile[d.Opposite()]);
                    }
                }
                else
                {
                    allResolved[(int)d] = true;
                }
            }
            // unresolvedTiles next
            // for (Directions d = Directions.North; d <= Directions.East; d++)
            // {
            //     if (neighbours[(int)d] >= 0)
            //     {
            //         VirtualTile neighbour = grid[neighbours[(int)d]];
            //         if (!neighbour.Resolved)
            //         {
            // 
            //         }
            //     }
            // }

            return new List<Tile>(allowedTiles);
        }

        public int GetNextTile(ref VirtualTile nextTile)
        {
            int tileIndex = GetLowestPotential();
            if(tileIndex < 0)
            {
                while (nextTile == null)
                {
                    tileIndex = random.NextInt(0, grid.Length);
                    // firstTileIndex = CoordToIndex(new int2(1, 1));
                    if (resolvedTiles.Contains(tileIndex))
                    {
                        continue;
                    }
                    nextTile = grid[tileIndex];
                }
            }
            else
            {
                nextTile = grid[tileIndex];
            }
            return tileIndex;
        }

        public int GetLowestPotential()
        {
            int lowestPotential = 5;
            int index = -1;
            for (int i = 0; i < grid.Length; i++)
            {
                int potential = 4;
                if (grid[i].Resolved)
                {
                    continue;
                }
                List<int> neighbours = grid[i].Neighbours;
                for (int n = 0; n < neighbours.Count; n++)
                {
                    if (neighbours[n] >= 0)
                    {
                        if (grid[neighbours[n]].Resolved)
                        {
                            potential--;
                        }
                    }
                }
                if(potential < lowestPotential)
                {
                    lowestPotential = potential;
                    index = i;
                }
            }
            return index;
        }

        public void PropogateToNeighbours(VirtualTile tile)
        {

        }

        public int CoordToIndex(int2 coord)
        {
            return coord.y * gridSize + coord.x;
        }

        public int2 IndexTocoord(int index)
        {
            return new int2(index % gridSize, index / gridSize);
        }

        public bool InBounds(int2 coord)
        {
            return coord.x >= 0 && coord.y >= 0 && coord.x < gridSize && coord.y < gridSize;
        }
    }


    public class VirtualTile
    {
        public TileCombCounter tileCounter;
        public int gridSize;
        public int X => coordinates.x;
        public int Y => coordinates.y;
        public bool IsEdgeTile => X == 0 || X == gridSize - 1 || Y == 0 || Y == gridSize - 1;
        public bool Resolved => resolvedTile != null;
        public int index;
        public int2 coordinates;
        public HashSet<Tile> potentialTiles;
        private List<int> neighbours =null;
        public List<int> Neighbours
        {
            get
            {
                neighbours ??= new()
                {
                    NeighbourN,
                    NeighbourS,
                    NeighbourW,
                    NeighbourE
                };
                return neighbours;
            }
        }
        public Tile resolvedTile;

        public VirtualTile(int gridSize, int index, int2 coordinates, TileCombCounter tileCounter)
        {
            this.gridSize = gridSize;
            this.index = index;
            this.coordinates = coordinates;
            potentialTiles = new HashSet<Tile>();
            resolvedTile = null;
            this.tileCounter = tileCounter;
        }

        public int NeighbourN
        {
            get
            {
                bool CanHaveNortherNeighbour = true;
                if (IsEdgeTile)
                {
                    CanHaveNortherNeighbour = Y + 1 < gridSize && X + 1 < gridSize;
                }
                if (CanHaveNortherNeighbour)
                {
                    return CoordToIndex(new int2(X, Y + 1));
                }
                return -1;
            }
        }

        public int NeighbourS
        {
            get
            {
                bool CanHaveNortherNeighbour = true;
                if (IsEdgeTile)
                {
                    CanHaveNortherNeighbour = Y - 1 >= 0;
                }
                if (CanHaveNortherNeighbour)
                {
                    return CoordToIndex(new int2(X, Y - 1));
                }
                return -1;
            }
        }

        public int NeighbourE
        {
            get
            {
                bool CanHaveNortherNeighbour = true;
                if (IsEdgeTile)
                {
                    CanHaveNortherNeighbour = X + 1 < gridSize;
                }
                if (CanHaveNortherNeighbour)
                {
                    return CoordToIndex(new int2(X + 1, Y));
                }
                return -1;
            }
        }

        public int NeighbourW
        {
            get
            {
                bool CanHaveNortherNeighbour = true;
                if (IsEdgeTile)
                {
                    CanHaveNortherNeighbour = X - 1 >= 0;
                }
                if (CanHaveNortherNeighbour)
                {
                    return CoordToIndex(new int2(X - 1, Y));
                }
                return -1;
            }
        }

        public void UpdateComboCounter()
        {
            if(tileCounter != null && potentialTiles!= null)
            {
                tileCounter.PossibleTiles = potentialTiles.Count;
            }
        }

        public int CoordToIndex(int2 coord)
        {
            return coord.y * gridSize + coord.x;
        }

    }
}
