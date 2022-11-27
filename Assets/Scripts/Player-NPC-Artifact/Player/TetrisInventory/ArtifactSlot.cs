using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// script with the items in the inventory, drap and drop functions, reescaling based on <c><artifact/c> size. <br/>
/// This script is present in each collected <c>artifact/c>.
/// </summary>
public class ArtifactSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler
{
    public Artifact artifact;

    public Vector2 startPosition;
    public Vector2 oldPosition;
    public Sprite icon;

    private Vector2 size; //slot cell size 
    private bool isClicked = false;
    
    private TetrisSlot slots;
    
    void Awake()
    {
        GameObject.FindGameObjectWithTag("UI").GetComponent<ChangeUI>().ChangeStateInventory();
        size = GameObject.Find("GridPanel").GetComponent<BetterGridLayout>().GetCellSize();
    }


    void Start()
    {
        #region Rescaling
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, artifact.Size.y * size.y);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, artifact.Size.x * size.x);
        icon = artifact.InventoryIcon;
        transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = icon;

        /*foreach (RectTransform child in transform)
        {
            child.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, artifact.Size.y * child.rect.height);
            child.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, artifact.Size.x * child.rect.width);
            
            foreach (RectTransform iconChild in child)
            {
                iconChild.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, artifact.Size.y * iconChild.rect.height);
                iconChild.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, artifact.Size.x * iconChild.rect.width);
                iconChild.localPosition = new Vector2(child.localPosition.x + child.rect.width / 2, child.localPosition.y + child.rect.height / 2 * -1f);
            }

        }*/
        #endregion

        slots = FindObjectOfType<TetrisSlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        oldPosition = transform.GetComponent<RectTransform>().anchoredPosition;

        GetComponent<CanvasGroup>().blocksRaycasts = false; // disable registering hit on artifact
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        //allow the intersection between old pos and new pos.
        for (int i = 0; i < artifact.Size.y; i++)
            for (int j = 0; j < artifact.Size.x; j++)
                slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 0;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true; // able registering hit on artifact
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 finalPos = GetComponent<RectTransform>().anchoredPosition; //position that the artifact was dropped on canvas

            Vector2 finalSlot;
            finalSlot.x = Mathf.Floor(finalPos.x / size.x); //which x slot it is
            finalSlot.y = Mathf.Floor(-finalPos.y / size.y); //which y slot it is
            Debug.Log("Slot " + finalSlot);

            if (((int)(finalSlot.x) + (int)(artifact.Size.x) - 1) < slots.maxGridX && ((int)(finalSlot.y) + (int)(artifact.Size.y) - 1) < slots.maxGridY && ((int)(finalSlot.x)) >= 0 && (int)finalSlot.y >= 0) // test if artifact is inside slot area
            {
                List<Vector2> newPosItem = new List<Vector2>(); //new artifact position in bag
                bool fit = false;
                Debug.Log("max Y " + slots.maxGridY + "  " + ((int)(finalSlot.y) + (int)(artifact.Size.y) - 1));
                Debug.Log("max X"  + slots.maxGridX + "  " + ((int)(finalSlot.x) + (int)(artifact.Size.x) - 1));

                for (int sizeY = 0; sizeY < artifact.Size.y; sizeY++)
                {
                    for (int sizeX = 0; sizeX < artifact.Size.x; sizeX++)
                    {
                        if (slots.grid[(int)finalSlot.x + sizeX, (int)finalSlot.y + sizeY] != 1)
                        {
                            Vector2 pos;
                            pos.x = (int)finalSlot.x + sizeX;
                            pos.y = (int)finalSlot.y + sizeY;
                            newPosItem.Add(pos);
                            fit = true;
                        }
                        else
                        {
                            fit = false;
                            Debug.Log("depart " + startPosition);

                            this.transform.GetComponent<RectTransform>().anchoredPosition = oldPosition; //back to old pos
                            sizeX = (int)artifact.Size.x;
                            sizeY = (int)artifact.Size.y;
                            newPosItem.Clear();

                        }
                    }
                }
                if (fit)
                { //delete old artifact position in bag
                    for (int i = 0; i < artifact.Size.y; i++) //through artifact Y
                        for (int j = 0; j < artifact.Size.x; j++) //through artifact X
                            slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 0; //clean old pos

                    for (int i = 0; i < newPosItem.Count; i++)
                        slots.grid[(int)newPosItem[i].x, (int)newPosItem[i].y] = 1; // add new pos

                    this.startPosition = newPosItem[0]; // set new start position
                    transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(newPosItem[0].x * size.x, -newPosItem[0].y * size.y);
                    Debug.Log("Position: " + transform.GetComponent<RectTransform>().anchoredPosition);
                }
                else
                    for (int i = 0; i < artifact.Size.y; i++) //through artifact Y
                        for (int j = 0; j < artifact.Size.x; j++) //through artifact X
                            slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 1; //back to position 1;
            }
            else // out of index, back to the old pos
            { 
                this.transform.GetComponent<RectTransform>().anchoredPosition = oldPosition;
            }
        }
        else
        {

            /*PlayerController player;
            player = FindObjectOfType<PlayerController>();

            TetrisListItens itenInGame; // list of items prefab to could be instantiated when dropping artifact.
            itenInGame = FindObjectOfType<TetrisListItens>();

            for (int t = 0; t < itenInGame.prefabs.Length; t++)
            {
                if (itenInGame.itens[t].itemName == artifact.itemName)
                {
                    Instantiate(itenInGame.prefabs[t].gameObject, new Vector2(player.transform.position.x + Random.Range(-1.5f, 1.5f), player.transform.position.y + Random.Range(-1.5f, 1.5f)), Quaternion.identity); //dropa o artifact

                    Destroy(this.gameObject);
                    break;
                }

            }

        }*/
            GetComponent<CanvasGroup>().blocksRaycasts = true; //register hit on artifact again
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    private void Update()
    {
        if (isClicked && Input.GetKeyDown(KeyCode.R))
        {
            print("turned");
            float tempXSize = size.x;
            size.x = size.y;
            size.y = tempXSize;
        }
        else if(isClicked && Input.GetKeyDown(KeyCode.Delete))
        {
            for (int i = 0; i < artifact.Size.y; i++) //through Y size of artifact
                for (int j = 0; j < artifact.Size.x; j++) //through X size of artifact
                    slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 0; //clean the old artifact position
            Destroy(this.gameObject); //artifact drop
        }
    }

    public void Click()
    {
        print("click");
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isClicked = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isClicked = false;
        ChangeUI uiChanger = FindObjectOfType<ChangeUI>();

        if(uiChanger.IsDescriptionSimilar(artifact.Title, artifact.Description, artifact.Effect, artifact.EffectDescription))
            uiChanger.ChangeDescription("Nom de l'art�fact", "", "Effets", "");
        else
            uiChanger.ChangeDescription(artifact.Title, artifact.Description, artifact.Effect, artifact.EffectDescription);

    }

    
}