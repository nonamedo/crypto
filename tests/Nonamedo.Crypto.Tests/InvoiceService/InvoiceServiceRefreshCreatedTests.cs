using System;
using Moq;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Services;
using Xunit;

namespace Common.Finance.Tests
{
    public class InvoiceServiceRefreshCreatedTests
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
        public async void ExpiredTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice(0, 0);
            

            //mockCrypto.Setup(x=>x.

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Expired, invoice.State);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }

        [Fact]
        public async void EmptyBalanceTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            //mockCrypto.Setup(x=>x.

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Created, invoice.State);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(1, invoice.Attempts);
        }

        [Fact]
        public async void NotEmptyBalanceTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal balance = r.Next();
            decimal gas = r.Next();
            string txid = r.Next().ToString();

            mockCrypto.Setup(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract))
                .ReturnsAsync(balance);

            mockCrypto.Setup(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance))
                .ReturnsAsync(gas);

            mockCrypto.Setup(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.Verify(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance));
            mockCrypto.Verify(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(balance, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.GasSent, invoice.State);
            Assert.Equal(txid, invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.ErrorMessage);
            Assert.Equal(0, invoice.Attempts);
        }


        [Fact]
        public async void GetTokenBalanceThrowsExceptionTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal balance = r.Next();
            decimal gas = 0;
            string txid = r.Next().ToString();
            string errorMessage = r.Next().ToString();

            mockCrypto.Setup(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract))
                .ThrowsAsync(new Exception(errorMessage));

            mockCrypto.Setup(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance))
                .ReturnsAsync(gas);

            mockCrypto.Setup(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(errorMessage, invoice.ErrorMessage);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Equal(0, invoice.Attempts);
        }


        [Fact]
        public async void GasEmptyTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal balance = r.Next();
            decimal gas = 0;
            string txid = r.Next().ToString();

            mockCrypto.Setup(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract))
                .ReturnsAsync(balance);

            mockCrypto.Setup(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance))
                .ReturnsAsync(gas);

            mockCrypto.Setup(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.Verify(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal("Could not calc gas", invoice.ErrorMessage);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Equal(0, invoice.Attempts);
        }

        [Fact]
        public async void CalcGasThrowsExeptionTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal balance = r.Next();
            decimal gas = r.Next();
            string txid = r.Next().ToString();
            string errorMessage = r.Next().ToString();

            mockCrypto.Setup(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract))
                .ReturnsAsync(balance);

            mockCrypto.Setup(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance))
                .ThrowsAsync(new Exception(errorMessage));

            mockCrypto.Setup(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.Verify(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(errorMessage, invoice.ErrorMessage);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
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
            decimal balance = r.Next();
            decimal gas = r.Next();
            string txid = null;

            mockCrypto.Setup(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract))
                .ReturnsAsync(balance);

            mockCrypto.Setup(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance))
                .ReturnsAsync(gas);

            mockCrypto.Setup(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.Verify(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance));
            mockCrypto.Verify(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal("Could not send gas", invoice.ErrorMessage);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
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
            decimal balance = r.Next();
            decimal gas = r.Next();
            string txid = "";

            mockCrypto.Setup(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract))
                .ReturnsAsync(balance);

            mockCrypto.Setup(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance))
                .ReturnsAsync(gas);

            mockCrypto.Setup(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.Verify(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance));
            mockCrypto.Verify(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal("Could not send gas", invoice.ErrorMessage);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
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
            decimal balance = r.Next();
            decimal gas = r.Next();
            string txid = "    ";

            mockCrypto.Setup(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract))
                .ReturnsAsync(balance);

            mockCrypto.Setup(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance))
                .ReturnsAsync(gas);

            mockCrypto.Setup(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas))
                .ReturnsAsync(txid);

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.Verify(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance));
            mockCrypto.Verify(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal("Could not send gas", invoice.ErrorMessage);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Equal(0, invoice.Attempts);
        }

        [Fact]
        public async void SendTransactionThrowsExceptionTest()
        {
            // Arrange
            var mockCrypto = new Mock<ICryptoService>();
            var owner = CreateOwner();
            var service = new InvoiceService(mockCrypto.Object, owner);
            var invoice = CreateInvoice();
            

            Random r = new Random();
            decimal balance = r.Next();
            decimal gas = r.Next();
            string txid = r.Next().ToString();
            string errorMessage = r.Next().ToString();

            mockCrypto.Setup(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract))
                .ReturnsAsync(balance);

            mockCrypto.Setup(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance))
                .ReturnsAsync(gas);

            mockCrypto.Setup(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            await service.RefreshInvoiceAsync(invoice);

            // Assert
            mockCrypto.Verify(x=>x.GetTokenBalanceAsync(invoice.Account, invoice.Contract));
            mockCrypto.Verify(x=>x.CalcGasToWithdrawTokenAsync(invoice.Account, owner.Address, invoice.Contract, balance));
            mockCrypto.Verify(x=>x.SendTransactionAsync(owner, invoice.Account.Address, gas));
            mockCrypto.VerifyNoOtherCalls();

           
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Equal(TokenInvoiceState.Failed, invoice.State);
            Assert.Equal(errorMessage, invoice.ErrorMessage);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Equal(0, invoice.Attempts);
        }
    }
}
