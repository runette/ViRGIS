﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Feature;

public class LineLayer : MonoBehaviour
{
    // Name of the input file, no extension
 public string inputfile;
 
  // The prefab for the data points to be instantiated
 public GameObject LinePrefab;

[SerializeField]
 private AbstractMap _map;


    // Start is called before the first frame update
    void Start()
    {
    // get geojson data

    GeoJsonReader geoJsonReader= new GeoJsonReader();
    geoJsonReader.Load(inputfile);
    FeatureCollection myFC = geoJsonReader.getFeatureCollection();
    
    _map.Initialize(new Vector2d(51.282433, 1.379470), 15);

    foreach (Feature feature in myFC.Features) 
        {
        // Get the geometry
        Point geometry = feature.Geometry as Point;
        IDictionary<string, object> properties = feature.Properties;
        string name = (string)properties["name"];
        string type = (string)properties["type"];


        IPosition in_position = geometry.Coordinates;
        Vector2d _location = new Vector2d(in_position.Latitude, in_position.Longitude);

        //float y = (float)in_position.Altitude;
        float y = _map.QueryElevationInMetersAt(_location);
        
        //instantiate the prefab with coordinates defined above
        GameObject dataPoint = Instantiate(LinePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        dataPoint.transform.parent = gameObject.transform;
        GameObject labelObject = new GameObject();
        labelObject.transform.parent = dataPoint.transform;
        TextMesh labelMesh = labelObject.AddComponent(typeof(TextMesh)) as TextMesh;
        labelMesh.text = name + "," +type;
        //Set the color
        dataPoint.SendMessage("SetColor", Color.blue);
        Vector3 scaleChange = new Vector3(1, 1, 1);
        dataPoint.transform.localScale = scaleChange;
        Vector2d pos = Conversions.GeoToWorldPosition(_location, _map.CenterMercator, _map.WorldRelativeScale);
        dataPoint.transform.position = new Vector3((float)pos.x, y*_map.WorldRelativeScale, (float)pos.y); 
        //Debug.Log(_location.x); 
        //Debug.Log(pos.x);
        };
    }

    void Update() {
        
    }  
}
