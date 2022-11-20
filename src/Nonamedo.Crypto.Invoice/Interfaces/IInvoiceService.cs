using System;
using System.Threading.Tasks;
using Nonamedo.Crypto.Service;

namespace Nonamedo.Crypto.Invoice.interfaces
{
    public interface IInvoiceService
    {
        Task<TokenInvoice> CreateUsdtInvoiceAsync(decimal tokenAmount, TimeSpan lifeTime, CryptoAccount account);
        Task<TokenInvoice> CreateTokenInvoiceAsync(string contract, decimal tokenAmount, TimeSpan lifeTime, CryptoAccount account);
        Task ProcessInvoiceAsync(TokenInvoice invoice);
        Task RefreshInvoiceAsync(TokenInvoice invoice);
    }
}
