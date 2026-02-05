namespace EducAIte.Domain.ValueObjects;


public record SchoolYear(int StartYear, int EndYear)
{
    public string DisplayName => $"{StartYear}-{EndYear}";  

    public bool IsValid => EndYear == StartYear + 1;
}