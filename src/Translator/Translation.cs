namespace Translator
{
    public delegate T Equation<T>(T valueToTranslate);
    public delegate T SeedEquation<T>(T valueToTranslate, T seed);

    /// <summary>
    /// A translation defines what two-items are translatable and how to translate them.
    /// 
    /// When using a translator to traverse translations, only `keyA` and `keyB` are used to compare whether a match is found.
    /// </summary>
    public class Translation<TKey, TValue>
    {
        /// <summary>
        /// Sets up a way to go between two-items.
        /// </summary>
        /// <param name="keyA"></param>
        /// <param name="valueA">How to go from `A` to `B` given a value.</param>
        /// <param name="keyB"></param>
        /// <param name="valueB">How to go from `B` to `A` given a value.</param>
        public Translation(
            TKey keyA,
            Equation<TValue> valueA,
            TKey keyB,
            Equation<TValue> valueB)
        {
            KeyA = keyA;
            ValueA = valueA;
            KeyB = keyB;
            ValueB = valueB;
        }

        /// <summary>
        /// Provides a way to short-circuit the constant-value associated when translating between `A` and `B`.
        /// </summary>
        /// <param name="seed">This is the value that is used in both translations (A->B and B->A).</param>
        /// <param name="keyA"></param>
        /// <param name="valueA">How to go from `A` to `B` given a value and seed [(x, seed) => ...].</param>
        /// <param name="keyB"></param>
        /// <param name="valueB">How to go from `B` to `A` given a value and seed [(x, seed) => ...].</param>
        public Translation(
            TValue seed,
            TKey keyA,
            SeedEquation<TValue> valueA,
            TKey keyB,
            SeedEquation<TValue> valueB)
            : this(keyA, x => valueA(x, seed), keyB, x => valueB(x, seed))
        { }

        public readonly TKey KeyA;
        public readonly TKey KeyB;
        public readonly Equation<TValue> ValueA;
        public readonly Equation<TValue> ValueB;
    }
}
