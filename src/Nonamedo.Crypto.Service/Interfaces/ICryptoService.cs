using System;
using System.Threading.Tasks;

namespace Nonamedo.Crypto.Service.interfaces
{
    public interface ICryptoService
    {
        string GetUsdtContract();
        Task<CryptoAccount> GenerateAccountAsync();
        Task<decimal> GetTokenBalanceAsync(CryptoAccount account, string contractAddress);
        Task<decimal> CalcGasAmountToSendAsync(string contractAddress);
        Task<string> SendTransactionAsync(CryptoAccount from, string toAddress, decimal amount);
        Task<bool> IsTrnsactionConfirmedAsync(string txid);
        Task<string> TransferTokenAsync(CryptoAccount from, string contractAddress, string toAddress, decimal amount);
        Task<decimal> CalcGasAmountToWithdrawAsync(decimal balance);
        Task<decimal> GetGasBalanceAsync(CryptoAccount account);
        
    }
}
