using System;
using Moq;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Services;
using Xunit;

namespace Common.Finance.Tests
{
    public class InvoiceServiceRefreshFailedTests
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
            
            
            Random r = new Random();
            string errorMessage = r.Next().ToString();
            invoice.Fail(errorMessage);

            //mockCrypto.Setup(x=>x.

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Equal(errorMessage, invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }

     
    }
}
