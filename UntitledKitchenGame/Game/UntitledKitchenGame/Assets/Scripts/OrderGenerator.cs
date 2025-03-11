using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OrderGenerator : MonoBehaviour
{
    //this item will return a set amount of orders and we can modify it to be either focused on orders or all items.
    //for now we will make this able to set the UI elements to their respective texts.
    public Canvas orderCanvas;
    public List<TextMeshProUGUI> orderText;
    public List<Image> orderImages;

    public ItemCollection itemDatabase;

    public List<FoodItem> foodItems;
    //being a prefab means the object cannot be scene referenced so we gots to find a score manager
    public ScoreManager scoreManage;
    public int itemMax = 3;

    public type onlySelect = new type();
    //maybe we could have the variables change as well depending on what kind of object it is?
    public enum type
    {
        Unlisted,
        Meat,
        Grain,
        Veggie,
        Fish
    }
    public void Start()
    {
        GameObject tempObj;
        tempObj = GameObject.Find("ScoreManager");
        scoreManage = tempObj.GetComponent<ScoreManager>();
        GenerateOrder();
    }
    private void Update()
    {
        PopulateUI();
    }
    public void GenerateOrder()
    {
        PopulateUI();
        PopulateOrder();
    }
    //no longer needed but why not keep them
    [ContextMenu("Populate UI")]
    void PopulateUI()
    {
        //need to add null edge cases
        //or my index is getting out of bounds and so every for loop needs a -1
        if (orderCanvas == null || orderText == null || orderImages ==null) 
        { 
            orderCanvas = this.GetComponentInChildren<Canvas>();
            Image[] tempImageList = this.GetComponentsInChildren<Image>();
            for (int i = 0; i < itemMax; i++)
            {
                orderImages.Add(tempImageList[i]);
            }
            TextMeshProUGUI[] tempList = this.GetComponentsInChildren<TextMeshProUGUI>();

            for (int i = 0; i < itemMax; i++)
            {
                orderText.Add(tempList[i]);
            }

            
        }
        
        for(int i = 0; i < orderText.Count-1; i++)
        {
            if (foodItems[i] == null && orderText[i] != null) { orderText[i].gameObject.SetActive(false); } 
            else { orderText[i].gameObject.SetActive(true); 
                orderText[i].text = foodItems[i].displayName; }
            
        }
        for (int i = 0; i < orderImages.Count - 1; i++)
        {
            if (foodItems[i] == null) { orderImages[i].gameObject.SetActive(false); }
            else
            {
                orderImages[i].gameObject.SetActive(true);
                orderImages[i].sprite = foodItems[i].displayImage;
            }

        }
    }
    // Start is called before the first frame update
    [ContextMenu ("Populate Order")]
    void PopulateOrder()
    {
        //I want if at max to exchange the items for new ones
        //changing the item max must also cull the list to less items
        if (foodItems.Count >= itemMax)
        {
            foodItems.Clear ();
        }

        for (int i = 0; i < itemMax; i++)
        {
            var selectedItem = SelectRandomItem();
            
            foodItems.Add(selectedItem);
        }
        var data = new ItemSubmitData()
        {
            requiredObject = foodItems[0].name,
            requiredObject1 = foodItems[1].name,
            requiredObject2 = foodItems[2].name,
        };
        TelemetryLogger.Log(this, "order generated", data);

    }
    private void OnTriggerEnter(Collider other)
    {
        //currently it is not grabbing the objects collider
        //Debug.Log(other.name);
        CheckItemSubmission(other.gameObject);
    }
    //please convert the object so it is visible from teh save file.
    [System.Serializable]
    public struct ItemSubmitData
    {
        public string objectName;
        public string requiredObject1;
        public string requiredObject2;
        public string requiredObject;
    }
    public void CheckItemSubmission(GameObject recievedObject)
    {
        //TelemetryLogger.Log(this, "Item Submitted");
        //arugably we could take
        foreach(FoodItem item in foodItems)
        {
            if(item.foodObject = recievedObject)
            {
                var data = new ItemSubmitData()
                {
                    objectName = recievedObject.name
                };
                TelemetryLogger.Log(this, "Correct Submission", data);
                scoreManage.score += item.pointValue;
                foodItems.Remove(item);
            }
            else
            {
                var data = new ItemSubmitData()
                {
                    objectName = recievedObject.name
                };
                TelemetryLogger.Log(this, "Incorrect Submission", data);
            }
        }
        
    }
    public FoodItem SelectRandomItem()
    {
        //not looking for an index looking for the rarity.
        int randomItem = Random.Range(0, 101);
        //Debug.Log(randomItem);
        //found the little issue
        foreach (FoodItem item in itemDatabase.Items)
        {
            //seem to be having an issue with what item is selected.
            //seems to grab the small items first? question about how it operates is required for the veggies as it seemed to miss onions and lettuce.

            //in specific cases items can simply not appear so forcing an item to be returned should be a good moment of functionality but rn we can ignore it.
            if (randomItem <= item.rarity)
            {
                if (item.type.ToString() == onlySelect.ToString())
                {
                    //Debug.Log(item.name);
                    return item;

                }
                else if (onlySelect == type.Unlisted)
                {
                    //Debug.Log(item.name);
                    return item;
                }
            }
        }
        return null;
    }
}
