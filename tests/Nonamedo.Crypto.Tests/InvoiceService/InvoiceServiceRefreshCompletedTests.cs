using System;
using Moq;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Services;
using Xunit;

namespace Common.Finance.Tests
{
    public class InvoiceServiceRefreshCompletedTests
    {
        CryptoAccount CreateOwner()
        {
            var r = new Random();
            var account = new CryptoAccount(
                address: r.Next().ToString(),
                publicKey: r.Next().ToString(),
                privateKey: r.Next().ToString());
            
            return account;
        }

        TokenInvoice CreateInvoice(int min = 0, int max = 24)
        {
            var r = new Random();
            var account = new CryptoAccount(
                address: r.Next().ToString(),
                publicKey: r.Next().ToString(),
                privateKey: r.Next().ToString());

            var invoice = new TokenInvoice(
                contract: r.Next().ToString(),
                account: account,
                tokenAmount: r.Next(),
                lifeTime: TimeSpan.FromHours(r.Next(min,  max)));

            return invoice;
        }


        [Fact]
        public async void Test1()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r= new Random();
            decimal balance = r.Next();
            string inGasHash =  r.Next().ToString();
            string outTokenHash = r.Next().ToString();
            invoice.SendGas(balance, inGasHash);
            invoice.WithdrawToken(outTokenHash);
            invoice.Complete();

            //mockCrypto.Setup(x=>x.

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(balance, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Completed, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Equal(outTokenHash, invoice.OutTokenHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }

    }
}
