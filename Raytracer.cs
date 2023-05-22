using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.ColorSpaces;
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
        float[,] tempArray = new float[2, 10000];
        int tempint = 0;

        internal bool debugMode;
        internal Raytracer(Surface screen)
        {
            scene = new Scene();
            camera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), 1f, screen);
            debugMode = false;
            this.screen = screen;
        }
        internal void Render()
        {
            int width = screen.width;
            int height = screen.height;


            if (debugMode)
            {
                screen.Clear(0x000000);

                screen.Line(TX(camera.leftBottom.X), TY(camera.position.Z - camera.distanceToScreenPlane), TX(camera.rightBottom.X), TY(camera.position.Z - camera.distanceToScreenPlane), 0xffffff);
                screen.Box(TX(camera.position.X) - 1, TY(camera.position.Z) - 1, TX(camera.position.X) + 1, TY(camera.position.Z) + 1, 0xffffff);

                foreach (var item in scene.primitives)
                    if (item is Sphere)
                    {
                        Sphere sphere = (Sphere)item;

                        for (float j = 0; j < 360; j += 3.6f)
                        {
                            float tempX1 = sphere.radius * (float)Math.Cos(MathHelper.DegreesToRadians(j));
                            float tempX2 = sphere.radius * (float)Math.Cos(MathHelper.DegreesToRadians(j + 3.6f));
                            float tempY1 = sphere.radius * (float)Math.Sin(MathHelper.DegreesToRadians(j));
                            float tempY2 = sphere.radius * (float)Math.Sin(MathHelper.DegreesToRadians(j + 3.6f));

                            screen.Line(TX(tempX1 + sphere.position.X), TY(tempY1 - sphere.position.Z), TX(tempX2 + sphere.position.X), TY(tempY2 - sphere.position.Z), 0xffffff);
                        }
                    }

                for (int x = 0; x < width; x++)
                    if (x % 20 == 0 || x == 0)
                    {
                        Ray primaryRay = FindPrimaryRay(x, height / 2, width, height);
                        Intersection primaryIntersection = scene.PrimaryRayIntersection(primaryRay);
                        if (primaryIntersection.nearestPrimitive != null && primaryIntersection.nearestPrimitive is Sphere)
                            screen.Line(TX(camera.position.X), TY(camera.position.Z), TX(primaryIntersection.position.X), TY(-primaryIntersection.position.Z), 0xfcba03);
                        else
                            screen.Line(TX(camera.position.X), TY(camera.position.Z), TX(primaryRay.direction.X * 100), TY(-primaryRay.direction.Z * 100), 0xfcba03);

                        if (primaryIntersection.nearestPrimitive != null)
                        {
                            int lightsCount = scene.lights.Count();
                            for (int i = 0; i < lightsCount; i++)
                            {
                                Light light = scene.lights[i];
                                Ray shadowRay = FindShadowRay(primaryIntersection, light);
                                Intersection shadowIntersection = scene.PrimaryRayIntersection(shadowRay);

                                if (shadowIntersection.nearestPrimitive != null)
                                    screen.Line(TX(primaryIntersection.position.X), TY(-primaryIntersection.position.Z), TX(shadowIntersection.position.X), TY(-shadowIntersection.position.Z), 0xff1100);
                                else if (!(shadowIntersection.nearestPrimitive is Sphere))
                                    screen.Line(TX(primaryIntersection.position.X), TY(-primaryIntersection.position.Z), TX(light.position.X), TY(-light.position.Z), 0xff1100);
                            }
                        }
                    }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        screen.pixels[x + y * width] = 0;
                        Ray primaryRay = FindPrimaryRay(x, y, width, height);
                        Intersection primaryIntersection = scene.PrimaryRayIntersection(primaryRay);

                        //    if (primaryIntersection.nearestPrimitive != null)
                        //    {

                        //        Vector3 color = Vector3.Zero;
                        //        int lightsCount = scene.lights.Count();
                        //        int tempColor = 0;

                        //        Vector3 normal = primaryIntersection.normal;
                        //        Vector3 materialColor = primaryIntersection.nearestPrimitive.materialColor;
                        //        Vector3 specularColor = primaryIntersection.nearestPrimitive.speculalColor;
                        //        Vector3 materialsAmbientColor = materialColor;
                        //        Vector3 ambientLightRadiance = new Vector3(0.05f, 0.05f, 0.05f);

                        //        for (int i = 0; i < lightsCount; i++)
                        //        {
                        //            Light light = scene.lights[i];
                        //            Ray shadowRay = FindShadowRay(primaryIntersection, light);
                        //            Vector3 intensity = scene.FindShadowRayColor(shadowRay, light);
                        //            float distance = shadowRay.intersectionDistance;

                        //            Vector3 r = -shadowRay.direction - 2 * Vector3.Dot(-shadowRay.direction, primaryIntersection.normal) * primaryIntersection.normal;
                        //            r.Normalize();
                        //            float n = 2;



                        //            if (intensity != Vector3.Zero)
                        //            {
                        //                color = light.rgbIntensity * (1 / (distance * distance)) * (materialColor * Math.Max(0, Vector3.Dot(normal, shadowRay.direction)) + specularColor * (float)Math.Pow(Math.Max(0, Vector3.Dot(-primaryRay.direction, r)), n));
                        //            }
                        //        }
                        //        color += materialsAmbientColor * ambientLightRadiance;

                        //int tempColorR = ((int)Math.Round(Math.Clamp(color.X, 0, 1) * 255)) * 256 * 256;
                        //int tempColorG = ((int)Math.Round(Math.Clamp(color.Y, 0, 1) * 255)) * 256;
                        //int tempColorB = (int)Math.Round(Math.Clamp(color.Z, 0, 1) * 255);
                        //tempColor = tempColorR + tempColorG + tempColorB;
                        //screen.pixels[x + y * width] = tempColor;
                        //    }
                        //}

                        Vector3 color = Trace(primaryRay, scene);
                        int tempColorR = ((int)Math.Round(Math.Clamp(color.X, 0, 1) * 255)) * 256 * 256;
                        int tempColorG = ((int)Math.Round(Math.Clamp(color.Y, 0, 1) * 255)) * 256;
                        int tempColorB = (int)Math.Round(Math.Clamp(color.Z, 0, 1) * 255);
                        int tempColor = tempColorR + tempColorG + tempColorB;
                        screen.pixels[x + y * width] = tempColor;
                    }
                }
            }
        }
        internal Vector3 Trace(Ray ray, Scene scene) //Might Move the things from scene into raytracer for easier code but isn't easier yet so wip
        {
            Vector3 color = Vector3.Zero;

            //-------------Start Searching For Closest Primitive-------------
            int primitivesCount = scene.primitives.Count;
            Intersection intersection = new Intersection(-1, null, Vector3.Zero, Vector3.Zero);

            for (int i = 0; i < primitivesCount; i++)
            {
                Primitive primitive = scene.primitives[i];
                Intersection tempIntersection = null;
                if (scene.primitives[i] is Plane)
                {
                    tempIntersection = collideRayPlane(ray, primitive as Plane);
                }
                else if (scene.primitives[i] is Sphere)
                {
                    tempIntersection = collideRaySphere(ray, primitive as Sphere);
                }
                if (tempIntersection.distance > Application.epsilon && (tempIntersection.distance < intersection.distance - Application.epsilon || intersection.distance == -1)) //If it hits something that is not itself
                {
                    intersection = tempIntersection;
                }
            }

            //-------------Ray Hit Something So Do...-------------
            if (intersection.nearestPrimitive != null)
            {
                if (intersection.nearestPrimitive.specularity == 1 && ray.numberOfBounces < 10) //Pure specular
                {
                    Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, intersection.normal) * intersection.normal;
                    reflectedVector.Normalize();
                    Ray reflectedRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);
                    color = intersection.nearestPrimitive.materialColor * Trace(reflectedRay, scene);

                    return color;
                }
                else //Phong Shading Model
                {
                    int lightsCount = scene.lights.Count();

                    for (int i = 0; i < lightsCount; i++)
                    {
                        Light light = scene.lights[i];
                        Ray shadowRay = FindShadowRay(intersection, light);
                        Vector3 intensity = scene.FindShadowRayColor(shadowRay, light);
                        float distance = shadowRay.intersectionDistance;

                        Vector3 r = -shadowRay.direction - 2 * Vector3.Dot(-shadowRay.direction, intersection.normal) * intersection.normal;
                        r.Normalize();

                        float n = 20;

                        if (intersection.nearestPrimitive.specularity != 0 && ray.numberOfBounces < 10)
                        {
                            Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, intersection.normal) * intersection.normal;
                            reflectedVector.Normalize();
                            Ray reflectedRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);
                            Vector3 tempColor = intersection.nearestPrimitive.specularColor * Trace(reflectedRay, scene);
                            color += light.rgbIntensity * (1 / (distance * distance)) * intersection.nearestPrimitive.materialColor * Math.Max(0, Vector3.Dot(intersection.normal, shadowRay.direction)) + tempColor;
                        }
                        else if (intensity != Vector3.Zero)
                        {
                            color += light.rgbIntensity * (1 / (distance * distance)) * (intersection.nearestPrimitive.materialColor * Math.Max(0, Vector3.Dot(intersection.normal, shadowRay.direction)) + intersection.nearestPrimitive.speculalColor * (float)Math.Pow(Math.Max(0, Vector3.Dot(-ray.direction, r)), n));
                        }
                    }

                    Vector3 materialsAmbientColor = intersection.nearestPrimitive.materialColor;
                    Vector3 ambientLightRadiance = new Vector3(0.05f, 0.05f, 0.05f);
                    
                    return color += materialsAmbientColor * ambientLightRadiance;
                }
                //Vector3 materialsAmbientColor = intersection.nearestPrimitive.materialColor;         // -------Verplaatst naar in de if statements zodat spiegels zwart konden worden als ze niks raaktten.
                //Vector3 ambientLightRadiance = new Vector3(0.05f, 0.05f, 0.05f);
                //
                //return color += materialsAmbientColor * ambientLightRadiance;
            }
            else
            {
                return Vector3.Zero;
            }
        }

        internal Intersection collideRayPlane(Ray ray, Plane primitive)
        {
            Vector3 pointOfIntersection = Vector3.Zero;
            Vector3 tempNormal = Vector3.Zero;
            float length = 0;
            if (Vector3.Dot(ray.direction, primitive.normal) != 0) //If the ray and plane are not parallel
            {
                pointOfIntersection = ray.origin - ray.direction * (Vector3.Dot(ray.origin - primitive.distanceToOrigin, primitive.normal) / Vector3.Dot(ray.direction, primitive.normal));
                tempNormal = primitive.normal;
                tempNormal.Normalize();
                Vector3 direction = pointOfIntersection - ray.origin;
                direction.Normalize();
                if (direction == ray.direction)
                    length = (pointOfIntersection - ray.origin).Length;
            }
            return new Intersection(length, primitive, tempNormal, pointOfIntersection);
        }

        internal Intersection collideRaySphere(Ray ray, Sphere primitive)
        {
            float length = 0;
            Vector3 pointOfIntersection = Vector3.Zero;
            Vector3 tempNormal = Vector3.Zero;
            float a = ray.direction.X * ray.direction.X + ray.direction.Y * ray.direction.Y + ray.direction.Z * ray.direction.Z;
            float b = (2 * (ray.origin.X - primitive.position.X) * ray.direction.X) + (2 * (ray.origin.Y - primitive.position.Y) * ray.direction.Y) + (2 * (ray.origin.Z - primitive.position.Z) * ray.direction.Z);
            float c = (ray.origin.X - primitive.position.X) * (ray.origin.X - primitive.position.X) + (ray.origin.Y - primitive.position.Y) * (ray.origin.Y - primitive.position.Y) + (ray.origin.Z - primitive.position.Z) * (ray.origin.Z - primitive.position.Z) - primitive.radius * primitive.radius;

            float t = 0;
            float d = b * b - 4 * a * c;
            if (d >= 0 && a >= 0) //If there are solutions
            {
                t = (float)((-b - Math.Sqrt(d)) / (2 * a));
                if (t > 0)
                {
                    length = (ray.direction * t).Length;
                    pointOfIntersection = ray.origin + ray.direction * t;
                    tempNormal = pointOfIntersection - primitive.position;
                    tempNormal.Normalize();
                }
            }
            return new Intersection(length, primitive, tempNormal, pointOfIntersection);
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

        private int TX(float X)
        {
            return (int)(X * 60 + screen.width / 2);
        }

        private int TY(float Z)
        {
            return (int)(Z * 60 + screen.height * 0.9f);
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
