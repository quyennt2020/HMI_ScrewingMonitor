using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HMI_ScrewingMonitor.Services
{
    /// <summary>
    /// Quản lý license: Load, Save, Validate, Trial period, Anti-tamper
    /// </summary>
    public class LicenseManager
    {
        private const int TRIAL_DAYS = 30;
        private const string REGISTRY_KEY_PATH = @"Software\HMI_ScrewingMonitor";
        private const string REGISTRY_INSTALL_DATE = "InstallDate";
        private const string REGISTRY_LAST_RUN = "LastRun";
        private const string LICENSE_FILE_NAME = "license.dat";

        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HMI_ScrewingMonitor"
        );

        private static readonly string LicenseFilePath = Path.Combine(AppDataFolder, LICENSE_FILE_NAME);

        // AES encryption key - Thay đổi thành giá trị riêng của bạn
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("HMI_SCREWING_2025_KEY_32BYTES!");

        public string LicenseKey { get; private set; }
        public string CompanyName { get; private set; }
        public DateTime? ExpiryDate { get; private set; }
        public DateTime FirstRunDate { get; private set; }
        public DateTime LastRunDate { get; private set; }
        public bool IsLicensed { get; private set; }
        public bool IsTrialExpired { get; private set; }
        public int DaysRemaining { get; private set; }
        public bool TamperDetected { get; private set; }
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

                // Load FirstRunDate và LastRunDate từ Registry
                LoadDatesFromRegistry();

                // Kiểm tra tamper (ngày bị chỉnh)
                CheckTamper();

                if (TamperDetected)
                {
                    IsLicensed = false;
                    IsTrialExpired = true;
                    DaysRemaining = 0;
                    return;
                }

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
                        IsTrialExpired = false;
                        DaysRemaining = -1; // Unlimited
                        SaveLastRunDate();
                        return;
                    }
                }

                // Không có license hợp lệ → Kiểm tra trial
                var daysSinceFirstRun = (DateTime.Today - FirstRunDate).Days;
                DaysRemaining = TRIAL_DAYS - daysSinceFirstRun;
                IsTrialExpired = DaysRemaining <= 0;

                // Lưu LastRunDate
                SaveLastRunDate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"License initialization error: {ex.Message}");
                // Fallback: Assume trial mode
                IsLicensed = false;
                IsTrialExpired = false;
                DaysRemaining = TRIAL_DAYS;
            }
        }

        /// <summary>
        /// Load FirstRunDate và LastRunDate từ Registry
        /// </summary>
        private void LoadDatesFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    // Load FirstRunDate
                    object installDateValue = key.GetValue(REGISTRY_INSTALL_DATE);
                    if (installDateValue != null)
                    {
                        // Decrypt
                        string encryptedDate = installDateValue.ToString();
                        string decryptedDate = DecryptString(encryptedDate);
                        FirstRunDate = DateTime.Parse(decryptedDate);
                    }
                    else
                    {
                        // First time run
                        FirstRunDate = DateTime.Today;
                        string encryptedDate = EncryptString(FirstRunDate.ToString("yyyy-MM-dd"));
                        key.SetValue(REGISTRY_INSTALL_DATE, encryptedDate);
                    }

                    // Load LastRunDate
                    object lastRunValue = key.GetValue(REGISTRY_LAST_RUN);
                    if (lastRunValue != null)
                    {
                        string encryptedDate = lastRunValue.ToString();
                        string decryptedDate = DecryptString(encryptedDate);
                        LastRunDate = DateTime.Parse(decryptedDate);
                    }
                    else
                    {
                        LastRunDate = DateTime.Today;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dates from registry: {ex.Message}");
                FirstRunDate = DateTime.Today;
                LastRunDate = DateTime.Today;
            }
        }

        /// <summary>
        /// Lưu LastRunDate vào Registry
        /// </summary>
        private void SaveLastRunDate()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    string encryptedDate = EncryptString(DateTime.Today.ToString("yyyy-MM-dd"));
                    key.SetValue(REGISTRY_LAST_RUN, encryptedDate);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving last run date: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra tamper (ngày bị chỉnh lùi)
        /// </summary>
        private void CheckTamper()
        {
            try
            {
                // Kiểm tra: CurrentDate < LastRunDate
                if (DateTime.Today < LastRunDate)
                {
                    TamperDetected = true;
                    System.Diagnostics.Debug.WriteLine("TAMPER DETECTED: System date rolled back!");
                }
                else
                {
                    TamperDetected = false;
                }
            }
            catch
            {
                TamperDetected = false;
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
                // Validate trước khi lưu
                bool isValid = Services.LicenseKey.ValidateLicenseKey(
                    licenseKey,
                    HardwareId,
                    out string validatedCompany,
                    out DateTime? expiryDate
                );

                if (!isValid)
                    return false;

                // Lưu vào file encrypted
                string dataToEncrypt = $"{licenseKey}|{companyName}";
                string encryptedData = EncryptString(dataToEncrypt);

                Directory.CreateDirectory(AppDataFolder);
                File.WriteAllText(LicenseFilePath, encryptedData);

                // Update properties
                LicenseKey = licenseKey;
                CompanyName = companyName;
                ExpiryDate = expiryDate;
                IsLicensed = true;
                IsTrialExpired = false;

                return true;
            }
            catch (Exception ex)
            {
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
                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.GenerateIV();

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        // Prepend IV
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        byte[] encrypted = msEncrypt.ToArray();
                        return Convert.ToBase64String(encrypted);
                    }
                }
            }
            catch
            {
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
