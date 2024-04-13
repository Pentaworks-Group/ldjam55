using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class JsonEditorManagerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private JsonEditorBaseBehaviour editorTemplate;


        [SerializeField]
        private GameObject closeButton;
        [SerializeField]
        private Button saveAndCloseButton;


        [SerializeField]
        private List<GameObject> hideWhenOpeningEditor;

        private List<JsonEditorBaseBehaviour> openEditors = new List<JsonEditorBaseBehaviour>();
        private List<Action> onEditorClosed = new List<Action>();
        private List<Action<JObject>> onEditorSave = new List<Action<JObject>>();





        public void OpenEditor(object objectToOpen, JObject jsonObject = null, Action<JObject> createdObjectAction = null)
        {
            var newEditor = CreateNewEditor(createdObjectAction);
            newEditor.PrepareEditor(objectToOpen, jsonObject);
        }

        public void OpenEditorForThisObject(object objectToOpen, Action<JObject> createdObjectAction = null)
        {
            var jsonObject = JObject.FromObject(objectToOpen);
            var newEditor = CreateNewEditor(createdObjectAction);
            newEditor.PrepareEditor(objectToOpen, jsonObject);
        }

        public void OpenEditorByObjectName(String objectName, Action<JObject> createdObjectAction = null, JObject jsonObject = null)
        {
            var newEditor = CreateNewEditor(createdObjectAction);
            var objectToOpen = newEditor.GetCustomObject(objectName);
            newEditor.PrepareEditor(objectToOpen, jsonObject);
        }

        private JsonEditorBaseBehaviour CreateNewEditor(Action<JObject> createdObjectAction)
        {
            foreach (var obj in this.hideWhenOpeningEditor)
            {
                obj.SetActive(false);
            }
            closeButton.SetActive(true);
            saveAndCloseButton.gameObject.SetActive(true);
            if (openEditors.Count != 0)
            {
                openEditors.Last().gameObject.SetActive(false);
            }
            var newEditor = Instantiate(editorTemplate, this.transform);
            newEditor.Initialise(); //TODO can we remove this?
            newEditor.SetCreatedObjectAction(createdObjectAction);
            newEditor.SetUpdateSaveButtonInteractability(UpdateSaveButtonInteractability);
            newEditor.gameObject.SetActive(true);
            openEditors.Add(newEditor);
            return newEditor;
        }

        public void CloseEditor(bool save)
        {
            var lastIndex = openEditors.Count - 1;
            var last = openEditors[lastIndex];
            last.CloseEditor(save);
            openEditors.RemoveAt(lastIndex);
            if (openEditors.Count != 0)
            {
                openEditors.Last().gameObject.SetActive(true);
            }
            else
            {
                closeButton.SetActive(false);
                saveAndCloseButton.gameObject.SetActive(false);
                foreach (var obj in this.hideWhenOpeningEditor)
                {
                    obj.SetActive(true);
                }
            }
            //if (openEditors.Count <= 1)
            //{
            //    foreach (var onEditorClose in onEditorClosed)
            //    {
            //        onEditorClose.Invoke();
            //    }
            //}
            Destroy(last.gameObject);
        }

        public void OnEditorSave(JObject jsonObject)
        {
            foreach (var onEditorSave in onEditorSave)
            {
                onEditorSave.Invoke(jsonObject);
            }
        }

        public void RegisterOnEditorClosed(Action closeAction)
        {
            onEditorClosed.Add(closeAction);
        }
        public void RegisterOnEditorSave(Action<JObject> saveAction)
        {
            onEditorSave.Add(saveAction);
        }

        public void UpdateSaveButtonInteractability(bool savePossible)
        {
            saveAndCloseButton.interactable = savePossible;
        }

    }
}
