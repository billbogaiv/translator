# Translator

Provides a way to create user-defined mappings between user-defined units and query the mappings. Querying works across multiple mappings `A->C` as long as there is mapping from `A->B and B->C...`

## How does it work?

The `Translator` defines what to translate and the type of output when running a translation. A `Translation` tells the `Translator` how to go from the *input* to *output*.

Since translations are user-defined, it's up to the implementation to correctly define going between *input* and *output* in a way that makes sense. When defining translations, a few different versions exist:

### Translation<TKey, TValue>

This is the standard-form of all translations where `TKey` is the type of identifier for the translation and `TValue` is the type of input/output the translation produces. Defining the translation is based on `Func<TValue, TValue>`:

```csharp
new Translation<string, double>(
    "ft", x => x * 5280,
    "mi", x => x / 5280)
```

In this case, translating from `ft` to `mi` means receiving an input of `(double)x` and returning the result of dividing `x` by `5280`. And, because the translation goes both ways, we need to provide the opposite expression.

### ForwardOnlyTranslation<TKey, TValue>

Much like the previous example, the main difference being only defining one-side of the translation:

```csharp
new ForwardOnlyTranslation<string, double>("ft", "mi", x => x / 5280)
```

In this case, we can only translate from `ft->mi`.

### DirectTranslation<TKey, TValue>

This version is for when the translation is driven by values–not expressions. The classic example is language translation:

```csharp
new DirectTranslation<string, string>("en-US", "hello", "de", "hallo")
```

**The key difference in this type of translation is both `TKey` and `TValue` are used to traverse translations within the translator. Otherwise, using the same set of keys across multiple translations would produce odd results**:

```csharp
new DirectTranslation<string, string>("en-US", "hello", "de", "hallo"),
new DirectTranslation<string, string>("en-US", "goodbye", "de", "auf wiedersehen")
```

Trying to translate English `goodbye` into German would always return `hallo` since it's first in the collection.

#### What about using the language example with `Translation<TKey, TValue>`?

This won't work since `Translation` is for expressions:

```csharp
new Translation<string, string>("en-US", _ => "hello", "de", _ => "hallo"),
new Translation<string, string>("en-US", _ => "goodbye", "de", _ => "auf wiedersehen")
```

While syntactically correct, the result is the same as the previous example. The translation-process would try to feed `goodbye` into the expression and always get back `hallo`.

#### What if I built a switch-statement?

Sure, you could build a switch statement into the expression for each language, but then the translation would only work between those two languages:

```csharp
new Translation<string, string>(
    "en-US", input =>
    {
        switch (input)
        {
            case "hallo":
                return "hello";

            default: return null;
        }
    },
    "de", input =>
    {
        switch (input)
        {
            case "hello":
                return "hallo";

            default: return null;
        }
    })
```

This would quickly become cumbersome to handle multiple words and prevents traversing across additional languages that may become available. In this next example, three-langugages are involved and linked together to allow translating into any of the languages:

```csharp
new DirectTranslation<string, string>("en-US", "hello", "de", "hallo"),
new DirectTranslation<string, string>("de", "hallo", "it", "ciao")
```

## Pros/Cons

### Pros

  - Automatic chaining of translations
  - No specific data structure required
  - No reliance on third-party APIs

### Cons

  - Requires manually creating translations
  - Executing a translation that will build a translation-map of thousands of steps will eventually cause a `StackOverflowException`.

To illustrate the last-point:

```csharp
public class LargeTranslator : Translator<int, int>
{
    public override List<Translation<int, int>> Translations { get; } = new List<Translation<int, int>>();
}

var largeTranslator = new LargeTranslator();

largeTranslator.Translations.AddRange(Enumerable.Range(0, 3000).Select(x => new DirectTranslation<int, int>(keyA: x, valueA: x, keyB: x + 1, valueB: x + 1)));

var (foundLargeTranslation, largeTranslationSteps) = largeTranslator.TryTranslation(0, 0, 3000, out var largeTranslated);
```

On a smaller-scale:

|  From  |   To   |
|  :---: |  :---: |
|    0   |    1   |
|    1   |    2   |
|    2   |    3   |
|    3   |    4   |
|    4   |    5   |

