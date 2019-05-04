using System.Collections.Generic;
using System.Linq;

namespace Translator
{
    public abstract class Translator<TKey, TValue>
    {
        public abstract List<Translation<TKey, TValue>> Translations { get; }

        public (bool, IReadOnlyCollection<Translation<TKey, TValue>>) TryTranslation(
            TValue fromValue,
            TKey fromKey,
            TKey toKey,
            out TValue translated)
        {
            var swappedTranslations = Translations
                .Where(x => x.ValueA != null)
                .Select(x => x is DirectTranslation<TKey, TValue>
                    ? new DirectTranslation<TKey,TValue>(x.KeyB, x.ValueB(default(TValue)), x.KeyA, x.ValueA(default(TValue)))
                    : new Translation<TKey, TValue>(x.KeyB, x.ValueB, x.KeyA, x.ValueA))
                .ToList();
            var allTranslations = Translations.Union(swappedTranslations).ToList();

            var (foundTranslation, translationSteps) = Finder(allTranslations, fromValue, fromKey, toKey);

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

                translated = translationSteps.Aggregate(fromValue, (acc, translation) => translation.ValueB(acc));

                return (true, translationSteps);
            }

            translated = default(TValue);

            return (false, Enumerable.Empty<Translation<TKey, TValue>>().ToList());
        }

        private (bool, List<Translation<TKey, TValue>>) Finder(
            ICollection<Translation<TKey, TValue>> translations,
            TValue fromValue,
            TKey fromKey,
            TKey toKey,
            List<Translation<TKey, TValue>> translationSteps = null,
            List<KeyValuePair<TKey, TKey>> pathsTraveled = null)
        {
            translationSteps = translationSteps ?? new List<Translation<TKey, TValue>>();
            pathsTraveled = pathsTraveled ?? new List<KeyValuePair<TKey, TKey>>();

            var keyEqualityComparer = EqualityComparer<TKey>.Default;
            var valueEqualityComparer = EqualityComparer<TValue>.Default;
            var fromSet = translations
                .Where(x => x is DirectTranslation<TKey, TValue>
                    ? keyEqualityComparer.Equals(x.KeyA, fromKey) && valueEqualityComparer.Equals(x.ValueA(default(TValue)), fromValue)
                    : keyEqualityComparer.Equals(x.KeyA, fromKey))
                .ToList();

            foreach (var item in fromSet)
            {
                if (keyEqualityComparer.Equals(item.KeyB, toKey))
                {
                    translationSteps.Add(item);

                    return (true, translationSteps);
                }
            }

            foreach (var item in fromSet)
            {
                if (!pathsTraveled.Contains(new KeyValuePair<TKey, TKey>(fromKey, item.KeyB)))
                {
                    pathsTraveled.Add(new KeyValuePair<TKey, TKey>(fromKey, item.KeyB));

                    var (finderFound, finderTranslationSteps) = Finder(
                        translations,
                        item is DirectTranslation<TKey, TValue>
                            ? item.ValueB(default(TValue))
                            : fromValue,
                        item.KeyB,
                        toKey,
                        translationSteps,
                        pathsTraveled);

                    if (finderFound)
                    {
                        finderTranslationSteps.Insert(0, item);

                        return (finderFound, finderTranslationSteps);
                    }
                }
            }

            return (false, Enumerable.Empty<Translation<TKey, TValue>>().ToList());
        }
    }
}
