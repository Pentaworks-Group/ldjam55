using System;
using System.Collections;
using System.Collections.Generic;

using GameFrame.Core.Definitions.Loaders;
using GameFrame.Core.Extensions;

using UnityEngine;

namespace Assets.Scripts.Core.Definitions.Loaders
{
    public class ResourceLoader<TDefinition> where TDefinition : GameFrame.Core.Definitions.BaseDefinition
    {
        protected readonly Dictionary<String, TDefinition> targetCache;
        protected Action<List<TDefinition>> onCompleteAction;

        public ResourceLoader(Dictionary<String, TDefinition> targetCache)
        {
            if (targetCache == default)
            {
                throw new ArgumentNullException(nameof(targetCache), "Definition cache is required and may not be null!");
            }

            this.targetCache = targetCache;
        }

        public virtual void LoadDefinition(String resourceName, Action<List<TDefinition>> onCompleteAction = default)
        {
            var filePath = $"{Application.streamingAssetsPath}/{resourceName}";

            this.onCompleteAction = onCompleteAction;

            LoadAsset(filePath, HandleDefinitions);
        }

        public virtual IEnumerator LoadDefinitionInumerator(String resourceName, Action<List<TDefinition>> onCompleteAction = default)
        {
            var filePath = $"{Application.streamingAssetsPath}/{resourceName}";

            this.onCompleteAction = onCompleteAction;

            return LoadAssetInumerator(filePath, HandleDefinitions);
        }

        protected void LoadAsset(String filePath, Func<List<TDefinition>, List<TDefinition>> onLoadedCallback)
        {
            var gameO = new GameObject();

            var mono = gameO.AddComponent<EmptyLoadingBehaviour>();

            _ = mono.StartCoroutine(LoadAssetInumerator(filePath, onLoadedCallback));

            GameObject.Destroy(gameO);
        }

        private static IEnumerator LoadAssetInumerator(string filePath, Func<List<TDefinition>, List<TDefinition>> onLoadedCallback)
        {
            return GameFrame.Core.Json.Handler.DeserializeObjectFromStreamingAssets(filePath, onLoadedCallback);
        }

        protected virtual List<TDefinition> HandleDefinitions(List<TDefinition> sourceList)
        {
            if (sourceList == default)
            {
                throw new ArgumentNullException(nameof(sourceList), "Definition source list may not be null!");
            }

            if (sourceList?.Count > 0)
            {
                foreach (var loadedDefinition in sourceList)
                {
                    if (loadedDefinition.Reference.HasValue())
                    {
                        this.targetCache[loadedDefinition.Reference] = loadedDefinition;
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(GameFrame.Core.Definitions.BaseDefinition.Reference), "Reference of Definition may not be Null, Empty or WhiteSpace!");
                    }
                }
            }

            this.onCompleteAction?.Invoke(sourceList);
            return sourceList;
        }
    }
}
