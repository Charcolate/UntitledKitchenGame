using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ScheduledActionsManager : MonoBehaviour
{
    [SerializeField] private List<ScheduledAction> _scheduledAction = new List<ScheduledAction>();

    // Start is called before the first frame update
    void Start()
    {
        RemoveOrders();
    }

    public void RemoveOrders()
    {
        _scheduledAction.RemoveAt(0);
    }
}

[System.Serializable]
public class ScheduledAction
{
    
    public string ActionName;
    public string ActionId;
    public Action Action;
}

public class Action : ScriptableObject {};