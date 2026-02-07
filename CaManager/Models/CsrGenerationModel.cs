using System.ComponentModel.DataAnnotations;

namespace CaManager.Models
{
    /// <summary>
    /// Model used for generating a Certificate Signing Request (CSR).
    /// </summary>
    public class CsrGenerationModel
    {
        /// <summary>
        /// The Common Name (CN) of the certificate subject.
        /// </summary>
        [Required]
        [Display(Name = "Common Name (CN)")]
        public string CommonName { get; set; } = "";

        /// <summary>
        /// The Organization (O) name.
        /// </summary>
        [Display(Name = "Organization (O)")]
        public string? Organization { get; set; }

        /// <summary>
        /// The Organizational Unit (OU) name.
        /// </summary>
        [Display(Name = "Organizational Unit (OU)")]
        public string? OrganizationalUnit { get; set; }

        /// <summary>
        /// The Locality (L) name (City).
        /// </summary>
        [Display(Name = "Locality (L)")]
        public string? Locality { get; set; }

        /// <summary>
        /// The State or Province (S) name.
        /// </summary>
        [Display(Name = "State (S)")]
        public string? State { get; set; }

        /// <summary>
        /// The two-letter Country (C) code.
        /// </summary>
        [Display(Name = "Country (C)")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string? Country { get; set; }

        /// <summary>
        /// The size of the RSA key to generate (e.g., 2048, 4096).
        /// </summary>
        [Required]
        [Display(Name = "Key Size")]
        public int KeySize { get; set; } = 2048;
    }
}
