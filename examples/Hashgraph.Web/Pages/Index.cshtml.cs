using Hashgraph.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Hashgraph.Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public GetBalanceRequestModel GetBalanceRequest { get; set; }
        public ulong? Balance { get; private set; }
        public string ErrorMessage { get; private set; }
        public void OnGet()
        {
            GetBalanceRequest = new GetBalanceRequestModel
            {
                GatewayName = "2.testnet.hedera.com",
                GatewayPort = 50211,
                GatewayRealmNum = 0,
                GatewayShardNum = 0,
                GatewayAccountNum = 5,
                AccountRealmNum = 0,
                AccountShardNum = 0,
                AccountAccountNum = 3
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                await using var client = new Client(ctx =>
                 {
                     ctx.Gateway = new Gateway(
                         $"{GetBalanceRequest.GatewayName}:{GetBalanceRequest.GatewayPort}",
                         GetBalanceRequest.GatewayRealmNum,
                         GetBalanceRequest.GatewayShardNum,
                         GetBalanceRequest.GatewayAccountNum);
                 });
                Balance = await client.GetAccountBalanceAsync(
                    new Address(
                        GetBalanceRequest.AccountRealmNum,
                        GetBalanceRequest.AccountShardNum,
                        GetBalanceRequest.AccountAccountNum));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.ToString();
            }
            return Page();
        }
    }
}
