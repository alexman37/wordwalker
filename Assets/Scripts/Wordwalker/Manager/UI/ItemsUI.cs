using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsUI : MonoBehaviour
{
    public GameObject itemsMenu;
    bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toggleItemsMenu()
    {
        itemsMenu.SetActive(!isActive);
        isActive = !isActive;
    }
}
