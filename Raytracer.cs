using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    //Raytracer, which owns the scene, camera and the display surface.
    //The Raytracer implements a method Render, which uses the camera to loop over the pixels of the screen plane and to generate a ray for each pixel,
    //which is then used to find the nearest intersection. The result is then visualized by plotting a pixel.
    //For the middle row of pixels (typically line 256 for a 512x512 window), it generates debug output by visualizing every Nthray (where N is e.g. 10).
    internal class Raytracer
    {
        Scene scene;
        Camera debugCamera;
        internal Camera camera;
        Surface screen;

        List<float> tempDistances = new List<float>();
        float[,] tempArray = new float[2,10000];
        int tempint = 0;

        bool debugMode;
        internal Raytracer(Surface screen)
        {
            scene = new Scene();
            camera = new Camera(new Vector3(0,0,0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), 1f, screen);
            debugMode = false;
            this.screen = screen;
        }
        internal void Render()
        {
            int width = screen.width;
            int height = screen.height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    screen.pixels[x + y * width] = 0;
                    Ray primaryRay = FindPrimaryRay(x, y, width, height);
                    Intersection primaryIntersection = scene.PrimaryRayIntersection(primaryRay);

                    if (debugMode) //Test debugMode (WIP)
                    {
                        
                    }
                    else
                    {
                        if (primaryIntersection.nearestPrimitive != null)
                        {
                            Vector3 color = Vector3.Zero;
                            int lightsCount = scene.lights.Count();
                            int tempColor = 0;
                            for (int i = 0; i < lightsCount; i++)
                            {
                                Light light = scene.lights[i];
                                Ray shadowRay = FindShadowRay(primaryIntersection, light);
                                Vector3 intensity = scene.FindShadowRayColor(shadowRay, light);
                                float distance = shadowRay.intersectionDistance;
                                Vector3 normal = primaryIntersection.normal;
                                Vector3 materialColor = primaryIntersection.nearestPrimitive.materialColor;
                                Vector3 specularColor = primaryIntersection.nearestPrimitive.speculalColor;
                                Vector3 r = -shadowRay.direction - 2 * Vector3.Dot(-shadowRay.direction, primaryIntersection.normal) * primaryIntersection.normal;
                                r.Normalize();
                                float n = 2000;
                                Vector3 materialsAmbientColor = materialColor;
                                Vector3 ambientLightRadiance = new Vector3(0.4f, 0.4f, 0.4f);

                                ////////test
                                normal.Normalize();
                                shadowRay.direction.Normalize();
                                color = light.rgbIntensity * (1 / (distance * distance)) * Math.Max(0, Vector3.Dot(normal, shadowRay.direction)) * materialColor;

                                if (y < screen.height / 2 && x % 100 == 0 && tempint < tempArray.GetLength(1))
                                {
                                    tempDistances.Add(Math.Max(0, Vector3.Dot(normal, shadowRay.direction)));
                                    tempArray[1, tempint] = distance;
                                    tempArray[0, tempint] = color.Y;
                                    tempint++;
                                }
                                if (tempint == tempArray.GetLength(1))
                                {
                                    Sort(tempArray, 0, "ASC");
                                    for(int j = 0; j < tempArray.GetLength(1); j++)
                                        Console.WriteLine(tempArray[0, j] + " __ " + tempArray[1,j]);
                                }
                                ///////einde test

                                //color = light.rgbIntensity * (1 / (distance * distance)) * Math.Max(0, Vector3.Dot(normal, shadowRay.direction)) * materialColor;
                                //color = light.rgbIntensity * (1 / (distance * distance)) * (materialColor * Math.Max(0, Vector3.Dot(normal, shadowRay.direction)) + specularColor * (float)Math.Pow(Math.Max(0, Vector3.Dot(-primaryRay.direction, r)), n)) + materialsAmbientColor * ambientLightRadiance;
                            }
                            tempColor = (int)Math.Round(color.X * 255 * 256 * 256 + color.Y * 255 * 256 + color.Z * 255);
                            screen.pixels[x + y * width] = tempColor;
                        }
                    }
                }
            }

        }

        internal Ray FindPrimaryRay(float x, float y, float width, float height)
        {
            Vector3 u = camera.rightTop - camera.leftTop;
            Vector3 v = camera.leftBottom - camera.leftTop;

            Vector3 screenPoint = camera.leftTop + (x / width) * u + (y / height) * v;
            Vector3 rayDirection = screenPoint - camera.position;
            float length = rayDirection.Length;
            rayDirection.Normalize();
            Ray ray = new Ray(camera.position, rayDirection, length);
            return ray;
        }
        internal Ray FindShadowRay(Intersection intersection, Light light)
        {
            Vector3 rayDirection = light.position - intersection.position;
            float length = rayDirection.Length;
            rayDirection.Normalize();
            return new Ray(intersection.position, rayDirection, length);
        }
        /// <summary>
        /// A generic routine to sort a two dimensional array of a specified type based on the specified column.
        /// </summary>
        /// <param name="array">The array to sort.</param>
        /// <param name="sortCol">The index of the column to sort.</param>
        /// <param name="order">Specify "DESC" or "DESCENDING" for a descending sort otherwise
        /// leave blank or specify "ASC" or "ASCENDING".</param>
        /// <remarks>The original array is sorted in place.</remarks>
        /// <see cref="http://stackoverflow.com/questions/232395/how-do-i-sort-a-two-dimensional-array-in-c"/>
        private static void Sort<T>(T[,] array, int sortCol, string order)
        {
            int colCount = array.GetLength(0), rowCount = array.GetLength(1);
            if (sortCol >= colCount || sortCol < 0)
                throw new System.ArgumentOutOfRangeException("sortCol", "The column to sort on must be contained within the array bounds.");

            DataTable dt = new DataTable();
            // Name the columns with the second dimension index values, e.g., "0", "1", etc.
            for (int col = 0; col < colCount; col++)
            {
                DataColumn dc = new DataColumn(col.ToString(), typeof(T));
                dt.Columns.Add(dc);
            }
            // Load data into the data table:
            for (int rowindex = 0; rowindex < rowCount; rowindex++)
            {
                DataRow rowData = dt.NewRow();
                for (int col = 0; col < colCount; col++)
                    rowData[col] = array[col, rowindex];
                dt.Rows.Add(rowData);
            }
            // Sort by using the column index = name + an optional order:
            DataRow[] rows = dt.Select("", sortCol.ToString() + " " + order);

            for (int row = 0; row <= rows.GetUpperBound(0); row++)
            {
                DataRow dr = rows[row];
                for (int col = 0; col < colCount; col++)
                {
                    array[col, row] = (T)dr[col];
                }
            }

            dt.Dispose();
        }
    }

}
