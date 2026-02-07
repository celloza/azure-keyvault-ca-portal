using System.ComponentModel.DataAnnotations;

namespace CaManager.Models
{
    /// <summary>
    /// Model for creating a new self-signed Root CA certificate.
    /// </summary>
    public class CreateRootCaModel
    {
        /// <summary>
        /// The subject distinguished name.
        /// </summary>
        [Required]
        [Display(Name = "Subject DN (e.g. CN=MyRootCA)")]
        public string SubjectName { get; set; } = "CN=MyRootCA";

        /// <summary>
        /// The validity period in months.
        /// </summary>
        [Required]
        [Display(Name = "Validity (Months)")]
        public int ValidityMonths { get; set; } = 60;

        /// <summary>
        /// The RSA key size.
        /// </summary>
        [Required]
        [Display(Name = "Key Size")]
        public int KeySize { get; set; } = 4096;
    }

    /// <summary>
    /// Model for importing an existing Root CA certificate.
    /// </summary>
    public class ImportRootCaModel
    {
        /// <summary>
        /// The name to assign to the certificate in Key Vault.
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// The PFX file to upload.
        /// </summary>
        [Required]
        public IFormFile CertificateFile { get; set; } = null!;
        
        /// <summary>
        /// The password for the PFX file.
        /// </summary>
        public string? Password { get; set; }
    }

    /// <summary>
    /// Model for inspecting and collecting details to sign a CSR.
    /// </summary>
    public class InspectCsrModel
    {
        /// <summary>
        /// The name of the issuer certificate in Key Vault.
        /// </summary>
        public string IssuerName { get; set; } = null!;
        
        /// <summary>
        /// The subject distinguished name extracted from the CSR.
        /// </summary>
        public string SubjectDn { get; set; } = null!;

        /// <summary>
        /// Information about the public key in the CSR.
        /// </summary>
        public string PublicKeyInfo { get; set; } = null!;

        /// <summary>
        /// The signature algorithm used in the CSR.
        /// </summary>
        public string SignatureAlgorithm { get; set; } = null!;
        
        /// <summary>
        /// The raw CSR content in Base64 format.
        /// </summary>
        [Required]
        public string CsrContentBase64 { get; set; } = null!; // Pass between steps
        
        /// <summary>
        /// The validity period for the new certificate in months.
        /// </summary>
        [Required]
        [Display(Name = "Validity for New Cert (Months)")]
        public int ValidityMonths { get; set; } = 12;
    }
}
