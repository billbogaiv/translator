namespace Translator
{
    /// <summary>
    /// Use when translation can only go one-way (A->B) or intentionally to prevent going from B->A.
    /// </summary>
    public class ForwardOnlyTranslation<TKey, TValue> : Translation<TKey, TValue>
    {
        public ForwardOnlyTranslation(
            TKey keyA,
            TKey keyB,
            Equation<TValue> valueB)
            : base(keyA, null, keyB, valueB)
        { }
    }
}
