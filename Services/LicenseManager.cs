using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HMI_ScrewingMonitor.Services
{
    /// <summary>
    /// Quản lý license: Load, Save, Validate
    /// Trial mode: 10 phút mỗi lần chạy
    /// </summary>
    public class LicenseManager
    {
        private const string LICENSE_FILE_NAME = "license.dat";

        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HMI_ScrewingMonitor"
        );

        private static readonly string LicenseFilePath = Path.Combine(AppDataFolder, LICENSE_FILE_NAME);

        // AES encryption key - 16 bytes (128-bit AES)
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("HMISCREWING_2025");

        public string LicenseKey { get; private set; }
        public string CompanyName { get; private set; }
        public DateTime? ExpiryDate { get; private set; }
        public bool IsLicensed { get; private set; }
        public string HardwareId { get; private set; }

        public LicenseManager()
        {
            Initialize();
        }

        /// <summary>
        /// Khởi tạo và kiểm tra license
        /// </summary>
        private void Initialize()
        {
            try
            {
                // Lấy Hardware ID
                HardwareId = HardwareInfo.GetHardwareId();

                // Ensure app data folder exists
                Directory.CreateDirectory(AppDataFolder);

                // Load license từ file
                LoadLicense();

                // Validate license
                if (!string.IsNullOrEmpty(LicenseKey))
                {
                    IsLicensed = Services.LicenseKey.ValidateLicenseKey(
                        LicenseKey,
                        HardwareId,
                        out string companyName,
                        out DateTime? expiryDate
                    );

                    if (IsLicensed)
                    {
                        CompanyName = companyName;
                        ExpiryDate = expiryDate;
                    }
                }
                else
                {
                    IsLicensed = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"License initialization error: {ex.Message}");
                IsLicensed = false;
            }
        }

        /// <summary>
        /// Load license từ file
        /// </summary>
        private void LoadLicense()
        {
            try
            {
                if (!File.Exists(LicenseFilePath))
                {
                    LicenseKey = null;
                    return;
                }

                string encryptedData = File.ReadAllText(LicenseFilePath);
                string decryptedData = DecryptString(encryptedData);

                // Parse: LicenseKey|CompanyName
                var parts = decryptedData.Split('|');
                if (parts.Length >= 1)
                {
                    LicenseKey = parts[0];
                }
                if (parts.Length >= 2)
                {
                    CompanyName = parts[1];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading license: {ex.Message}");
                LicenseKey = null;
            }
        }

        /// <summary>
        /// Lưu license vào file
        /// </summary>
        public bool SaveLicense(string licenseKey, string companyName = "")
        {
            try
            {
                Console.WriteLine($"[SAVE LICENSE] Starting save process...");
                Console.WriteLine($"[SAVE LICENSE] License key: {licenseKey}");
                Console.WriteLine($"[SAVE LICENSE] Hardware ID: {HardwareId}");

                // Validate trước khi lưu
                bool isValid = Services.LicenseKey.ValidateLicenseKey(
                    licenseKey,
                    HardwareId,
                    out string validatedCompany,
                    out DateTime? expiryDate
                );

                Console.WriteLine($"[SAVE LICENSE] Validation result: {isValid}");

                if (!isValid)
                {
                    Console.WriteLine($"[SAVE LICENSE] Validation failed - returning false");
                    return false;
                }

                // Lưu vào file encrypted
                string dataToEncrypt = $"{licenseKey}|{companyName}";
                Console.WriteLine($"[SAVE LICENSE] Data to encrypt: {dataToEncrypt}");

                string encryptedData = EncryptString(dataToEncrypt);
                Console.WriteLine($"[SAVE LICENSE] Encrypted data length: {encryptedData.Length}");

                if (string.IsNullOrEmpty(encryptedData))
                {
                    Console.WriteLine($"[SAVE LICENSE] ERROR: Encrypted data is empty!");
                    return false;
                }

                Directory.CreateDirectory(AppDataFolder);
                Console.WriteLine($"[SAVE LICENSE] Writing to file: {LicenseFilePath}");
                File.WriteAllText(LicenseFilePath, encryptedData);
                Console.WriteLine($"[SAVE LICENSE] File written successfully");

                // Update properties
                LicenseKey = licenseKey;
                CompanyName = companyName;
                ExpiryDate = expiryDate;
                IsLicensed = true;

                Console.WriteLine($"[SAVE LICENSE] Save completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SAVE LICENSE] ERROR: {ex.Message}");
                Console.WriteLine($"[SAVE LICENSE] Stack trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"Error saving license: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Encrypt string bằng AES
        /// </summary>
        private string EncryptString(string plainText)
        {
            try
            {
                Console.WriteLine($"[ENCRYPT] Starting encryption...");
                Console.WriteLine($"[ENCRYPT] Plain text: {plainText}");
                Console.WriteLine($"[ENCRYPT] Encryption key length: {EncryptionKey.Length}");

                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.GenerateIV();
                    Console.WriteLine($"[ENCRYPT] AES IV length: {aes.IV.Length}");

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        // Prepend IV
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);
                        Console.WriteLine($"[ENCRYPT] Wrote IV to stream");

                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                            swEncrypt.Flush();
                            csEncrypt.FlushFinalBlock();
                            Console.WriteLine($"[ENCRYPT] Wrote plain text and flushed");
                        }

                        byte[] encrypted = msEncrypt.ToArray();
                        Console.WriteLine($"[ENCRYPT] Encrypted bytes length: {encrypted.Length}");
                        string result = Convert.ToBase64String(encrypted);
                        Console.WriteLine($"[ENCRYPT] Base64 result length: {result.Length}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ENCRYPT] ERROR: {ex.Message}");
                Console.WriteLine($"[ENCRYPT] Stack trace: {ex.StackTrace}");
                return "";
            }
        }

        /// <summary>
        /// Decrypt string bằng AES
        /// </summary>
        private string DecryptString(string cipherText)
        {
            try
            {
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;

                    // Extract IV
                    byte[] iv = new byte[aes.IV.Length];
                    Array.Copy(buffer, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Reset license (xóa license file)
        /// </summary>
        public void ResetLicense()
        {
            try
            {
                if (File.Exists(LicenseFilePath))
                {
                    File.Delete(LicenseFilePath);
                }

                LicenseKey = null;
                CompanyName = null;
                ExpiryDate = null;
                IsLicensed = false;
                Initialize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting license: {ex.Message}");
            }
        }
    }
}
