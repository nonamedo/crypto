namespace Nonamedo.Crypto.Service
{
    public class CryptoAccount
    {
        public string Address {get; private set;}
        public string PublicKey {get; private set;}
        public string PrivateKey {get; private set;}
        
        public int? PermissionId { get; private set; }

        public CryptoAccount(string address, string publicKey, string privateKey, int? permissionId = null)
        {
            this.Address = address;
            this.PublicKey = publicKey;
            this.PrivateKey = privateKey;
            this.PermissionId = permissionId;
        }
    }
}