using System;
using Moq;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Services;
using Xunit;

namespace Common.Finance.Tests
{
    public class InvoiceServiceCreateTokenInvoiceTests
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
        public void Test1()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            
            Random r = new Random();
            decimal tokenAmount = r.Next();
            TimeSpan lifetime = TimeSpan.FromHours(r.Next(1,24));
            CryptoAccount account = new CryptoAccount(r.Next().ToString(), r.Next().ToString(), r.Next().ToString());
            string contract = r.Next().ToString();

             mockCrypto.Setup(x=>x.GetUsdtContract())
                .Returns(contract);
            
            mockCrypto.Setup(x=>x.GenerateAccountAsync())
                .ReturnsAsync(account);

            // Act
            var invoice = service.CreateUsdtInvoiceAsync(
                tokenAmount: tokenAmount,
                lifeTime: lifetime,
                account: null).GetAwaiter().GetResult();

            // Assert
            mockCrypto.Verify(x=>x.GetUsdtContract());
            mockCrypto.Verify(x=>x.GenerateAccountAsync());
            mockCrypto.VerifyNoOtherCalls();

            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Created, invoice.State);
            Assert.NotNull(invoice.Account);
            Assert.Equal(account.Address, invoice.Account.Address);
            Assert.Equal(account.PrivateKey, invoice.Account.PrivateKey);
            Assert.Equal(account.PublicKey, invoice.Account.PublicKey);
            Assert.Equal(contract, invoice.Contract);
            Assert.Equal(tokenAmount, invoice.ExpectedTokenAmount);
            Assert.True(invoice.ExpiredAt > DateTime.UtcNow);
            Assert.True(invoice.StateChangedAt < DateTime.UtcNow);

            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }


        [Fact]
        public void Test2()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            
            Random r = new Random();
            decimal tokenAmount = r.Next();
            TimeSpan lifetime = TimeSpan.FromHours(r.Next(1,24));
            CryptoAccount account = new CryptoAccount(r.Next().ToString(), r.Next().ToString(), r.Next().ToString());
            string contract = r.Next().ToString();

             mockCrypto.Setup(x=>x.GetUsdtContract())
                .Returns(contract);
            
            // Act
            var invoice = service.CreateUsdtInvoiceAsync(
                tokenAmount: tokenAmount,
                lifeTime: lifetime,
                account: account).GetAwaiter().GetResult();

            // Assert
            mockCrypto.Verify(x=>x.GetUsdtContract());
            mockCrypto.VerifyNoOtherCalls();

            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Created, invoice.State);
            Assert.NotNull(invoice.Account);
            Assert.Equal(account.Address, invoice.Account.Address);
            Assert.Equal(account.PrivateKey, invoice.Account.PrivateKey);
            Assert.Equal(account.PublicKey, invoice.Account.PublicKey);
            Assert.Equal(contract, invoice.Contract);
            Assert.Equal(tokenAmount, invoice.ExpectedTokenAmount);
            Assert.True(invoice.ExpiredAt > DateTime.UtcNow);
            Assert.True(invoice.StateChangedAt < DateTime.UtcNow);

            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }

    }
}
