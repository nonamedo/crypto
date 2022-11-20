using System;
using Moq;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Services;
using Xunit;

namespace Common.Finance.Tests
{
    public class InvoiceServiceRefreshGasWithdrawnTests
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
            decimal actualTokenAmount = r.Next();
            string inGasHash = r.Next().ToString();
            string outTokenHash = r.Next().ToString();
            string outGasHash = r.Next().ToString();

            invoice.SendGas(actualTokenAmount, inGasHash);
            invoice.WithdrawToken(outTokenHash);
            invoice.WithdrawGas(outGasHash);

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(outGasHash))
                .ReturnsAsync(true);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(outGasHash));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Completed, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal(outTokenHash, invoice.OutTokenHash);
            Assert.Equal(outGasHash, invoice.OutGasHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }


        [Fact]
        public async void TransactionIsNotConfirmedTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal actualTokenAmount = r.Next();
            string inGasHash = r.Next().ToString();
            string outTokenHash = r.Next().ToString();
            string outGasHash = r.Next().ToString();

            invoice.SendGas(actualTokenAmount, inGasHash);
            invoice.WithdrawToken(outTokenHash);
            invoice.WithdrawGas(outGasHash);

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(outGasHash))
                .ReturnsAsync(false);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(outGasHash));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.GasWithdrawn, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal(outTokenHash, invoice.OutTokenHash);
            Assert.Equal(outGasHash, invoice.OutGasHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(1, invoice.Attempts);
        }

        [Fact]
        public async void IsTrnsactionConfirmedThrowsExceptionTest()
        {
             // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal actualTokenAmount = r.Next();
            string inGasHash = r.Next().ToString();
            string outTokenHash = r.Next().ToString();
            string outGasHash = r.Next().ToString();
            string errorMessage = r.Next().ToString();

            invoice.SendGas(actualTokenAmount, inGasHash);
            invoice.WithdrawToken(outTokenHash);
            invoice.WithdrawGas(outGasHash);

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(outGasHash))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(outGasHash));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal(outTokenHash, invoice.OutTokenHash);
            Assert.Equal(outGasHash, invoice.OutGasHash);
            Assert.Equal(errorMessage, invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }


        
    
    }
}
