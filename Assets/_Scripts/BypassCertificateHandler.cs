using UnityEngine.Networking;

// --- WARNING: DEVELOPMENT ONLY ---
// This class is used to bypass SSL certificate validation for localhost.
// Do NOT use this in a production build with a real server.
public class BypassCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Always return true to trust the certificate
        return true;
    }
}
