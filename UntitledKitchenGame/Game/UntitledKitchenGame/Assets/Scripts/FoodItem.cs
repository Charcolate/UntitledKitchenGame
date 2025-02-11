using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewItem.asset", menuName = "Data/Food Item")]
public class FoodItem : ScriptableObject
{
    public string displayName;
    public Sprite displayImage;
    //[CustomPreview(Mesh.AcquireReadOnlyMeshData())] hopefully something works
    public Mesh mesh;

    public FoodType type;

    public int pointValue;
    public int rarity;
    public int stages;

}
