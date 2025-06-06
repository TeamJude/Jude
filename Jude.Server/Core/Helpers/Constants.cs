namespace Jude.Server.Core.Helpers;

public static class Constants
{
    public const string AccessTokenCookieName = "access_token";
}

public struct Features
{
    public const string Claims = "Claims";
    public const string KnowledgeBase = "KnowledgeBase";
    public const string Users = "Users";

    public static readonly List<string> All = [Claims, KnowledgeBase, Users];
}
