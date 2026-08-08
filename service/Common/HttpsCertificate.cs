// SPDX-License-Identifier: GPL-3.0-or-later

using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Vistava.Service.Common;

public class HttpsCertificate
{
	public X509Certificate2 Certificate { get; }

	public HttpsCertificate(X509Certificate2 certificate) => 
		Certificate = certificate;

	public HttpsCertificate(byte[] certificateRawData) => 
		Certificate = new(certificateRawData);

	/// <summary>
	/// Exports the certificate and the private key in the PFX format.
	/// Commonly used file extensions for this format are <c>pfx</c> and <c>p12</c>.
	/// </summary>
	/// <returns>The certificate file as <see cref="byte"/> array.</returns>
	public byte[] ExportAsPfx()
	{
		return Certificate.Export(X509ContentType.Pfx);
	}

	/// <summary>
	/// Exports the certificate as binary file in the DER format.
	/// The commonly used file extension for this format is <c>der</c>.
	/// This format is the binary equivalent of the format used by <see cref="ExportAsPem"/>.
	/// </summary>
	/// <returns>The certificate file as <see cref="byte"/> array.</returns>
	public byte[] ExportAsDer()
	{
		return Certificate.Export(X509ContentType.Cert);
	}

	/// <summary>
	/// Exports the certificate as base-64 encoded ASCII file in the PEM format.
	/// Commonly used file extensions for this format are <c>pem</c>, <c>cer</c> and 
	/// <c>crt</c>.
	/// This format is the ASCII equivalent of the format used by <see cref="ExportAsDer"/>.
	/// </summary>
	/// <param name="includePrivateKey">
	/// <c>true</c> to include the private key into the exported file,
	/// <c>false</c> (default) to only export the public certificate.
	/// </param>
	/// <returns>The certificate file as <see cref="byte"/> array.</returns>
	/// <exception cref="InvalidOperationException">
	/// Is thrown when the private key couldn't be exported.
	/// </exception>
	public byte[] ExportAsPem(bool includePrivateKey = false)
	{
		var certificateData = Certificate.Export(X509ContentType.Cert);
		var certificateDataBase64 = Convert.ToBase64String(certificateData,
			Base64FormattingOptions.InsertLineBreaks);
			
			
		StringBuilder builder = new();

		if (includePrivateKey) 
		{
			var privateKey = Certificate.GetECDsaPrivateKey();
			var privateKeyData = privateKey?.ExportPkcs8PrivateKeyPem();
			if (privateKeyData != null)
			{
				builder.AppendLine(privateKeyData);
			}
			else
			{
				throw new InvalidOperationException("The private key couldn't be exported.");
			}
		}
			
		builder.AppendLine("-----BEGIN CERTIFICATE-----");
		builder.AppendLine(certificateDataBase64);
		builder.AppendLine("-----END CERTIFICATE-----");			

		return Encoding.ASCII.GetBytes(builder.ToString());
	}
	
	/// <summary>
	/// Imports a <see cref="HttpsCertificate"/> from a <c>.cer</c> or <c>.pfx</c> file.
	/// </summary>
	/// <param name="path">The path to the certificate file.</param>
	/// <returns>A new <see cref="HttpsCertificate"/> instance.</returns>
	/// <exception cref="FileNotFoundException">
	/// Is thrown when the file under the specified <paramref name="path"/> wasn't found.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Is thrown when the specified certificate file couldn't be imported.
	/// </exception>
	public static HttpsCertificate Import(string path)
	{
		if (!File.Exists(path))
		{
			throw new FileNotFoundException("The specified certificate file wasn't found.");
		}
		try
		{
			var certificate = X509Certificate.CreateFromCertFile(path);
			var certificatev2 = new X509Certificate2(certificate);
			return new HttpsCertificate(certificatev2);
		}
		catch (Exception exc)
		{
			throw new InvalidOperationException("The certificate couldn't be loaded.", exc);
		}
	}

	public static implicit operator X509Certificate2(HttpsCertificate cert)
	{
		return cert.Certificate;
	}
}
