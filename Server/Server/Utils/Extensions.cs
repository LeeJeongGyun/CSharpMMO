namespace Server.Utils;

using Server.DB;

public static class Extensions
{
    public static bool SaveChangesEx(this AppDbContext db)
    {
        try
        {
            db.SaveChanges();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}
