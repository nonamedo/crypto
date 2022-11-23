using System;
using System.Threading.Tasks;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Invoice.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;

namespace Nonamedo.Crypto.Services
{
    public class InvoiceService:   IInvoiceService
    {
        private readonly ICryptoService _cryptoService;
        private readonly CryptoAccount _owner;
        
        public InvoiceService(ICryptoService cryptoService, CryptoAccount owner)
        {
            _cryptoService = cryptoService;
            _owner = owner;
        }


        public async Task<TokenInvoice> CreateUsdtInvoiceAsync(decimal tokenAmount, TimeSpan lifeTime, CryptoAccount account)
        {
            return await CreateTokenInvoiceAsync(
                contract: _cryptoService.GetUsdtContract(), 
                tokenAmount: tokenAmount, 
                lifeTime: lifeTime,
                account: account);
        }


        public async Task<TokenInvoice> CreateTokenInvoiceAsync(string contract, decimal tokenAmount, TimeSpan lifeTime, CryptoAccount account)
        {
            if (account == null)
            {
                account = await _cryptoService.GenerateAccountAsync();
            }

            var invoice = new TokenInvoice(
                contract: contract,
                account: account,
                tokenAmount: tokenAmount,
                lifeTime: lifeTime);

            return invoice;
        }


        public async Task ProcessInvoiceAsync(TokenInvoice invoice)
        {
            TokenInvoiceState lastState;
            do 
            {
                lastState = invoice.State;
                await RefreshInvoiceAsync(invoice);
            }
            while (lastState != invoice.State);
        }

        public async Task RefreshInvoiceAsync(TokenInvoice invoice)
        {
            try
            {
                switch (invoice.State)
                {
                    case TokenInvoiceState.Created: 
                        await SendGas(invoice);
                        break;
                    
                    case TokenInvoiceState.GasSent: 
                        await WithdrawToken(invoice);
                        break;
                    
                    case TokenInvoiceState.TokenWithdrawn: 
                        await WithdrawGas(invoice);
                        break;
                    
                    case TokenInvoiceState.GasWithdrawn: 
                        await Complete(invoice);
                        break;

                    default: 
                        break;
                }
            }
            catch (Exception exc)
            {
                invoice.Fail(exc.Message);
            }
        }


        #region Private
        
        private async Task SendGas(TokenInvoice invoice)
        {
            if (invoice.ExpiredAt < DateTime.UtcNow)
            {
                invoice.Expire();
            }
            else 
            {
                var balance = await  _cryptoService.GetTokenBalanceAsync(
                    account: invoice.Account,
                    contractAddress: invoice.Contract);

                if (balance > 0)
                {
                    var gas = await _cryptoService.CalcGasToWithdrawTokenAsync(
                        from: invoice.Account,
                        toAddress: _owner.Address,
                        contractAddress: invoice.Contract,
                        tokenAmount: balance);

                    if (gas == 0)
                    {
                        throw new Exception("Could not calc gas");
                    }

                    var txid = await _cryptoService.SendTransactionAsync(
                        from: _owner,
                        toAddress: invoice.Account.Address,
                        amount: gas);

                    if (string.IsNullOrWhiteSpace(txid))
                    {
                        throw new Exception("Could not send gas");
                    }
                    
                    invoice.SendGas(balance, txid);
                }
                else
                {
                    invoice.AddAttempt();
                }
            }
        }

        private async Task WithdrawToken(TokenInvoice invoice)
        {
            if (await _cryptoService.IsTrnsactionConfirmedAsync(invoice.InGasHash))
            {
                var txid = await _cryptoService.WithdrawTokenAsync(
                    from: invoice.Account,
                    contractAddress: invoice.Contract,
                    toAddress: _owner.Address,
                    tokenAmount: invoice.ActualTokenAmount);

                if (string.IsNullOrWhiteSpace(txid))
                {
                    throw new Exception("Could not withdraw token");
                }
                
                invoice.WithdrawToken(txid);
            }
            else
            {
                invoice.AddAttempt();
            }
        }

        private async Task WithdrawGas(TokenInvoice invoice)
        {
            if (await _cryptoService.IsTrnsactionConfirmedAsync(invoice.OutTokenHash))
            {
                var balance = await _cryptoService.GetGasBalanceAsync(invoice.Account);
                if (balance > 0)
                {
                   var txid = await _cryptoService.WithdrawGasAsync(
                        from: invoice.Account,
                        toAddress: _owner.Address);

                    if (string.IsNullOrWhiteSpace(txid))
                    {
                        throw new Exception("Could not withdraw gas");
                    }

                    invoice.WithdrawGas(txid);
                }
                else
                {
                    invoice.Complete();
                }
            }
            else
            {
                 invoice.AddAttempt();
            }
        }
        
        private async Task Complete(TokenInvoice invoice)
        {
            if (await _cryptoService.IsTrnsactionConfirmedAsync(invoice.OutGasHash))
            {
                invoice.Complete();
            }
            else
            {
                invoice.AddAttempt();
            }
        }

        #endregion


    }
}
