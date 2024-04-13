using System;

using UnityEngine;

namespace Assets.Scripts.Prefabs.Menues.Book
{
    public class PageBehaviour : MonoBehaviour
    {
        public String indexName;
        private BookBehaviour bookBehaviour;

        public void SetPageBehaviour(BookBehaviour bookBehaviour)
        {
            this.bookBehaviour = bookBehaviour;
        }
        public void OpenThisPage()
        {
            bookBehaviour.OpenPage(this);
        }
    }
}
