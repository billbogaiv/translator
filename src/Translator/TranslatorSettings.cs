namespace Translator
{
    public class TranslatorSettings
    {
        /// <summary>
        /// Represents how many paths the translator will take before stopping the process.
        /// 
        /// (a, b), (b, c), (c, d), (d, e)
        /// 
        /// to go from a->d takes three paths: a->b and b->c and c->d.
        /// 
        /// Be careful setting this value too high (i.e. >2000) as a `StackOverflowException` will most-likely occur.
        /// </summary>
        public int MaximumPathTraversal { get; set; } = 10;
    }
}
