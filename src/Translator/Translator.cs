using System.Collections.Generic;
using System.Linq;

namespace Translator
{
    public abstract class Translator<TKey, TValue>
    {
        public TranslatorSettings Settings { get; set; }

        private Dictionary<TKey, Dictionary<TKey, HashSet<Translation<TKey, TValue>>>> translations = new Dictionary<TKey, Dictionary<TKey, HashSet<Translation<TKey, TValue>>>>();

        public void Add(Translation<TKey, TValue> translation)
        {
            AddRange(new[] { translation });
        }

        public void AddRange(IEnumerable<Translation<TKey, TValue>> translations)
        {
            foreach (var groupA in translations
                .Union(translations
                .Where(x => !(x is ForwardOnlyTranslation<TKey, TValue>))
                .Select(x => x.Swap()))
                .GroupBy(x => x.KeyA))
            {
                Dictionary<TKey, HashSet<Translation<TKey, TValue>>> keyATranslations;

                if (!this.translations.TryGetValue(groupA.Key, out keyATranslations))
                {
                    keyATranslations = new Dictionary<TKey, HashSet<Translation<TKey, TValue>>>();

                    this.translations.Add(groupA.Key, keyATranslations);
                }

                foreach (var groupB in groupA.GroupBy(x => x.KeyB))
                {
                    keyATranslations.Add(groupB.Key, new HashSet<Translation<TKey, TValue>>(groupB));
                }
            }
        }

        public (bool foundTranslation, IReadOnlyCollection<Translation<TKey, TValue>> translationSteps) TryTranslation(
            TValue fromValue,
            TKey fromKey,
            TKey toKey,
            out TValue translated)
        {
            return TryTranslation(fromValue, fromKey, toKey, out translated, null);
        }

        public (bool foundTranslation, IReadOnlyCollection<Translation<TKey, TValue>> translationSteps) TryTranslation(
        TValue fromValue,
            TKey fromKey,
            TKey toKey,
            out TValue translated,
            TranslatorSettings settings = null)
        {
            settings = settings ?? Settings ?? new TranslatorSettings();

            var (foundTranslation, translationSteps) = Finder(translations, fromValue, fromKey, toKey, settings);

            if (foundTranslation)
            {
                var keyEqualityComparer = EqualityComparer<TKey>.Default;

                for (var i = 0; i < translationSteps.Count; i++)
                {
                    for (var j = i + 1; j < translationSteps.Count; j++)
                    {
                        if (keyEqualityComparer.Equals(translationSteps[i].KeyA, translationSteps[j].KeyB))
                        {
                            translationSteps.RemoveRange(i, (j - i) + 1);

                            i--;

                            break;
                        }
                    }
                }

                translated = translationSteps.Aggregate(fromValue, (accumulator, translation) => translation.ValueB(accumulator));

                return (true, translationSteps);
            }

            translated = default(TValue);

            return (false, Enumerable.Empty<Translation<TKey, TValue>>().ToList());
        }

        private (bool foundTranslation, List<Translation<TKey, TValue>> translationSteps) Finder(
            Dictionary<TKey, Dictionary<TKey, HashSet<Translation<TKey, TValue>>>> translations,
            TValue fromValue,
            TKey fromKey,
            TKey toKey,
            TranslatorSettings settings,
            List<Translation<TKey, TValue>> translationSteps = null,
            HashSet<KeyValuePair<TKey, TKey>> pathsTraveled = null)
        {
            translationSteps = translationSteps ?? new List<Translation<TKey, TValue>>();
            pathsTraveled = pathsTraveled ?? new HashSet<KeyValuePair<TKey, TKey>>();

            var keyEqualityComparer = EqualityComparer<TKey>.Default;
            var valueEqualityComparer = EqualityComparer<TValue>.Default;

            if (translations.TryGetValue(fromKey, out var fromSet))
            {
                if (fromSet.TryGetValue(toKey, out var toSet))
                {
                    foreach (var item in toSet)
                    {
                        if (item is DirectTranslation<TKey, TValue> directTranslation
                            ? valueEqualityComparer.Equals(directTranslation.DirectValueA, fromValue)
                            : keyEqualityComparer.Equals(item.KeyB, toKey))
                        {
                            translationSteps.Add(item);

                            return (true, translationSteps);
                        }
                    }
                }

                foreach (var item in fromSet
                    .SelectMany(x => x.Value)
                    .Where(x => x is DirectTranslation<TKey, TValue> directTranslation
                        ? valueEqualityComparer.Equals(directTranslation.DirectValueA, fromValue)
                        : true))
                {
                    if (pathsTraveled.Count >= settings.MaximumPathTraversal - 1)
                    {
                        break;
                    }
                    else
                    {
                        var pathKey = new KeyValuePair<TKey, TKey>(fromKey, item.KeyB);

                        if (!pathsTraveled.Contains(pathKey))
                        {
                            pathsTraveled.Add(pathKey);

                            var (finderFound, finderTranslationSteps) = Finder(
                                translations,
                                item is DirectTranslation<TKey, TValue> directTranslation
                                    ? directTranslation.DirectValueB
                                    : fromValue,
                                item.KeyB,
                                toKey,
                                settings,
                                translationSteps,
                                pathsTraveled);

                            if (finderFound)
                            {
                                finderTranslationSteps.Insert(0, item);

                                return (finderFound, finderTranslationSteps);
                            }
                        }
                    }
                }
            }

            return (false, Enumerable.Empty<Translation<TKey, TValue>>().ToList());
        }
    }
}
