using System;
using Nonamedo.Crypto.Service;

namespace Nonamedo.Crypto.Invoice
{
    public enum TokenInvoiceState
    {
        Failed = -1,
        Created,
        Expired,
        

        GasSent, // after a token received
        TokenWithdrawn, // after a gas received
        GasWithdrawn, // after a token withdrawn
        Completed, 
    } 


    public class TokenInvoice
    {
        public int Id {get; private set;}
        public CryptoAccount Account {get;private set;}
        public DateTime ExpiredAt {get;private set;}
        public decimal ExpectedTokenAmount {get;private set;}
        public string Contract {get;private set;}


        // changable
        public TokenInvoiceState State {get;private set;}
        public DateTime StateChangedAt {get; private set;}
        public decimal ActualTokenAmount {get;private set;}
        public string InGasHash {get;private set;}
        public string OutTokenHash {get;private set;}
        public string OutGasHash {get;private set;}

        public int Attempts {get; private set;}
        

        public string ErrorMessage {get; private set;}

        public bool IsStoped()
        {
            return this.State == TokenInvoiceState.Failed || 
                this.State == TokenInvoiceState.Completed ||
                this.State == TokenInvoiceState.Expired;
        }


        public TokenInvoice(string contract, CryptoAccount account, decimal tokenAmount, TimeSpan lifeTime)
        {
            this.Account = account;
            this.Contract = contract;
            this.ExpectedTokenAmount = tokenAmount;
            
            this.ExpiredAt= DateTime.SpecifyKind(DateTime.UtcNow.Add(lifeTime), DateTimeKind.Utc);
            this.State = TokenInvoiceState.Created;
        }

        public TokenInvoice(int id, CryptoAccount account, DateTime expiredAt, decimal expectedTokenAmount, string contract,
            TokenInvoiceState state, DateTime stateChangedAt, decimal actualTokenAmount, string inGasHash, string outTokenHash, 
            string outGasHash, int attempts, string errorMessage)
            {
                this.Id = id;
                this.Account = account;
                this.ExpiredAt = expiredAt;
                this.ExpectedTokenAmount = expectedTokenAmount;
                this.Contract = contract;
                this.State = state;
                this.StateChangedAt = stateChangedAt;
                this.ActualTokenAmount = actualTokenAmount;
                this.InGasHash = inGasHash;
                this.OutTokenHash = outTokenHash;
                this.OutGasHash = outGasHash;
                this.Attempts = attempts;
                this.ErrorMessage = errorMessage;
            }

        

        void ChangeState(TokenInvoiceState newState)
        {
            this.State = newState;
            this.StateChangedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            this.Attempts = 0;
        }

        public void AddAttempt()
        {
            this.Attempts++; 
        }



        public void Expire()
        {
            if (this.State != TokenInvoiceState.Created)
                throw new InvalidOperationException();

            if (this.ExpiredAt > DateTime.UtcNow)
                throw new InvalidOperationException();

            this.ChangeState(TokenInvoiceState.Expired);
        }

        public void SendGas(decimal balance, string inGasHash)
        {
             if (this.State != TokenInvoiceState.Created)
                throw new InvalidOperationException();
            
            
            this.ActualTokenAmount = balance;
            this.InGasHash = inGasHash;
            this.ChangeState(TokenInvoiceState.GasSent);
        }

        public void WithdrawToken(string outTokenHash)
        {
            if (this.State != TokenInvoiceState.GasSent)
                throw new InvalidOperationException();
            
            
            this.OutTokenHash = outTokenHash;
            this.ChangeState(TokenInvoiceState.TokenWithdrawn);
        }

        public void WithdrawGas(string outGasHash)
        {
            if (this.State != TokenInvoiceState.TokenWithdrawn)
                throw new InvalidOperationException();
            
            
            this.OutGasHash = outGasHash;
            this.ChangeState(TokenInvoiceState.GasWithdrawn);
        }

        public void Complete()
        {
            if (this.State!= TokenInvoiceState.TokenWithdrawn && this.State != TokenInvoiceState.GasWithdrawn)
                throw new InvalidOperationException();
            
            this.ChangeState(TokenInvoiceState.Completed);
        }

        public void Fail(string errorMessage)
        {
              if (this.IsStoped())
                throw new InvalidOperationException();

            this.ErrorMessage = errorMessage;
            this.ChangeState(TokenInvoiceState.Failed);
        }
     
    }
}
