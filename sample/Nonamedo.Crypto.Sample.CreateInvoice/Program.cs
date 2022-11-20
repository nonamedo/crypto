using Nonamedo.Crypto.Factories;
using Nonamedo.Crypto.Invoice;
using Nonamedo.Crypto.Invoice.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Services;

decimal expectedTokenAmount = 15; 
TimeSpan waitTime = TimeSpan.FromDays(1);

ICryptoService cryptoService = CryptoServiceFactory.CreateTronService( 
    fullNode: "http://35.180.51.163:8090", // public node
    solidityNode: "http://35.180.51.163:8091", // public node
    httpClient: new HttpClient());

// Funds will be transferd here from the invoice's temp account
/*
We can create a new acc like the following:
var newAcc = cryptoService.GenerateAccountAsync().GetAwaiter().GetResult();
*/
CryptoAccount destinationAcc = new CryptoAccount(
    address: "TRtCNbKpz66rrgxWqYJ8hmsEB8Z8L9we82",
    publicKey: "0497C6CA5C7E450D1D693EF2497CC2758D68407ABC603FE79C99DF442FD56C2341CF2AEBDB087C76990E70D98AA4188EA35935C2A99AF08CE8F0965A3C8CE94354",
    privateKey: "c6129b04261d3cb26fad45b544807abf22392d83306345054fb06895ab291935");

IInvoiceService invoiceService = new InvoiceService(cryptoService, destinationAcc); 

// creating the invoice
TokenInvoice invoice = invoiceService.CreateUsdtInvoiceAsync(
    tokenAmount: expectedTokenAmount,
    lifeTime: waitTime,
    account: null).GetAwaiter().GetResult(); // if account is null, then a new temp account will be created for the invoice

// processing   
while (!invoice.IsStoped())
{
    invoiceService.ProcessInvoiceAsync(invoice).GetAwaiter().GetResult();

    if (!invoice.IsStoped())
    {
        Thread.Sleep(1000 * 20); // 20 sec
    }
}
            