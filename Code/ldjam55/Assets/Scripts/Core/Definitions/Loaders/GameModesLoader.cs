﻿using System;
using System.Collections;
using System.Collections.Generic;

using GameFrame.Core.Definitions.Loaders;

using Unity.VisualScripting;

namespace Assets.Scripts.Core.Definitions.Loaders
{
    public class GameModesLoader : DefinitionLoader<GameMode>
    {
        private readonly IDictionary<String, GameField> gameFieldCache;

        public GameModesLoader(IDictionary<String, GameMode> targetCache, IDictionary<String, GameField> gameFieldCache) : base(targetCache)
        {
            if (gameFieldCache == default)
            {
                throw new ArgumentNullException(nameof(gameFieldCache));
            }

            this.gameFieldCache = gameFieldCache;
        }

        protected override void OnDefinitionsLoaded(List<GameMode> loadedDefinitions)
        {
            if (loadedDefinitions?.Count > 0)
            {
                foreach (var loadedGameMode in loadedDefinitions)
                {
                    var newGameMode = new GameMode()
                    {
                        Reference = loadedGameMode.Reference,
                        Name = loadedGameMode.Name,
                        Description = loadedGameMode.Description,
                        StartLevel = loadedGameMode.StartLevel,
                        Creepers = loadedGameMode.Creepers,
                        Levels = loadedGameMode.Levels,
                        FlowSpeed = loadedGameMode.FlowSpeed,
                        MinFlow = loadedGameMode.MinFlow,
                        MinNewCreep = loadedGameMode.MinNewCreep,
                        NothingFlowRate = loadedGameMode.NothingFlowRate,
                    };

                    foreach (var level in loadedGameMode.Levels)
                    {
                        level.GameField = CheckItem(level.GameField, this.gameFieldCache);
                    }

                    targetCache[loadedGameMode.Reference] = newGameMode;
                }
            }
        }

        protected virtual TItem CheckItem<TItem>(TItem loadedItem, IDictionary<String, TItem> referenceCache) where TItem : GameFrame.Core.Definitions.BaseDefinition, new()
        {
            var targetItem = new TItem()
            {
                Reference = loadedItem.Reference
            };

            if (loadedItem.IsReferenced)
            {
                if (referenceCache.TryGetValue(loadedItem.Reference, out var referencedItem))
                {
                    foreach (var property in loadedItem.GetType().GetProperties())
                    {
                        if (property.PropertyType.IsGenericType && (typeof(IList).IsAssignableFrom(property.PropertyType.GetGenericTypeDefinition())))
                        {
                            var listValue = property.GetValue(loadedItem);

                            if (listValue == default)
                            {
                                listValue = property.GetValue(referencedItem);
                            }

                            var newList = (IList)Activator.CreateInstance(property.PropertyType);

                            if (listValue is IList list)
                            {
                                foreach (var item in list)
                                {
                                    _ = newList.Add(item);
                                }
                            }

                            property.SetValue(targetItem, newList);
                        }
                        else if (property.PropertyType.IsNullable())
                        {
                            var actualValue = property.GetValue(loadedItem);

                            if (actualValue == default)
                            {
                                actualValue = property.GetValue(referencedItem);
                            }

                            property.SetValue(targetItem, actualValue);
                        }
                    }
                }
            }
            else
            {
                foreach (var property in loadedItem.GetType().GetProperties())
                {
                    if (property.PropertyType.IsGenericType && (typeof(IList).IsAssignableFrom(property.PropertyType.GetGenericTypeDefinition())))
                    {
                        var listValue = property.GetValue(loadedItem);

                        var newList = (IList)Activator.CreateInstance(property.PropertyType);

                        if (listValue is IList list)
                        {
                            foreach (var item in list)
                            {
                                _ = newList.Add(item);
                            }
                        }

                        property.SetValue(targetItem, newList);
                    }
                    else if (property.PropertyType.IsNullable())
                    {
                        var actualValue = property.GetValue(loadedItem);

                        property.SetValue(targetItem, actualValue);
                    }
                }
            }
            return targetItem;
        }

        protected virtual void CheckItems<TItem>(List<TItem> loadedItems, List<TItem> targetItems, Dictionary<String, TItem> referenceCache) where TItem : GameFrame.Core.Definitions.BaseDefinition, new()
        {
            if (loadedItems?.Count > 0)
            {
                foreach (var loadedItem in loadedItems)
                {
                    var targetItem = CheckItem(loadedItem, referenceCache);

                    targetItems.Add(targetItem);
                }
            }
        }
    }
}
