using System;
using System.Linq;
using Translator;

namespace ConsoleSample
{
    public enum Language
    {
        German,
        English,
        Italian,
        Spanish
    }

    public enum Unit
    {
        Inch,
        Foot,
        Mile,
        Centimeter,
        Decimeter,
        Millimeter
    }

    public class LanguageTranslator : Translator<Language, string>
    {
        public LanguageTranslator()
        {
            AddRange(new[]
            {
                // These types of translations do not require expressions. They are simple 1:1 mappings. Also, notice the lack of direct translation between English and Spanish.
                new DirectTranslation<Language, string>(Language.English, "hello", Language.German, "hallo"),
                new DirectTranslation<Language, string>(Language.German, "hallo", Language.Italian, "ciao"),
                new DirectTranslation<Language, string>(Language.Italian, "ciao", Language.Spanish, "hola"),
                new DirectTranslation<Language, string>(Language.English, "goodbye", Language.German, "auf wiedersehen"),
                new DirectTranslation<Language, string>(Language.English, "good", Language.German, "gut")
            });
        }
    }

    public class MeasurementTranslator : Translator<Unit, double>
    {
        public MeasurementTranslator()
        {
            AddRange(new[]
            {
                // If you really don't want translations from dm -> cm.
                new ForwardOnlyTranslation<Unit, double>(Unit.Centimeter, Unit.Decimeter, x => x / 10),

                // This constructor provides a seed-value that is used by both expressions during translations.
                new Translation<Unit, double>(10, Unit.Centimeter, (x, seed) => x / seed, Unit.Millimeter, (x, seed) => x * seed),

                new Translation<Unit, double>(2.54, Unit.Inch, (x, seed) => x / seed, Unit.Centimeter, (x, seed) => x * seed),
                new Translation<Unit, double>(12, Unit.Inch, (x, seed) => x * seed, Unit.Foot, (x, seed) => x / seed),
                new Translation<Unit, double>(Unit.Foot, x => x * 5280, Unit.Mile, x => x / 5280)
            });
        }
    }

    public class LargeTranslator : Translator<int, int>
    { }

    class Program
    {
        static void Main(string[] args)
        {
            var largeTranslator = new LargeTranslator();

            largeTranslator.AddRange(Enumerable.Range(0, 500000).Select(x => new DirectTranslation<int, int>(keyA: x, valueA: x, keyB: x + 1, valueB: x + 1)));

            var (foundLargeTranslation, largeTranslationSteps) = largeTranslator.TryTranslation(0, 0, 11, out var largeTranslated, new TranslatorSettings() { MaximumPathTraversal = 10 });

            Console.WriteLine($"Large stepped-translation of `0` from 0 to 11, but should not find endpoint due to settings stopping traversal at 10");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundLargeTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", largeTranslationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {largeTranslated}");

            var languageTranslator = new LanguageTranslator();

            var englishHello = "hello";
            var englishGoodbye = "goodbye";
            var goodSpanishHello = "hola";
            var badSpanishHello = "yo";

            var (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation(englishHello, Language.English, Language.German, out var languageTranslated);

            Console.WriteLine($"\n\nLanguage translation of `{englishHello}` from {Language.English} to {Language.German}");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundLanguageTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", translationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {languageTranslated}");

            (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation(englishGoodbye, Language.English, Language.German, out languageTranslated);

            Console.WriteLine($"\n\nLanguage translation of `{englishGoodbye}` from {Language.English} to {Language.German}");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundLanguageTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", translationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {languageTranslated}");

            (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation(englishHello, Language.English, Language.Spanish, out languageTranslated);

            Console.WriteLine($"\n\nLanguage translation of `{englishHello}` from {Language.English} to {Language.Spanish}");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundLanguageTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", translationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {languageTranslated}");

            (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation(goodSpanishHello, Language.Spanish, Language.English, out languageTranslated);

            Console.WriteLine($"\n\nLanguage translation of `{goodSpanishHello}` from {Language.Spanish} to {Language.English}");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundLanguageTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", translationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {languageTranslated}");

            (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation(badSpanishHello, Language.Spanish, Language.English, out languageTranslated);

            Console.WriteLine($"\n\nLanguage translation of `{badSpanishHello}` from {Language.Spanish} to {Language.English}");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundLanguageTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", translationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {languageTranslated}");

            var measurementTranslator = new MeasurementTranslator();

            var centimeters = 1;

            var (foundMeasurementTranslation, measurementTranslationSteps) = measurementTranslator.TryTranslation(centimeters, Unit.Centimeter, Unit.Millimeter, out var measurementTranslated);

            Console.WriteLine($"\n\nMeasurement translation of `{centimeters}` {Unit.Centimeter} to {Unit.Millimeter}");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundMeasurementTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", measurementTranslationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {measurementTranslated}");

            (foundMeasurementTranslation, measurementTranslationSteps) = measurementTranslator.TryTranslation(centimeters, Unit.Centimeter, Unit.Mile, out measurementTranslated);

            Console.WriteLine($"\n\nMeasurement translation of `{centimeters}` {Unit.Centimeter} to {Unit.Mile}");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundMeasurementTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", measurementTranslationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {measurementTranslated}");

            var inches = 15840;

            (foundMeasurementTranslation, measurementTranslationSteps) = measurementTranslator.TryTranslation(inches, Unit.Inch, Unit.Mile, out measurementTranslated);

            Console.WriteLine($"\n\nMeasurement translation of `{inches}` {Unit.Inch} to {Unit.Mile}");
            Console.WriteLine("--------------------\n");
            Console.WriteLine($"Translation found: {foundMeasurementTranslation.ToString()}");
            Console.WriteLine($"Translation steps: {string.Join(" => ", measurementTranslationSteps.Select(x => $"{x.KeyA.ToString()}->{x.KeyB.ToString()}"))}");
            Console.WriteLine($"Translated value: {measurementTranslated}");
        }
    }
}
