﻿using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using Project;
using g3;
using System;

namespace Virgis
{

    public class MeshLayer : VirgisLayer<GeographyCollection, List<SimpleMesh>>
    {
        // The prefab for the data points to be instantiated
        public Material material;
        public GameObject handle;
        public List<GameObject> meshes;
        public Material HandleMaterial;

        private Dictionary<string, Unit> symbology;
        private Material mainMat;
        private Material selectedMat;

        private async Task<SimpleMeshBuilder> loadObj(string filename)
        {
            StandardMeshReader reader = new StandardMeshReader();
            reader.MeshBuilder = new SimpleMeshBuilder();
            try
            {
                IOReadResult result = reader.Read(filename, new ReadOptions());
            }
            catch (Exception e) when (
                e is UnauthorizedAccessException ||
                e is DirectoryNotFoundException ||
                e is FileNotFoundException ||
                e is NotSupportedException
                )
            {
                Debug.LogError("Failed to Load" + filename + " : " + e.ToString());
            }
            return reader.MeshBuilder as SimpleMeshBuilder;
        }

        protected override async Task _init(GeographyCollection layer)
        {
            SimpleMeshBuilder meshes = await loadObj(layer.Source);
            features = meshes.Meshes;
            symbology = layer.Properties.Units;

            Color col = symbology.ContainsKey("point") ? (Color)symbology["point"].Color : Color.white;
            Color sel = symbology.ContainsKey("point") ? new Color(1 - col.r, 1 - col.g, 1 - col.b, col.a) : Color.red;
            mainMat = Instantiate(HandleMaterial);
            mainMat.SetColor("_BaseColor", col);
            selectedMat = Instantiate(HandleMaterial);
            selectedMat.SetColor("_BaseColor", sel);
        }

        protected override VirgisFeature _addFeature(Vector3[] geometry)
        {
            throw new System.NotImplementedException();
        }
        protected override void _draw()
        {
            transform.position = layer.Position.Coordinates.Vector3();
            transform.Translate(AppState.instance.map.transform.TransformVector((Vector3)layer.Transform.Position ));
            Dictionary<string, Unit> symbology = layer.Properties.Units;
            meshes = new List<GameObject>();

            foreach (SimpleMesh simpleMesh in features)
            {
                GameObject meshGameObject = new GameObject();
                MeshFilter mf = meshGameObject.AddComponent<MeshFilter>();
                MeshRenderer renderer = meshGameObject.AddComponent<MeshRenderer>();
                renderer.material = material;
                meshGameObject.transform.localScale = AppState.instance.map.transform.localScale;
                meshGameObject.transform.parent = transform;
                meshGameObject.transform.localPosition = Vector3.zero;
                mf.mesh = simpleMesh.ToMesh();
                meshes.Add(meshGameObject);
            }
            transform.rotation = layer.Transform.Rotate;
            transform.localScale = layer.Transform.Scale;
            GameObject centreHandle = Instantiate(handle, transform.position, Quaternion.identity);
            centreHandle.transform.localScale = AppState.instance.map.transform.TransformVector((Vector3) symbology["handle"].Transform.Scale);
            centreHandle.GetComponent<Datapoint>().SetMaterial(mainMat, selectedMat);
            centreHandle.transform.parent = transform;

        }

        public override void Translate(MoveArgs args)
        {
            foreach (GameObject mesh in meshes)
            {
                if (args.translate != Vector3.zero) transform.Translate(args.translate, Space.World);
                changed = true;
            }
        }

        /// https://answers.unity.com/questions/14170/scaling-an-object-from-a-different-center.html
        public override void MoveAxis(MoveArgs args)
        {
            if (args.translate != Vector3.zero) transform.Translate(args.translate, Space.World);
            args.rotate.ToAngleAxis(out float angle, out Vector3 axis);
            transform.RotateAround(args.pos, axis, angle);
            Vector3 A = transform.localPosition;
            Vector3 B = transform.parent.InverseTransformPoint(args.pos);
            Vector3 C = A - B;
            float RS = args.scale;
            Vector3 FP = B + C * RS;
            if (FP.magnitude < float.MaxValue)
            {
                transform.localScale = transform.localScale * RS;
                transform.localPosition = FP;
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform T = transform.GetChild(i);
                    if (T.GetComponent<Datapoint>() != null)
                    {
                        T.localScale /= RS;
                    }
                }
            }
            changed = true;
        }

        protected override void _checkpoint() { }

        protected override Task _save()
        {
            layer.Position = transform.position.ToPoint();
            layer.Transform.Position = Vector3.zero;
            layer.Transform.Rotate = transform.rotation;
            layer.Transform.Scale = transform.localScale;
            return Task.CompletedTask;
        }
    }
}

