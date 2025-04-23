namespace Server.DB;

using System.ComponentModel.DataAnnotations.Schema;
using Protocol;

[Table("Account")]
public class AccountDb
{
    public int AccountDbId { get; set; }
    public string AccountName { get; set; }

    public ICollection<PlayerDb> Players { get; set; }
}

[Table("Player")]
public class PlayerDb
{
    public int PlayerDbId { get; set; }

    public string PlayerName { get; set; }

    [ForeignKey("Account")]
    public int AccountDbId { get; set; }
    public AccountDb Account { get; set; }

    public StatInfo StatInfo { get; set; }

    public ICollection<ItemDb> Items { get; set; }
}

[Table("Item")]
public class ItemDb
{
    public int ItemDbId { get; set; }
    public int TemplateId { get; set; }
    public int Count { get; set; }
    public int Slot { get; set; }

    [ForeignKey("Owner")]
    public int? OwnerDbId { get; set; }
    public PlayerDb Owner { get; set; }
}
