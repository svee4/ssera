using Radzen;
using System.Diagnostics;

namespace Ssera.Client.Infra;

/// <summary>
///     Contains Radzen-specific implementation details to abuse.
/// </summary>
public static class RadzenImplementationDetails
{
    /// <summary>
    ///     Gets the radzen css classname for the given <see cref="Variant"/>.
    /// </summary>
    /// <exception cref="UnreachableException"></exception>
    public static string GetVariantClassname(Variant variant) => variant switch
    {
        Variant.Filled => "rz-variant-filled",
        Variant.Flat => "rz-variant-flat",
        Variant.Outlined => "rz-variant-outlined",
        Variant.Text => "rz-variant-text",
        _ => throw new UnreachableException($"Unknown {nameof(Variant)} value '{variant}'.")
    };

    /// <summary>
    ///     Gets the <see cref="Variant"/> that is used by default in Radzen components.
    /// </summary>
    public static Variant DefaultVariant => Variant.Outlined;

    public static string FormFieldClassname => "rz-form-field";
    public static string FormFieldContentClassname => "rz-form-field-content";
}
