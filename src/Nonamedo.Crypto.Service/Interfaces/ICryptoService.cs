using System;
using System.Threading.Tasks;

namespace Nonamedo.Crypto.Service.interfaces
{
    public interface ICryptoService
    {
        string GetUsdtContract();
        Task<CryptoAccount> GenerateAccountAsync();
        Task<decimal> GetTokenBalanceAsync(CryptoAccount account, string contractAddress);
        Task<decimal> CalcGasToWithdrawTokenAsync(CryptoAccount from, string toAddress, string contractAddress, decimal tokenAmount);
        Task<string> SendTransactionAsync(CryptoAccount from, string toAddress, decimal amount);
        Task<bool> IsTrnsactionConfirmedAsync(string txid);
        Task<string> WithdrawTokenAsync(CryptoAccount from, string toAddress, string contractAddress, decimal tokenAmount);
        Task<string> WithdrawGasAsync(CryptoAccount from, string toAddress);
        Task<decimal> GetGasBalanceAsync(CryptoAccount account);
        
    }
}
