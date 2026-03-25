namespace ForkFeedMobile.Data;

public static class DatabaseConstants
{
    public const string DatabaseFilename = "ForkFeed.db3";

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}
