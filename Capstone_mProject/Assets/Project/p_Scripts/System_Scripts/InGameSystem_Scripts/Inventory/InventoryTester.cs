using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    public Inventory inventory;

    public ItemData[] itemDataArray;

    void Start()
    {
        // if (itemDataArray?.Length > 0)
        // {
        //     for (int i = 0; i < itemDataArray.Length; i++)
        //     {
        //         inventory.Add(itemDataArray[i], 3);

        //         if (itemDataArray[i] is CountableItemData)
        //             inventory.Add(itemDataArray[i], 255);
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (itemDataArray[0] is CountableItemData)
                inventory.Add(itemDataArray[0], 80);
            if (itemDataArray[1] is CountableItemData)
                inventory.Add(itemDataArray[1], 8);
        }
    }
}
