using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// script with the items in the inventory, drap and drop functions, reescaling based on <c><artifact/c> size. <br/>
/// This script is present in each collected <c>artifact/c>.
/// </summary>
public class ArtifactSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Artifact artifact;

    public Vector2 startPosition;
    public Vector2 oldPosition;
    public Sprite icon;

    public  Vector2 size; //slot cell size
    private Vector2 oldSize;
    
    private bool isRotated = false;
    private bool isClicked = false;
    private bool isDragged = false;
    
    private TetrisSlot slots;
    
    void Awake()
    {
        if(! GameObject.FindGameObjectWithTag("UI").GetComponent<ChangeUI>().GetIsInventoryOpen())
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
        #endregion

        slots = FindObjectOfType<TetrisSlot>();
    }

    /// <summary>
    /// What happen when the player start dragging
    /// </summary>
    /// <param name="eventData">The event</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        isRotated = false;
        isDragged = true;
        oldPosition = transform.GetComponent<RectTransform>().anchoredPosition;

        GetComponent<CanvasGroup>().blocksRaycasts = false; // disable registering hit on artifact
    }

    /// <summary>
    /// What happen when the player is dragging (only works when there's a movement)
    /// </summary>
    /// <param name="eventData">The event</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        transform.position = eventData.position;
        //allow the intersection between old pos and new pos.
        if(isRotated)
            for (int i = 0; i < artifact.Size.y; i++)
                for (int j = 0; j < artifact.Size.x; j++)
                    slots.grid[(int)startPosition.x + i, (int)startPosition.y + j] = 0;
        else
            for (int i = 0; i < artifact.Size.y; i++)
                for (int j = 0; j < artifact.Size.x; j++)
                    slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 0;
    }

    /// <summary>
    /// What happen when the player stop dragging
    /// </summary>
    /// <param name="eventData">The event</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

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

                if (fit)
                { //delete old artifact position in bag
                    print(isRotated);
                    if(isRotated)
                    {
                        Rotate();
                        for (int i = 0; i < artifact.Size.y; i++) //through artifact Y
                            for (int j = 0; j < artifact.Size.x; j++) //through artifact X
                                slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 0; //clean old pos

                        Rotate();
                        for (int i = 0; i < newPosItem.Count; i++)
                            slots.grid[(int)newPosItem[i].x, (int)newPosItem[i].y] = 1; // add new pos

                        this.startPosition = newPosItem[0]; // set new start position
                        transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(newPosItem[0].x * size.x, -newPosItem[0].y * size.y);
                    }
                    else
                    {
                        for (int i = 0; i < artifact.Size.y; i++) //through artifact Y
                            for (int j = 0; j < artifact.Size.x; j++) //through artifact X
                                slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 0; //clean old pos

                        for (int i = 0; i < newPosItem.Count; i++)
                            slots.grid[(int)newPosItem[i].x, (int)newPosItem[i].y] = 1; // add new pos

                        this.startPosition = newPosItem[0]; // set new start position
                        transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(newPosItem[0].x * size.x, -newPosItem[0].y * size.y);
                    }
                    
                    //Debug.Log("Position: " + transform.GetComponent<RectTransform>().anchoredPosition);
                }
                else
                {
                    if (isRotated)
                    {
                        Rotate();
                        for (int i = 0; i < oldSize.y; i++) //through artifact Y
                            for (int j = 0; j < oldSize.x; j++) //through artifact X
                                slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 1; //back to position 1;
                    }
                    else
                        for (int i = 0; i < oldSize.y; i++) //through artifact Y
                            for (int j = 0; j < oldSize.x; j++) //through artifact X
                                slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 1; //back to position 1;
                }
            }
            else // out of index, back to the old pos
            {
                if (isRotated)
                    Rotate();
                this.transform.GetComponent<RectTransform>().anchoredPosition = oldPosition;
            }
        }
        else
        {
            #region old thing that can be usefull later TODO TO BE DELETED AT THE END
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

            }*/
            #endregion
            GetComponent<CanvasGroup>().blocksRaycasts = true; //register hit on artifact again
        }
        isDragged = false;
    }

    /// <summary>
    /// This Update will handle all the keyboard clicks from the player when he is dragging an artifact
    /// </summary>
    private void Update()
    {
        if (isClicked && (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1)))
            Rotate();
        else if (isClicked && Input.GetKeyDown(KeyCode.Delete))
        {
            for (int i = 0; i < artifact.Size.y; i++) //through Y size of artifact
                for (int j = 0; j < artifact.Size.x; j++) //through X size of artifact
                    slots.grid[(int)startPosition.x + j, (int)startPosition.y + i] = 0; //clean the old artifact position
            Destroy(this.gameObject); //artifact drop
        }
    }

    /// <summary>
    /// What happen when the player press M1
    /// </summary>
    /// <param name="eventData">The event</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        isClicked = true;
    }

    /// <summary>
    /// What happen when the player release the M1
    /// </summary>
    /// <param name="eventData">The event</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        isClicked = false;
        ChangeUI uiChanger = FindObjectOfType<ChangeUI>();

        if (!isDragged)
        {
            if (uiChanger.IsDescriptionSimilar(artifact.Title, artifact.Description, artifact.Effect, artifact.EffectDescription))
                uiChanger.ChangeDescription("Nom de l'art�fact", "", "Effets", "");
            else
                uiChanger.ChangeDescription(artifact.Title, artifact.Description, artifact.Effect, artifact.EffectDescription, artifact.SkillBarIcon);
        }

    }

    /// <summary>
    /// This function is used to rotate the size of the <c>Artifact</c> itself and the <c>RectTranform</c> of it
    /// </summary>
    private void Rotate()
    {
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, artifact.Size.x * size.y);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, artifact.Size.y * size.x);
        oldSize = artifact.Size;
        artifact.Size = new Vector2Int(artifact.Size.y, artifact.Size.x);
        isRotated = !isRotated;
    }
}