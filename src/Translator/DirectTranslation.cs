namespace Translator
{
    /// <summary>
    /// Use when translations do not require expressions.
    /// 
    /// Example: { "en-US", "Hello" } <-> { "de": "Hallo" }
    /// </summary>
    public class DirectTranslation<TKey, TValue> : Translation<TKey, TValue>
    {
        public DirectTranslation(
            TKey keyA,
            TValue valueA,
            TKey keyB,
            TValue valueB)
            : base(keyA, _ => valueA, keyB, _ => valueB)
        { }
    }
}