To go from `0` to `5` a map is built: `0->1 => 1->2 => 2->3 => 3->4 => 4->5`. *Each step in the map internally is the result of a recursive call. Only when the end-translation is found does the map actually get built.*

**Rembember, it's not the number of translations, it's the size of the translation-map that's important!**

```charp
largeTranslator.Translations.AddRange(Enumerable.Range(0, 500000).Select(x => new DirectTranslation<int, int>(keyA: x, valueA: x, keyB: x + 1, valueB: x + 1)));

var (foundLargeTranslation, largeTranslationSteps) = largeTranslator.TryTranslation(0, 0, 100, out var largeTranslated);
```

This example is perfectly fine and produces a translation-map of one-hundred steps.

## Examples

### .NET Core

```shell
dotnet add package Translator
```

#### Create a measurement translator

```csharp
public enum Unit
{
    Inch,
    Foot,
    Mile,
    Centimeter,
    Decimeter,
    Millimeter
}

public class MeasurementTranslator : Translator<Unit, double>
{
    public override List<Translation<Unit, double>> Translations { get; } = new List<Translation<Unit, double>>()
    {
        // If you really don't want translations from dm -> cm.
        new ForwardOnlyTranslation<Unit, double>(Unit.Centimeter, Unit.Decimeter, x => x / 10),

        // This constructor provides a seed-value that is used by both expressions during translations.
        new Translation<Unit, double>(10, Unit.Centimeter, (x, seed) => x / seed, Unit.Millimeter, (x, seed) => x * seed),

        new Translation<Unit, double>(2.54, Unit.Inch, (x, seed) => x / seed, Unit.Centimeter, (x, seed) => x * seed),
        new Translation<Unit, double>(12, Unit.Inch, (x, seed) => x * seed, Unit.Foot, (x, seed) => x / seed),
        new Translation<Unit, double>(Unit.Foot, x => x * 5280, Unit.Mile, x => x / 5280)
    };
}
```

#### Using the translator

```csharp
var measurementTranslator = new MeasurementTranslator();

/**
 * Return-value tells us whether a translation is found and what steps it took to get from `Inch` to `Mile`.
 * Steps: Inch->Foot => Foot->Mile
 */
var (foundMeasurementTranslation, measurementTranslationSteps) = measurementTranslator.TryTranslation(15840, Unit.Inch, Unit.Mile, out var measurementTranslated);

// measurementTranslated == 0.25 // quarter-mile


/**
 * Variant to show `Centimeter` to `Mile` and how certain translations work both-ways and there doesn't need to be a direct translation between `Centimeter` and `Mile`.
 * Steps: Centimeter->Inch => Inch->Foot => Foot->Mile
 */
var (foundMeasurementTranslation, measurementTranslationSteps) = measurementTranslator.TryTranslation(1, Unit.Centimeter, Unit.Mile, out var measurementTranslated);

```

### Create a language translator

```csharp
public enum Language
{
    German,
    English,
    Italian,
    Spanish
}

public class LanguageTranslator : Translator<Language, string>
{
    public override List<Translation<Language, string>> Translations { get; } = new List<Translation<Language, string>>()
    {
        // These types of translations do not require expressions. They are simple 1:1 mappings. Also, notice the lack of direct translation between English and Spanish.
        new DirectTranslation<Language, string>(Language.English, "hello", Language.German, "hallo"),
        new DirectTranslation<Language, string>(Language.German, "hallo", Language.Italian, "ciao"),
        new DirectTranslation<Language, string>(Language.Italian, "ciao", Language.Spanish, "hola"),
        new DirectTranslation<Language, string>(Language.English, "goodbye", Language.German, "auf wiedersehen"),
        new DirectTranslation<Language, string>(Language.English, "good", Language.German, "gut")
    };
}
```

#### Using the translator

```csharp
var languageTranslator = new LanguageTranslator();

// Steps: English->German => German->Italian => Italian->Spanish
var (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation("hello", Language.English, Language.Spanish, out var languageTranslated);

// languageTranslated == "hola"

// Steps: Spanish->Italian => Italian->German
var (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation("hola", Language.Spanish, Language.German, out var languageTranslated);

// languageTranslated == "hallo"
```