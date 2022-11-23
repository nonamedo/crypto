# Nonamedo.Crypto
Nonamedo.Crypto is a simple project that helps you creating and processing Crypto-Invoices to get payments in crypto and for many other features.

## Install
To install the needed NuGet package apply:

```
Install-Package Nonamedo.Crypto
```

## Usage: Creating an invoice
To create a crypto-token invoice you can use the following code:

```csharp

using Nonamedo.Crypto.Factories;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Invoice.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Services;

decimal expectedTokenAmount = 15; 
TimeSpan waitTime = TimeSpan.FromDays(1);


// tron:
/*ICryptoService cryptoService = CryptoServiceFactory.CreateTronService( 
    fullNode: "http://35.180.51.163:8090", // public node
    solidityNode: "http://35.180.51.163:8091", // public node
    httpClient: new HttpClient());

// Funds will be transferd here from the invoice's temp account

// We can create a new acc like the following: var newAcc = cryptoService.GenerateAccountAsync().GetAwaiter().GetResult();
CryptoAccount destinationAcc = new CryptoAccount(
    address: "TRtCNbKpz66rrgxWqYJ8hmsEB8Z8L9we82",
    publicKey: "0497C6CA5C7E450D1D693EF2497CC2758D68407ABC603FE79C99DF442FD56C2341CF2AEBDB087C76990E70D98AA4188EA35935C2A99AF08CE8F0965A3C8CE94354",
    privateKey: "c6129b04261d3cb26fad45b544807abf22392d83306345054fb06895ab291935");*/


// ethereum
string yourApiKey = "<infura.io api key>"; // to get api key you have to sign up on infura.io
ICryptoService cryptoService = CryptoServiceFactory.CreateEthereumService( 
     node: "https://mainnet.infura.io/v3/" + yourApiKey);

CryptoAccount destinationAcc = new CryptoAccount(
    address: "0xD33bEea45180E323619C7dec2a1Fba4b692747b1",
    publicKey: "043d6e18fade6447227063d442ffefdef0cf6c736413765fb6a8cb74b91c25b913f60068ae380050bf5a2d0c8024da28dd1e2df54c5e7330f055ab811bd3d8ada4",
    privateKey: "0xa890fc7df898d1eb791025704d17eaba6ddb4668ea77893b8ac5958e46f3a4da");


IInvoiceService invoiceService = new InvoiceService(cryptoService, destinationAcc); 

// creating the invoice
TokenInvoice invoice = invoiceService.CreateUsdtInvoiceAsync(
    tokenAmount: expectedTokenAmount,
    lifeTime: waitTime,
    account: null).GetAwaiter().GetResult(); 

/* if account is null, then a new temp account will be created for the invoice, 
otherwise specify the existing one where you going to receive the payment from a client.
If you specify an existing account, make sure the balance is 0 
*/

// processing   
while (!invoice.IsStoped())
{
    invoiceService.ProcessInvoiceAsync(invoice).GetAwaiter().GetResult();

    if (!invoice.IsStoped())
    {
        Thread.Sleep(1000 * 20); // 20 sec
    }
}           

```
	  
