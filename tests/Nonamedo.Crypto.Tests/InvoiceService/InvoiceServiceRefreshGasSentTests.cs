using System;
using Moq;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Services;
using Xunit;

namespace Common.Finance.Tests
{
    public class InvoiceServiceRefreshGasSentTests
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
            string txid = r.Next().ToString();
            string inGasHash = r.Next().ToString();
            invoice.SendGas(actualTokenAmount, inGasHash);

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(inGasHash))
                .ReturnsAsync(true);

            mockCrypto.Setup(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            /*mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(invoice.InGasHash));
            mockCrypto.Verify(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount));
            mockCrypto.VerifyNoOtherCalls();*/

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.TokenWithdrawn, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal(txid, invoice.OutTokenHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }

        [Fact]
        public async void TransactionIsNotConfirmedTest1()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal actualTokenAmount = r.Next();
            string txid = r.Next().ToString();
            string inGasHash = r.Next().ToString();
            invoice.SendGas(actualTokenAmount, inGasHash);

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(inGasHash))
                .ReturnsAsync(false);

            mockCrypto.Setup(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(invoice.InGasHash));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.GasSent, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.OutGasHash);
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
            string txid = r.Next().ToString();
            string inGasHash = r.Next().ToString();
            invoice.SendGas(actualTokenAmount, inGasHash);
            string errorMessage = r.Next().ToString();

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(inGasHash))
                .ThrowsAsync(new Exception(errorMessage));

            mockCrypto.Setup(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(invoice.InGasHash));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal(errorMessage, invoice.ErrorMessage);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Equal(0, invoice.Attempts);
        }
       


        [Fact]
        public async void TxidNullTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal actualTokenAmount = r.Next();
            string txid = null;
            string inGasHash = r.Next().ToString();
            invoice.SendGas(actualTokenAmount, inGasHash);

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(inGasHash))
                .ReturnsAsync(true);

            mockCrypto.Setup(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(invoice.InGasHash));
            mockCrypto.Verify(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal("Could not withdraw token", invoice.ErrorMessage);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Equal(0, invoice.Attempts);
        }

        [Fact]
        public async void TxidEmptyTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal actualTokenAmount = r.Next();
            string txid = "";
            string inGasHash = r.Next().ToString();
            invoice.SendGas(actualTokenAmount, inGasHash);

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(inGasHash))
                .ReturnsAsync(true);

            mockCrypto.Setup(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(invoice.InGasHash));
            mockCrypto.Verify(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal("Could not withdraw token", invoice.ErrorMessage);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Equal(0, invoice.Attempts);
        }

        [Fact]
        public async void TxidWhitespaceTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal actualTokenAmount = r.Next();
            string txid = "   ";
            string inGasHash = r.Next().ToString();
            invoice.SendGas(actualTokenAmount, inGasHash);

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(inGasHash))
                .ReturnsAsync(true);

            mockCrypto.Setup(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(invoice.InGasHash));
            mockCrypto.Verify(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal("Could not withdraw token", invoice.ErrorMessage);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Equal(0, invoice.Attempts);
        }


        [Fact]
        public async void TransferTokenThrowsExcpetionTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal actualTokenAmount = r.Next();
            string txid = r.Next().ToString();
            string inGasHash = r.Next().ToString();
            invoice.SendGas(actualTokenAmount, inGasHash);
            string errorMessage = r.Next().ToString();

            mockCrypto.Setup(x=>x.IsTrnsactionConfirmedAsync(inGasHash))
                .ReturnsAsync(true);

            mockCrypto.Setup(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.IsTrnsactionConfirmedAsync(invoice.InGasHash));
            mockCrypto.Verify(x=>x.TransferTokenAsync(invoice.Account, invoice.Contract, owner.Address, invoice.ActualTokenAmount));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal(errorMessage, invoice.ErrorMessage);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Equal(0, invoice.Attempts);
        }

    
    }
}
