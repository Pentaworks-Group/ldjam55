using System;
using System.Collections.Generic;

using GameFrame.Core.Extensions;

using UnityEngine;

namespace Assets.Scripts.Core.Definitions.Loaders
{
    public class ResourceLoader<TDefinition> where TDefinition : GameFrame.Core.Definitions.BaseDefinition
    {
        protected readonly Dictionary<String, TDefinition> targetCache;

        public ResourceLoader(Dictionary<String, TDefinition> targetCache)
        {
            if (targetCache == default)
            {
                throw new ArgumentNullException(nameof(targetCache), "Definition cache is required and may not be null!");
            }

            this.targetCache = targetCache;
        }

        public virtual void LoadDefinition(String resourceName)
        {
            var filePath = $"{Application.streamingAssetsPath}/{resourceName}";

            LoadAsset(filePath, HandleDefinitions);
        }

        protected void LoadAsset(String filePath, Func<List<TDefinition>, List<TDefinition>> onLoadedCallback)
        {
            var gameO = new GameObject();

            var mono = gameO.AddComponent<EmptyLoadingBehaviour>();

            _ = mono.StartCoroutine(GameFrame.Core.Json.Handler.DeserializeObjectFromStreamingAssets(filePath, onLoadedCallback));

            GameObject.Destroy(gameO);
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

            return sourceList;
        }
    }
}
