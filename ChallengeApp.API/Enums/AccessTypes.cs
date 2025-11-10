namespace ChallengeApp.API.Enums;

public static class AccessTypes
{
    public const string Public = "Public";
    public const string Private = "Private";
    public const string Restricted = "Restricted";

    public static readonly string[] All = { Public, Private, Restricted };
    
    public const string ValidationPattern = "Public|Private|Restricted";
    public const string ValidationMessage = "AccessType must be Public, Private, or Restricted.";
}
