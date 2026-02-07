using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CaManager.Models;
using CaManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaManager.Controllers
{
    [Authorize]
    /// <summary>
    /// Controller for managing Key Vault certificates (Root CAs).
    /// </summary>
    [Authorize]
    public class CertificatesController : Controller
    {
        private readonly IKeyVaultService _kvService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificatesController"/> class.
        /// </summary>
        /// <param name="kvService">The Key Vault service.</param>
        public CertificatesController(IKeyVaultService kvService)
        {
            _kvService = kvService;
        }

        /// <summary>
        /// Displays the list of certificates.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var certs = await _kvService.GetCertificatesAsync();
            return View(certs);
        }

        /// <summary>
        /// Displays the form to create a new Root CA.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateRootCaModel());
        }

        /// <summary>
        /// Handles the creation of a new Root CA.
        /// </summary>
        /// <param name="model">The create root CA model.</param>
        [HttpPost]
        public async Task<IActionResult> Create(CreateRootCaModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _kvService.CreateRootCaAsync(model.SubjectName, model.ValidityMonths, model.KeySize);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        /// <summary>
        /// Displays the form to import an existing Root CA.
        /// </summary>
        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        /// <summary>
        /// Handles the import of an existing Root CA.
        /// </summary>
        /// <param name="model">The import root CA model.</param>
        [HttpPost]
        public async Task<IActionResult> Import(ImportRootCaModel model)
        {
             if (!ModelState.IsValid) return View(model);

             using var stream = new MemoryStream();
             await model.CertificateFile.CopyToAsync(stream);
             var bytes = stream.ToArray();

             try
             {
                 await _kvService.ImportRootCaAsync(model.Name, bytes, model.Password);
                 return RedirectToAction(nameof(Index));
             }
             catch (Exception ex)
             {
                 ModelState.AddModelError("", ex.Message);
                 return View(model);
             }
        }

        /// <summary>
        /// Displays the list of certificates to choose a signer from.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SignStart()
        {
            var certs = await _kvService.GetCertificatesAsync();
            return View(certs);
        }

        /// <summary>
        /// Displays the form to upload or paste a CSR for a specific issuer.
        /// </summary>
        /// <param name="id">The name of the issuer certificate.</param>
        [HttpGet]
        public IActionResult Sign(string id)
        {
            // ID = Issuer Certificate Name
            ViewBag.IssuerName = id;
            return View();
        }

        /// <summary>
        /// Inspects a uploaded CSR and displays details for confirmation.
        /// </summary>
        /// <param name="issuerName">The name of the issuer certificate.</param>
        /// <param name="csrFile">The uploaded CSR file.</param>
        /// <param name="csrText">The pasted CSR text.</param>
        [HttpPost]
        public IActionResult Inspect(string issuerName, IFormFile? csrFile, string? csrText)
        {
            byte[] bytes;

            if (!string.IsNullOrWhiteSpace(csrText))
            {
                bytes = System.Text.Encoding.UTF8.GetBytes(csrText);
            }
            else if (csrFile != null && csrFile.Length > 0)
            {
                using var stream = new MemoryStream();
                csrFile.CopyTo(stream);
                bytes = stream.ToArray();
            }
            else
            {
                ModelState.AddModelError("", "Please upload a CSR file or paste the CSR content.");
                return View("Sign", new { id = issuerName });
            }

            try
            {
                // Attempt to parse CSR
                // PEM or DER? LoadSigningRequest handles both usually if using generic Load or we need to detect.
                // CertificateRequest.LoadSigningRequest accepts byte[]                
                var csr = CertificateRequest.LoadSigningRequest(bytes, HashAlgorithmName.SHA256, CertificateRequestLoadOptions.UnsafeLoadCertificateExtensions);
                
                var model = new InspectCsrModel
                {
                    IssuerName = issuerName,
                    SubjectDn = csr.SubjectName.Name ?? "Unknown",
                    PublicKeyInfo = csr.PublicKey.Oid.FriendlyName ?? "Unknown", // e.g. RSA
                    SignatureAlgorithm = csr.HashAlgorithm.Name ?? "Unknown",
                    CsrContentBase64 = Convert.ToBase64String(bytes),
                    ValidityMonths = 12
                };

                return View("Inspect", model);
            }
            catch (Exception ex)
            {
                 ModelState.AddModelError("", $"Error parsing CSR: {ex.Message}");
                 ViewBag.IssuerName = issuerName;
                 return View("Sign");
            }
        }

        /// <summary>
        /// Executes the signing of the CSR and returns the signed certificate.
        /// </summary>
        /// <param name="model">The inspect CSR model containing the approved details.</param>
        [HttpPost]
        public async Task<IActionResult> ExecuteSign(InspectCsrModel model)
        {
             try
             {
                 var csrBytes = Convert.FromBase64String(model.CsrContentBase64);
                 var cert = await _kvService.SignCsrAsync(model.IssuerName, csrBytes, model.ValidityMonths);

                 var certBytes = cert.Export(X509ContentType.Cert);
                 return File(certBytes, "application/x-x509-ca-cert", "signed-certificate.cer");
             }
             catch (Exception ex)
             {
                 ModelState.AddModelError("", ex.Message);
                 return View("Inspect", model);
             }
        }
    }
}
