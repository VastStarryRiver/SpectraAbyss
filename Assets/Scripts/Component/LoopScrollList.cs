using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;



namespace Invariable
{
    public class LoopScrollList : MonoBehaviour
    {
        private RectTransform content;
        private GameObject itemPrefab;
        private int totalCount = 0; // 总项数
        private int showCount = 0; // 可见项数
        private List<RectTransform> tranItems;
        private float itemHeight;
        private int currItemIndex = 0;
        private bool isInit = false;



        private void Update()
        {
            if (!isInit)
            {
                return;
            }

            if (content.anchoredPosition.y < currItemIndex * itemHeight)
            {
                ScrollUp();
            }
            else if (content.anchoredPosition.y > (currItemIndex + 1) * itemHeight)
            {
                ScrollDown();
            }
        }



        public void Init(int totalCount, int showCount)
        {
            if (isInit)
            {
                return;
            }

            this.totalCount = totalCount;
            this.showCount = showCount;

            itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height;
            tranItems = new List<RectTransform>();

            UpdateAllItems();

            content.sizeDelta = new Vector2(content.sizeDelta.x, itemHeight * totalCount);

            isInit = true;
        }

        private void ScrollUp()
        {
            currItemIndex--;

            if (currItemIndex < 0)
            {
                currItemIndex = totalCount - 1;
            }

            int itemIndex = 0;

            if (currItemIndex > 0)
            {
                itemIndex = currItemIndex % totalCount;
            }

            UpdateItem(itemIndex, currItemIndex + 1);
        }

        private void ScrollDown()
        {
            currItemIndex++;

            if (currItemIndex >= totalCount)
            {
                currItemIndex = 0;
            }

            int itemIndex = 0;

            if (currItemIndex > 0)
            {
                itemIndex = currItemIndex % totalCount;
            }

            UpdateItem(itemIndex, currItemIndex + 1);
        }

        private void UpdateAllItems()
        {
            for (int i = 0; i < showCount; i++)
            {
                if (tranItems[i] == null)
                {
                    tranItems.Add(Instantiate(itemPrefab, content).GetComponent<RectTransform>());
                }

                int dataIndex = (currItemIndex + i) % totalCount + 1;

                UpdateItem(i, dataIndex);
            }
        }

        private void UpdateItem(int itemIndex, int dataIndex)
        {
            tranItems[itemIndex].anchoredPosition = new Vector2(0, -1 * (dataIndex - 1) * itemHeight);
            tranItems[itemIndex].GetComponentInChildren<Text>().text = "Item " + dataIndex;
        }
    }
}