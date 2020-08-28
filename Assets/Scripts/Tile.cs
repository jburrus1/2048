using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile
{
    public GameObject obj;
    public int value;
    private TextMesh text;
    private bool isDeleted;

    private bool isTracking;
    private Tile trackedTile;
    private int colorIndex;
    // Start is called before the first frame update
    public Tile()
    {
        isTracking = false;
        var prefab = Resources.Load("Prefabs/Tile") as GameObject;
        obj = MonoBehaviour.Instantiate(prefab);
        text = obj.GetComponentInChildren<TextMesh>();
        var value = UnityEngine.Random.Range(0,1f) < 0.9 ? 2 : 4;
        SetValue(value);
    }

    public void Increment()
    {
        if(colorIndex <= 11)
            colorIndex++;
        SetValue(value * 2);
    }

    private void SetValue(int value)
    {
        if(value == 2)
        {
            colorIndex = 0;
        }
        else if(value == 4)
        {
            colorIndex = 1;
        }
        this.value = value;
    }

    private void setColor()
    {
        obj.GetComponentInChildren<SpriteRenderer>().color = Color.HSVToRGB((24f*colorIndex)/360f,.6f,1);
    }

    public void UpdateValue()
    {
        text.text = value.ToString();

        setColor();
    }

    public void SetPosition(Vector3 position)
    {
        MonoBehaviour.print("Setting position to " + position.ToString());
        obj.transform.SetPositionAndRotation(position, new Quaternion(0,0,0,0));
    }

    public void SetDeleted()
    {
        isDeleted = true;
    }

    public bool IsDeleted()
    {
        return isDeleted;
    }

    public bool IsTracking()
    {
        return isTracking;
    }

    public void SetTracking(Tile tile)
    {
        trackedTile = tile;
        isTracking = true;
    }

    public void ResolveTracking(Board b, int x, int y)
    {
        b.UpdateMoveLocation(trackedTile, new Vector2(x, y));
    }

    public void clearTracking()
    {
        isTracking = false;
    }
}
