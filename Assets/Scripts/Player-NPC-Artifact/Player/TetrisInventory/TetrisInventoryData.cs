using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory
{
    [Serializable]
    public class TetrisInventoryData
    {

        public Vector2Int gridSize { get; private set; }

        private TetrisInventoryItem[,] inventoryGrid;

        public List<TetrisInventoryItem> inventoryItems = new List<TetrisInventoryItem>();


        public TetrisInventoryData(Vector2Int gridSize)
        {
            this.gridSize = gridSize;
            inventoryGrid = new TetrisInventoryItem[gridSize.x, gridSize.y];
        }

        public List<Artifact> GetArtifacts()
        {
            return inventoryItems.Select(x => x.itemData).ToList();
        }

        public bool SlotToItem(Vector2Int slot, out TetrisInventoryItem item)
        {
            item = inventoryGrid[slot.x, slot.y];

            return item != null;
        }

        public bool CanPlace(Vector2Int slot, TetrisInventoryItem item)
        {
            foreach (var itemSlot in item.RotatedSlots(item.rotation))
            {
                int x = slot.x + itemSlot.x;
                int y = slot.y + itemSlot.y;

                if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
                {
                    return false;
                }
                if (inventoryGrid[x, y] != null)
                {
                    return false;
                }
            }

            return true;
        }

        public void AddItem(Vector2Int slot, TetrisInventoryItem item)
        {

            foreach (var itemSlot in item.RotatedSlots(item.rotation))
            {
                inventoryGrid[slot.x + itemSlot.x, slot.y + itemSlot.y] = item;
            }

            inventoryItems.Add(item);
            item.slot = slot;
        }

        public void RemoveItem(TetrisInventoryItem item)
        {

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (inventoryGrid[x, y] == item)
                    {
                        inventoryGrid[x, y] = null;
                    }
                }
            }

            inventoryItems.Remove(item);
        }

        public bool FindSlotForItem(TetrisInventoryItem item, out Vector2Int foundSlot)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {

                    Vector2Int slot = new Vector2Int(x, y);

                    if (CanPlace(slot, item))
                    {
                        foundSlot = slot;
                        return true;
                    }

                }
            }

            foundSlot = Vector2Int.zero;
            return false;
        }

    }
}
