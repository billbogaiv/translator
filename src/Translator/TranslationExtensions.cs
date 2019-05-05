namespace Translator
{
    internal static class TranslationExtensions
    {
        public static Translation<TKey, TValue> Swap<TKey, TValue>(this Translation<TKey, TValue> translation)
        {
            return translation is DirectTranslation<TKey, TValue> directTranslation
                ? new DirectTranslation<TKey, TValue>(directTranslation.KeyB, directTranslation.DirectValueB, directTranslation.KeyA, directTranslation.DirectValueA)
                : new Translation<TKey, TValue>(translation.KeyB, translation.ValueB, translation.KeyA, translation.ValueA);
        }
    }
}
