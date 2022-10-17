using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisSlot : MonoBehaviour
{
    //script of the inventory matrix to add/remove tetris items
    #region Singleton
    public static TetrisSlot instanceSlot;

    void Awake()
    {
        if (instanceSlot != null)
        {
            Debug.LogWarning("More than one Tetris inventory");
            return;
        }
        instanceSlot = this;
    }
    #endregion

    public int[,] grid; //2 dimensions

    public Inventory playerInventory;
    public List<ArtifactSlot> ArtifactInBag = new List<ArtifactSlot>();

    public int maxGridX;
    public int maxGridY;

    public ArtifactSlot prefabSlot; // item prefab
    Vector2 cellSize = new Vector2(34f, 34f); //slot cell size 

    List<Vector2> posItemNaBag = new List<Vector2>(); // new item pos in bag matrix

    void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();

        maxGridX = playerInventory.SizeX;
        maxGridY = playerInventory.SizeY;

        grid = new int[maxGridX, maxGridY]; // matrix of bag size
    }

    public bool addInFirstSpace(Artifact item)
    {
        int contX = item.SizeX; //size of item in x
        int contY = item.SizeY; //size of item in y

        for (int i = 0; i < maxGridX; i++)//bag in X
        {
            for (int j = 0; j < maxGridY; j++) //bag in Y
            {
                if (posItemNaBag.Count != (contX * contY)) // if false, the item fit the bag
                {
                    //for each x,y position (i,j), test if item fits
                    for (int sizeY = 0; sizeY < contY; sizeY++) // item size in Y
                    {
                        for (int sizeX = 0; sizeX < contX; sizeX++)//item size in X
                        {
                            if ((i + sizeX) < maxGridX && (j + sizeY) < maxGridY && grid[i + sizeX, j + sizeY] != 1)//inside of index
                            {
                                Vector2 pos;
                                pos.x = i + sizeX;
                                pos.y = j + sizeY;
                                posItemNaBag.Add(pos);
                            }
                            else
                            {
                                sizeX = contX;
                                sizeY = contY;
                                posItemNaBag.Clear();
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        if (posItemNaBag.Count == (contX * contY)) // if item already in bag
        {
            ArtifactSlot myArtifact = Instantiate(prefabSlot);
            myArtifact.startPosition = new Vector2(posItemNaBag[0].x, posItemNaBag[0].y); //first position
            myArtifact.artifact = item; // get item
            myArtifact.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //change anchor position
            myArtifact.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
            myArtifact.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
            myArtifact.transform.SetParent(this.GetComponent<RectTransform>(), false);
            myArtifact.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            myArtifact.GetComponent<RectTransform>().anchoredPosition = new Vector2(myArtifact.startPosition.x * cellSize.x, -myArtifact.startPosition.y * cellSize.y);
            ArtifactInBag.Add(myArtifact);

            for (int k = 0; k < posItemNaBag.Count; k++) //upgrade matrix
            {
                int posToAddX = (int)posItemNaBag[k].x;
                int posToAddY = (int)posItemNaBag[k].y;
                grid[posToAddX, posToAddY] = 1;
            }
            posItemNaBag.Clear();
            Debug.Log("COunt: " + ArtifactInBag.Count);
            return true;
        }
        return false;
    }
}
