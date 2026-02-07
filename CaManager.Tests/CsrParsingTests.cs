using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace CaManager.Tests
{
    public class CsrParsingTests
    {
        [Fact]
        public void CanParseProvidedCsr()
        {
            var csrPem = @"-----BEGIN CERTIFICATE REQUEST-----
MIIEkTCCAnkCAQAwTDELMAkGA1UEBhMCVUsxFDASBgNVBAoMC05IUyBEaWdpdGFs
MRAwDgYDVQQLDAdOSFMgVlJTMRUwEwYDVQQDDAxQcm94eSBDbGllbnQwggIiMA0G
CSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQC2uRAaTgR+TI4LnlUaTxVU094BjlAt
MtAgurLwu7NWIgqfi0+52qwijzdO2rp/fItkfhU+MIhRvQLNIG319IVyKrXHz5lK
/yfK7wUz77U82nKByPjMCHHyhH/xGsd4hSc5jObtYISNYDgfOzV7F2bgz1SZMauT
OGkjywqqwnPmYOlq7U8/a8oyBEYeKhkssgqBJ2f6ERvrmsiHkwe/OfjYEPqizDaG
b8rpLoveuSQ3NcdJdx9Fj4fpbQOgQJQdMEa4wbJUsRhottmTTsGUg3SLbLwPxGnV
sPvGlXHZm5RFSURsTN1dCP/2Xk4Q1EaVgQlXwfg5hZ/crP2AjN9phGH+FAiirlbu
H7/Zi0j1Rt/v8j9tRBMCfhdmp6IeWfAd/po0prq/oRwgKA2J8FQPP15EL/noAB98
R0mGfgLOCgzClxG5qL+2S0Em6Fwmf+UIOp7s69Hww7K0faa8tRj8uPjCdukhlIkL
RU+DyBvleipUafov7IiUO1s1aiJF4Y1/XtiKsPWe29h1gx/8Z5hrwRQkZ+6NkRfg
mdH5g7xLlBkGdDF7b4rynEaJBMsL7zo0lpnA6lVnydlmKISc5LMhmRU90Aqdc2Ou
JM8Iqu5Xsoc+RqqKQaPHvpC9UcHgEHa/ppNWwppf9GlRz3hy8g8+gptD1VE8FxK8
crg1g+tGwJ4JvQIDAQABoAAwDQYJKoZIhvcNAQELBQADggIBAJ+meGYbPYmT1AXL
Xt6JsRHsBkKRKxR7uC3kngxc08XKQTDZSw0GXaNkRi/abYzKPf6PDlb5u8Md3Q+A
zHUSQTj/gC3DonsZr23SugBLGdAj62WN01E3cjyD6F/ABNmIWYYNEWKjMkORg0dh
FN9c+tyVc63wXiei/1uYMVEWfDI7tNkYll7gxNoHYiuwxXUUY4LTxvXmhaz4FOSX
gh4ejrIJXFnYm/WBAsjp9G+j/lltoTKSJx/l084T+Ne9wDsSwZeQj2+kIJ/ClicH
whqnpoyz/Bc31FJHCG31TwUiangPvDsV0SNvug9wZX58LTkadQR059YzecNTFagT
bsVaOzJTw2CPvyHD+Rrd+dQJ5m02mJ0xVSG9k8akilhL7CRvE5M87CRCSBvTIamy
tfWOZvhxuA4nO4Ypni0d6j0xvbp+k2HbUYOUM2WQ/DDzs50hFDc6EPy37Gu3tkUU
BYSq6YPtNiYgfd8grYny62c9x15vcmk2gNtyWoqzPVKW5LbvH8cUO7j938LeguVV
d9DQ0N4A8yqURG52NAaiL8bkN5+FY3ui4bBO5dsJeA69uvG0M7P7H2msVUi0zZV7
qkbO5VEBvz70QUaujNNYAnQWnm0HgTXFkZhFm8J0V0xUtDknJfeaxIqa+eLo3OUI
5+4GCmO0w0pkrd/+/ovCXnNQTCos
-----END CERTIFICATE REQUEST-----";

            try
            {
                byte[] derBytes;
                if (csrPem.Contains("-----BEGIN"))
                {
                     var sb = new StringBuilder();
                    var lines = csrPem.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (!line.StartsWith("-----"))
                        {
                            sb.Append(line.Trim());
                        }
                    }
                    derBytes = Convert.FromBase64String(sb.ToString());
                }
                else
                {
                    derBytes = Convert.FromBase64String(csrPem);
                }

                var csr = CertificateRequest.LoadSigningRequest(derBytes, HashAlgorithmName.SHA256, CertificateRequestLoadOptions.UnsafeLoadCertificateExtensions);
                Assert.NotNull(csr);
                Assert.NotNull(csr.SubjectName);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Parsing Failed: {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
}
