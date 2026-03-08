namespace Allocore.Domain.Entities.Users;

public record LocaleTag
{
    public string Value { get; }
    
    private LocaleTag(string value) => Value = value;
    
    public static LocaleTag Default => new("en-US");
    
    public static LocaleTag Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Default;
        return new LocaleTag(value.Trim());
    }
}
