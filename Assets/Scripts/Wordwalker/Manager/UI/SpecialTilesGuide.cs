using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTilesGuide : WidgetPopup
{
    // Start is called before the first frame update
    void Start()
    {
        base.Setup();
    }

    public void openPopup()
    {
        Tile.toggleCanClickTiles(false);
        base.openWidgetPopup();
    }

    public void closePopup()
    {
        Tile.toggleCanClickTiles(true);
        base.closeWidgetPopup();
    }
}
