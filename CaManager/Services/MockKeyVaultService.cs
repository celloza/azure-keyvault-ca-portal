using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Certificates;

#if DEBUG
namespace CaManager.Services
{
    /// <inheritdoc cref="IKeyVaultService"/>
    /// <summary>
    /// Mock implementation of <see cref="IKeyVaultService"/> for local development and testing.
    /// Does not interact with Azure Key Vault.
    /// </summary>
    public class MockKeyVaultService : IKeyVaultService
    {
        private readonly List<KeyVaultCertificateWithPolicy> _certificates = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MockKeyVaultService"/> class.
        /// Seeds the service with a dummy certificate.
        /// </summary>
        public MockKeyVaultService()
        {
            // Seed with a dummy mock certificate
            _certificates.Add(CreateMockCertificate("mock-root-ca", "CN=Mock Root CA"));
        }

        /// <inheritdoc/>
        public Task<List<KeyVaultCertificateWithPolicy>> GetCertificatesAsync()
        {
            return Task.FromResult(_certificates);
        }

        /// <inheritdoc/>
        public Task<KeyVaultCertificateWithPolicy> GetCertificateAsync(string name)
        {
            var cert = _certificates.FirstOrDefault(c => c.Name == name);
            if (cert == null)
            {
                throw new Exception($"Certificate {name} not found");
            }
            return Task.FromResult(cert);
        }

        /// <inheritdoc/>
        public Task<KeyVaultCertificateWithPolicy> CreateRootCaAsync(string subjectName, int validityMonths, int keySize)
        {
            var name = $"root-{Guid.NewGuid().ToString().Substring(0, 8)}";
            var cert = CreateMockCertificate(name, subjectName);
            _certificates.Add(cert);
            return Task.FromResult(cert);
        }

        /// <inheritdoc/>
        public Task<KeyVaultCertificateWithPolicy> ImportRootCaAsync(string certificateName, byte[] pfxBytes, string? password)
        {
            // Parse the PFX to get actual details
            using var certObj = new X509Certificate2(pfxBytes, password, X509KeyStorageFlags.Exportable);
            
            var cert = CreateMockCertificate(
                certificateName, 
                certObj.Subject, 
                certObj.Thumbprint, 
                certObj.NotAfter
            );
            _certificates.Add(cert);
            return Task.FromResult(cert);
        }

        /// <inheritdoc/>
        public Task<X509Certificate2> SignCsrAsync(string issuerCertName, byte[] csrBytes, int validityMonths)
        {
            // 1. Get the Issuer details
            var issuerCert = _certificates.FirstOrDefault(c => c.Name == issuerCertName);
            if (issuerCert == null) throw new Exception($"Issuer certificate {issuerCertName} not found");

            // 2. Parse the CSR
            var csr = CertificateRequest.LoadSigningRequest(csrBytes, HashAlgorithmName.SHA256, CertificateRequestLoadOptions.UnsafeLoadCertificateExtensions);

            // 3. Create a temporary "CA Key" to act as the signer (since we mock the private key)
            using var issuerKey = RSA.Create(2048);

            // 4. Create a generator for the Issuer
            // Note: We need an X509Certificate2 to represent the issuer for the 'Create' method
            // So we create a self-signed cert for the issuer *just to pass to the generator*
            var issuerRequest = new CertificateRequest(new X500DistinguishedName(issuerCert.Policy.Subject), issuerKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var issuerX509 = issuerRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddYears(1));

            // 5. Generate Serial Number
            var serialNumber = new byte[8];
            RandomNumberGenerator.Fill(serialNumber);

            // 6. Sign!
            var notBefore = DateTimeOffset.UtcNow;
            var notAfter = notBefore.AddMonths(validityMonths);

            var cert = csr.Create(
                issuerX509.SubjectName, // Use the Issuer's Subject Name as the Issuer Name
                X509SignatureGenerator.CreateForRSA(issuerKey, RSASignaturePadding.Pkcs1),
                notBefore,
                notAfter,
                serialNumber
            );

            return Task.FromResult(cert);
        }

        /// <inheritdoc/>
        public Task DeleteCertificateAsync(string name)
        {
            var cert = _certificates.FirstOrDefault(c => c.Name == name);
            if (cert != null)
            {
                _certificates.Remove(cert);
            }
            return Task.CompletedTask;
        }

        private KeyVaultCertificateWithPolicy CreateMockCertificate(string name, string subjectName, string? thumbprintHex = null, DateTime? expires = null)
        {
            byte[] thumbprintBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            if (!string.IsNullOrEmpty(thumbprintHex))
            {
                thumbprintBytes = Convert.FromHexString(thumbprintHex);
            }

            // Use CertificateModelFactory to create an instance of the model
            var props = CertificateModelFactory.CertificateProperties(
                    name: name,
                    id: new Uri($"https://mock-kv.vault.azure.net/certificates/{name}"),
                    x509thumbprint: thumbprintBytes,
                    expiresOn: expires ?? DateTimeOffset.UtcNow.AddYears(1),
                    createdOn: DateTimeOffset.UtcNow
                );
            props.Enabled = true;

            return CertificateModelFactory.KeyVaultCertificateWithPolicy(
                properties: props,
                policy: CertificateModelFactory.CertificatePolicy(
                    issuerName: "Self",
                    subject: subjectName
                ),
                cer: new byte[0] 
            );
        }
    }
}
#endif
