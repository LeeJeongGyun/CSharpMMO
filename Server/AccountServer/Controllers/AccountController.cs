namespace AccountServer.Controllers;

using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private AppDbContext _dbContext;

    public AccountController(AppDbContext dbContext) => _dbContext = dbContext;

    [HttpPost]
    [Route("create")]
    public CreateAccountRes CreateAccount([FromBody] CreateAccountReq accountReqPacket)
    {
        CreateAccountRes createAccountRes = new CreateAccountRes();

        AccountDb? account = _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.AccountName == accountReqPacket.AccountName)
            .FirstOrDefault();

        if (account == null)
        {
            account = new AccountDb();
            account.AccountName = accountReqPacket.AccountName;
            account.Password = accountReqPacket.Password;
            _dbContext.Accounts.Add(account);
            _dbContext.SaveChanges(); // 동시에 같은 AccountName이 들어올 경우 여기서 Crash, 지금은 같지 않다고 가정

            createAccountRes.Result = true;
        }
        else
        {
            createAccountRes.Result = false;
        }

        return createAccountRes;
    }

    [HttpPost]
    [Route("login")]
    public LoginAccountRes LoginAccount([FromBody] LoginAccountReq loginReqPacket)
    {
        LoginAccountRes loginAccountRes = new LoginAccountRes();

        AccountDb? account = _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.AccountName == loginReqPacket.AccountName && a.Password == loginReqPacket.Password)
            .FirstOrDefault();

        if (account == null)
        {
            loginAccountRes.Result = false;
        }
        else
        {
            loginAccountRes.Result = true;
            loginAccountRes.ServerInfos.Add(new ServerInfo() { Name = "스카니아", Ip = "127.0.0.1" });
            loginAccountRes.ServerInfos.Add(new ServerInfo() { Name = "제니스", Ip = "127.0.0.1" });
        }

        return loginAccountRes;
    }
}
