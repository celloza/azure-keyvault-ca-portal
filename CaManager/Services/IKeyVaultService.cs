using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Certificates;

namespace CaManager.Services
{
    /// <summary>
    /// Service for interacting with Azure Key Vault certificates and keys.
    /// </summary>
    public interface IKeyVaultService
    {
        /// <summary>
        /// Retrieves a list of all certificates in the Key Vault, including their policy details.
        /// </summary>
        Task<List<KeyVaultCertificateWithPolicy>> GetCertificatesAsync();

        /// <summary>
        /// Retrieves a specific certificate by name.
        /// </summary>
        /// <param name="name">The name of the certificate.</param>
        Task<KeyVaultCertificateWithPolicy> GetCertificateAsync(string name);

        /// <summary>
        /// Creates a new self-signed Root CA certificate in the Key Vault.
        /// </summary>
        /// <param name="subjectName">The subject distinguished name (e.g., "CN=MyRootCA").</param>
        /// <param name="validityMonths">The validity period in months.</param>
        /// <param name="keySize">The RSA key size (e.g., 2048, 4096).</param>
        Task<KeyVaultCertificateWithPolicy> CreateRootCaAsync(string subjectName, int validityMonths, int keySize);

        /// <summary>
        /// Imports an existing Root CA certificate (PFX) into the Key Vault.
        /// </summary>
        /// <param name="certificateName">The name to assign to the certificate in Key Vault.</param>
        /// <param name="pfxBytes">The byte content of the PFX file.</param>
        /// <param name="password">The password for the PFX file, if any.</param>
        Task<KeyVaultCertificateWithPolicy> ImportRootCaAsync(string certificateName, byte[] pfxBytes, string? password);

        /// <summary>
        /// Signs a Certificate Signing Request (CSR) using a Root CA stored in Key Vault.
        /// </summary>
        /// <param name="issuerCertName">The name of the issuer certificate in Key Vault.</param>
        /// <param name="csrBytes">The raw bytes of the CSR.</param>
        /// <param name="validityMonths">The validity period for the signed certificate.</param>
        /// <returns>The signed X509Certificate2.</returns>
        Task<X509Certificate2> SignCsrAsync(string issuerCertName, byte[] csrBytes, int validityMonths);

        /// <summary>
        /// Deletes a certificate from the Key Vault.
        /// </summary>
        /// <param name="name">The name of the certificate to delete.</param>
        Task DeleteCertificateAsync(string name);
    }
}
