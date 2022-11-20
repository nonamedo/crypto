using System;
using Moq;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Service;
using Xunit;

namespace Common.Finance.Tests
{
    public class TokenInvoiceTests
    {
        [Fact]
        public void Test1()
        {
            // arrange
            var r = new Random();
            string contract = r.Next().ToString();
            string accountPrivateKey = r.Next().ToString();
            string accountAddress = r.Next().ToString();
            string accountPublicKey = r.Next().ToString();

            var account = new CryptoAccount(
                address: accountAddress,
                publicKey: accountPublicKey,
                privateKey: accountPrivateKey);

            decimal tokenAmount = r.Next();
            TimeSpan lifetime = TimeSpan.FromHours(r.Next(0,24));


            // act
            var invoice = new TokenInvoice(
                contract: contract,
                account: account,
                tokenAmount:tokenAmount,
                lifeTime: lifetime);

            // assert
            Assert.Equal(0, invoice.Id);
            Assert.Equal(accountAddress, invoice.Account.Address);
            Assert.Equal(accountPrivateKey, invoice.Account.PrivateKey);
            Assert.Equal(accountPublicKey, invoice.Account.PublicKey);
            Assert.True(invoice.ExpiredAt > DateTime.UtcNow);
            Assert.Equal(tokenAmount, invoice.ExpectedTokenAmount);
            Assert.Equal(contract, invoice.Contract);
            Assert.Equal(TokenInvoiceState.Created, invoice.State);
            Assert.True(invoice.StateChangedAt < DateTime.UtcNow);
            Assert.Equal(0, invoice.ActualTokenAmount);
            Assert.Null(invoice.InGasHash);
            Assert.Null(invoice.OutTokenHash);
            Assert.Null(invoice.OutGasHash);
            Assert.Equal(0, invoice.Attempts);
            Assert.Null(invoice.ErrorMessage);
        }

        [Fact]
        public void Test2()
        {
            // arrange
            var r = new Random();
            int id = r.Next();
            string contract = r.Next().ToString();
            string accountPrivateKey = r.Next().ToString();
            string accountAddress = r.Next().ToString();
            string accountPublicKey = r.Next().ToString();

            var account = new CryptoAccount(
                address: accountAddress,
                publicKey: accountPublicKey,
                privateKey: accountPrivateKey);

            TimeSpan lifetime = TimeSpan.FromHours(r.Next(0,24));
            DateTime expiredAt = DateTime.UtcNow.Add(lifetime);
            decimal expectedTokenAmount = r.Next();
            TokenInvoiceState state = (TokenInvoiceState)r.Next(7);
            DateTime stateChangedAt = DateTime.UtcNow;
            decimal actualTokenAmount = r.Next();
            string inGasHash = r.Next().ToString();
            string outTokenHash = r.Next().ToString();
            string outGasHash = r.Next().ToString();
            int attempts = r.Next();
            string errorMessage = r.Next().ToString();


            // act
            var invoice = new TokenInvoice(
                id: id,
                account: account,
                expiredAt: expiredAt,
                expectedTokenAmount: expectedTokenAmount,
                contract: contract,
                state: state,
                stateChangedAt: stateChangedAt,
                actualTokenAmount: actualTokenAmount,
                inGasHash:inGasHash,
                outTokenHash: outTokenHash,
                outGasHash: outGasHash,
                attempts: attempts,
                errorMessage: errorMessage);



            // assert
            Assert.Equal(id, invoice.Id);
            Assert.Equal(accountAddress, invoice.Account.Address);
            Assert.Equal(accountPrivateKey, invoice.Account.PrivateKey);
            Assert.Equal(accountPublicKey, invoice.Account.PublicKey);
            Assert.Equal(expiredAt, invoice.ExpiredAt);
            Assert.Equal(expectedTokenAmount, invoice.ExpectedTokenAmount);
            Assert.Equal(contract, invoice.Contract);
            Assert.Equal(state, invoice.State);
            Assert.Equal(stateChangedAt, invoice.StateChangedAt);
            Assert.Equal(actualTokenAmount, invoice.ActualTokenAmount);
            Assert.Equal(inGasHash, invoice.InGasHash);
            Assert.Equal(outTokenHash, invoice.OutTokenHash);
            Assert.Equal(outGasHash, invoice.OutGasHash);
            Assert.Equal(attempts, invoice.Attempts);
            Assert.Equal(errorMessage, invoice.ErrorMessage);
        }

        [Fact]
        public void AddAttemptTest()
        {
            // arrange
            var r = new Random();
            string contract = r.Next().ToString();
            string accountPrivateKey = r.Next().ToString();
            string accountAddress = r.Next().ToString();
            string accountPublicKey = r.Next().ToString();

            var account = new CryptoAccount(
                address: accountAddress,
                publicKey: accountPublicKey,
                privateKey: accountPrivateKey);

            decimal tokenAmount = r.Next();
            TimeSpan lifetime = TimeSpan.FromHours(r.Next(0,24));

            var invoice = new TokenInvoice(
                contract: contract,
                account: account,
                tokenAmount:tokenAmount,
                lifeTime: lifetime);

            int attempts = r.Next(100);


            // act
            for (var i=0; i<attempts; i++)
            {
                invoice.AddAttempt();    
            }
            

            // assert
            Assert.Equal(attempts, invoice.Attempts);
        }
    }
}