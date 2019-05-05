namespace Translator
{
    /// <summary>
    /// Use when translation does not require expressions.
    /// 
    /// Example: { "en-US", "Hello" } <-> { "de": "Hallo" }
    /// 
    /// When using a translator to traverse translations, keys and values are used to compare whether a match is found.
    /// This is an important distinction versus `Translation<TKey, TValue>` which only uses keys for comparison.
    /// </summary>
    public class DirectTranslation<TKey, TValue> : Translation<TKey, TValue>
    {
        public DirectTranslation(
            TKey keyA,
            TValue valueA,
            TKey keyB,
            TValue valueB)
            : base(keyA, _ => valueA, keyB, _ => valueB)
        {
            DirectValueA = valueA;
            DirectValueB = valueB;
        }

        /// <summary>
        /// Original value passed to the constructor.
        /// Not an unwrapped-version like `base.ValueA(default(TValue))`.
        /// </summary>
        public readonly TValue DirectValueA;

        /// <summary>
        /// Original value passed to the constructor.
        /// Not an unwrapped-version like `base.ValueB(default(TValue))`.
        /// </summary>
        public readonly TValue DirectValueB;
    }
}
