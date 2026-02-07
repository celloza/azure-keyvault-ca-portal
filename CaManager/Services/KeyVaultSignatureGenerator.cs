using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Formats.Asn1;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace CaManager.Services
{
    /// <summary>
    /// Custom X509SignatureGenerator that delegates the signing operation to Azure Key Vault.
    /// This allows us to sign data (like a CSR) using a private key that never leaves the vault.
    /// </summary>
    public class KeyVaultSignatureGenerator : X509SignatureGenerator
    {
        private readonly CryptographyClient _cryptoClient;
        private readonly X509Certificate2 _issuerCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultSignatureGenerator"/> class.
        /// </summary>
        /// <param name="cryptoClient">The Key Vault CryptographyClient for the signing key.</param>
        /// <param name="issuerCertificate">The public certificate of the issuer.</param>
        public KeyVaultSignatureGenerator(CryptographyClient cryptoClient, X509Certificate2 issuerCertificate)
        {
            _cryptoClient = cryptoClient;
            _issuerCertificate = issuerCertificate;
        }

        /// <inheritdoc/>
        protected override PublicKey BuildPublicKey()
        {
            // Return Issuer's PublicKey
            return _issuerCertificate.PublicKey;
        }

        /// <summary>
        /// Gets the algorithm identifier for the specified hash algorithm.
        /// </summary>
        /// <param name="hashAlgorithm">The hash algorithm to use.</param>
        /// <returns>The encoded algorithm identifier.</returns>
        public override byte[] GetSignatureAlgorithmIdentifier(HashAlgorithmName hashAlgorithm)
        {
            // OID for sha256WithRSAEncryption: 1.2.840.113549.1.1.11
            // This method expects the full AlgorithmIdentifier ASN.1 sequence, not just the OID.
            // However, X509SignatureGenerator usage often varies. The .NET implementation for RSAPKCS1SignatureGenerator
            // returns the AlgorithmIdentifier sequence containing the OID and NULL parameters.
            
            if (hashAlgorithm == HashAlgorithmName.SHA256)
            {
                var writer = new AsnWriter(AsnEncodingRules.DER);
                writer.PushSequence();
                writer.WriteObjectIdentifier("1.2.840.113549.1.1.11"); // sha256WithRSAEncryption
                writer.WriteNull();
                writer.PopSequence();
                return writer.Encode();
            }
            throw new NotSupportedException($"Hash algorithm {hashAlgorithm} is not supported. Only SHA256 is implemented.");
        }

        /// <summary>
        /// Signs the data using the Key Vault CryptographyClient.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use.</param>
        /// <returns>The signature.</returns>
        public override byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm)
        {
            if (hashAlgorithm != HashAlgorithmName.SHA256)
            {
                throw new NotSupportedException($"Hash algorithm {hashAlgorithm} is not supported. Only SHA256 is implemented.");
            }

            // Remote Sign using Key Vault
            // Hash the data first, then Sign.
            using var hasher = SHA256.Create();
            var digest = hasher.ComputeHash(data);

            var result = _cryptoClient.SignAsync(SignatureAlgorithm.RS256, digest).GetAwaiter().GetResult();
            return result.Signature;
        }
    }
}
