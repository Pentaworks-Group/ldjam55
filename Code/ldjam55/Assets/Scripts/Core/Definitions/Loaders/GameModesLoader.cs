using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;

namespace Assets.Scripts.Core.Definitions.Loaders
{
    public class GameModesLoader : ResourceLoader<GameMode>
    {
        private readonly Dictionary<String, GameField> gameFieldCache;

        public GameModesLoader(Dictionary<String, GameMode> targetCache, Dictionary<String, GameField> gameFieldCache) : base(targetCache)
        {
            this.gameFieldCache = gameFieldCache;
        }

        protected override List<GameMode> HandleDefinitions(List<GameMode> loadedGameModes)
        {
            if (loadedGameModes?.Count > 0)
            {
                foreach (var loadedGameMode in loadedGameModes)
                {
                    var newGameMode = new GameMode()
                    {
                        Reference = loadedGameMode.Reference,
                        Name = loadedGameMode.Name,
                        Description = loadedGameMode.Description,
                    };

                    newGameMode.GameField = CheckItem(loadedGameMode.GameField, this.gameFieldCache);
                    //CheckItems(loadedGameMode.Spacecrafts, newGameMode.Spacecrafts, this.spacecraftCache);
                    //CheckItems(loadedGameMode.PlayerSpacecrafts, newGameMode.PlayerSpacecrafts, this.spacecraftCache);

                    targetCache[loadedGameMode.Reference] = newGameMode;
                }
            }

            return loadedGameModes;
        }

        private TItem CheckItem<TItem>(TItem loadedItem, Dictionary<String, TItem> referenceCache) where TItem : GameFrame.Core.Definitions.BaseDefinition, new()
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
                                    newList.Add(item);
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
                                newList.Add(item);
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

        private void CheckItems<TItem>(List<TItem> loadedItems, List<TItem> targetItems, Dictionary<String, TItem> referenceCache) where TItem : GameFrame.Core.Definitions.BaseDefinition, new()
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
