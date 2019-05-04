# Translator

Provides a way to create user-defined mappings between user-defined units and query the mappings. Querying works across multiple mappings as long as there is mapping from `A->B and B->C...`

## Examples

### ASP.NET Core 2.2

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

// Steps: English->German => German->Spanish
var (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation("hello", Language.English, Language.Spanish, out var languageTranslated);

// languageTranslated == "hola"

// Steps: Spanish->Italian => Italian->German
var (foundLanguageTranslation, translationSteps) = languageTranslator.TryTranslation("hola", Language.Spanish, Language.German, out var languageTranslated);

// languageTranslated == "hallo"
```