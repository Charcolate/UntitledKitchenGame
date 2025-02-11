using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemImporter.Asset", menuName = "Data/Item Importer")]
public class ItemImporter : ScriptableObject
{
    [Tooltip("Path to Excel file to import. Use forward slashes")]
    public string excelFilePath = "Editor/Items.xlsx";


    [ContextMenu("Import")]
    public void Import()
    {
        //TODO: call the function somehow, maybe have a button in the inspector (stretch goal but feasible)
        var excel = new ExcelImporter(excelFilePath);
        var items = DataHelper.GetAllAssetsOfType<FoodItem>();

        ImportItems("Food", excel, items);

        Debug.Log("Finished Importing items from excel");
    }
    void ImportItems(string category,ExcelImporter excel, Dictionary<string,FoodItem> items)
    {
        //this did it apparently
        var itemCollectionObject = DataHelper.GetAllAssetsOfType<ItemCollection>();
        var itemCollectionAsset = DataHelper.GetOrCreateAsset("ItemCollection", itemCollectionObject, "Editor");

        //how can i restrict where these items are spawned
        if (!excel.TryGetTable(category, out var table))
        {
            Debug.LogError($"Could not find {category} table in {excelFilePath}.");
            return;
        }

        for (int row = 1; row <= table.RowCount; row++)
        {
            string name = table.GetValue<string>(row, "Name");
            if (string.IsNullOrWhiteSpace(name)) { continue; }
            //continue is basically a skip for empty rows
            var item = DataHelper.GetOrCreateAsset(name, items, category);

            if (string.IsNullOrWhiteSpace(item.displayName))
                item.displayName = name;

            if (table.TryGetEnum<FoodType>(row, "Type", out var type))
                item.type = type;
            //updating the items asset cost by grabbbing the value from the excel table in the row in the for loop, getting the info in the Cost column
            item.pointValue = table.GetValue<int>(row, "Points");

            item.rarity = table.GetValue<int>(row, "Rarity");
            //this might need to be modified to have a more restricted and changeable tag enum data thingy
            item.stages = table.GetValue<int>(row, "Stages");

            //it seems to not work if the collection doe snot exist.
            if (!itemCollectionAsset.Items.Contains(item))
            {
                itemCollectionAsset.Items.Add(item);
            }

        }

         
    }
}
