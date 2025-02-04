﻿using UnityEngine;
using System.Collections;

public class DebugCity : MonoBehaviour 
{
    [SerializeField]
    private bool showCityBounds;
    [SerializeField]
    private bool showDistrictPerimeter;
    [SerializeField]
    private bool showDistrictSeedPoint;
    [SerializeField]
    private bool showBlockPerimeter;
    [SerializeField]
    private bool showBlockControlPoint;
    [SerializeField]
    private bool showBuildingPositions;
    [SerializeField]
    private bool showChunkBoundaries;
    [SerializeField]
    private bool showItemPositions;

    private CityChunkManager chunkManager;

    /// <summary>
    /// Grab any variables that need grabbing.
    /// </summary>
    void Start()
    {
        chunkManager = GetComponent<CityChunkManager>();
    }

    /// <summary>
    /// Draws city using Gizmos.
    /// </summary>
    void OnDrawGizmos()
    {
        City city = Game.Instance.CityInstance;
        ItemPoolManager poolManager = Game.Instance.ItemPoolInstance;
        
        // Make sure the city is defined
        if (city != null)
        {
            // Show city Bounds
            if (showCityBounds)
            {
                Gizmos.color = Color.blue;
                drawBox(city.BoundingBox);
            }

			if(showItemPositions)
            {
            	ItemPoolInfo[,] itemInfo = poolManager.GetItemPool();
            	int i, j, k;

				Gizmos.color = Color.yellow;

				for(i = 0; i < itemInfo.GetLength(0); ++i)
            	{
					for(j = 0; j < itemInfo.GetLength(1); ++j)
            		{
						if(itemInfo[i, j] != null && itemInfo[i, j].Locations.Count > 0)
            			{
            				for(k = 0; k < itemInfo[i, j].Locations.Count; ++k)
            				{
            					Gizmos.DrawSphere(itemInfo[i, j].Locations[k], 1f);
            				}
            			}
            		}
            	}
            }

            // In each district
            for (int i = 0; i < city.Districts.Length; ++i)
            {
                District district = city.Districts[i];
                
                // Show district perimter
                if (showDistrictPerimeter)
                {
                    Gizmos.color = Color.cyan;
                    drawPolygon(district.EdgeVerticies);
                }

                // Show district seed points
                if (showDistrictSeedPoint)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(district.SeedPoint, 0.5f);
                }

                // In each block
                for (int j = 0; j < district.Blocks.Count; ++j)
                {
                    Block block = district.Blocks[j];

                    // Show block perimeter
                    if (showBlockPerimeter)
                    {
                        Gizmos.color = Color.magenta;
                        drawPolygon(block.Verticies);
                    }

                    // Show control points
                    if (showBlockControlPoint)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(block.Center, 0.2f);
                    }

                    if (showBuildingPositions)
                    {
                        // In each building
                        for (int k = 0; k < block.Buildings.Count; ++k)
                        {
                            Gizmos.color = Color.black;
                            Gizmos.DrawSphere(block.Buildings[k].Position, 0.15f);
                        }
                    }
                }
            }

            if (showChunkBoundaries)
            {
                if (chunkManager.Chunks != null)
                {
                    for (int i = 0; i <= chunkManager.ChunksAcross; ++i)
                    {
                        for (int j = 0; j <= chunkManager.ChunksDown; ++j)
                        {
                            Gizmos.color = Color.red;

                            if (chunkManager.Chunks[i, j].IsLoaded)
                            {
                                Gizmos.color = Color.green;
                            }

                            drawBox(chunkManager.Chunks[i, j].BoundingBox);
                        }
                    }
                }
            }

            if (showChunkBoundaries)
            {
                if (chunkManager.Chunks != null)
                {
                    for (int i = 0; i <= chunkManager.ChunksAcross; ++i)
                    {
                        for (int j = 0; j <= chunkManager.ChunksDown; ++j)
                        {
                            Gizmos.color = Color.red;

                            if (chunkManager.Chunks[i, j].IsLoaded)
                            {
                                Gizmos.color = Color.green;
                            }

                            drawBox(chunkManager.Chunks[i, j].BoundingBox);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws a polygon.
    /// </summary>
    /// <param name="points">The verticies of the polygon.</param>
    /// <param name="color">The color to draw.</param>
    private void drawPolygon (Vector3[] points)
    {
        Vector3 last = points[0];

        for (int v = 1; v < points.Length; ++v)
        {
            Gizmos.DrawLine(last, points[v]);
            last = points[v];
        }

        Gizmos.DrawLine(last, points[0]);
    }

    /// <summary>
    /// Draws the edges of a bounding box.
    /// </summary>
    /// <param name="box">Bounding box</param>
    /// <param name="color">Debug color</param>
    private void drawBox (Bounds box)
    {
        float maxX = box.max.x;
        float minX = box.min.x;
        float maxY = box.max.y;
        float minY = box.min.y;
        float maxZ = box.max.z;
        float minZ = box.min.z;

        Vector3 a = new Vector3(minX, minY, minZ);
        Vector3 b = new Vector3(minX, minY, maxZ);
        Vector3 c = new Vector3(minX, maxY, minZ);
        Vector3 d = new Vector3(minX, maxY, maxZ);
        Vector3 e = new Vector3(maxX, minY, minZ);
        Vector3 f = new Vector3(maxX, minY, maxZ);
        Vector3 g = new Vector3(maxX, maxY, minZ);
        Vector3 h = new Vector3(maxX, maxY, maxZ);

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(a, c);
        Gizmos.DrawLine(a, e);

        Gizmos.DrawLine(d, b);
        Gizmos.DrawLine(d, c);
        Gizmos.DrawLine(d, h);

        Gizmos.DrawLine(f, b);
        Gizmos.DrawLine(f, e);
        Gizmos.DrawLine(f, h);

        Gizmos.DrawLine(g, h);
        Gizmos.DrawLine(g, c);
        Gizmos.DrawLine(g, e);
    }
}
