namespace AccountServer.DB;

using System.ComponentModel.DataAnnotations.Schema;

[Table("Account")]
public class AccountDb
{
    public int AccountDbId { get; set; }
    public string AccountName { get; set; }
    public string Password { get; set; }
}
