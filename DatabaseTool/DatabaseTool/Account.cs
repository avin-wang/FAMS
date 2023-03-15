
namespace FAMS.Data.Accounts
{
    public struct Account
    {
        public string AccountName;      // Account name.
        public int AccountType;         // Account type(0-general, 1-financial, 2-work).
        public string URL;              // Website address.
        public string UserName;         // User name, i.e., login name.
        public string Password;         // Password to login.
        public string DisplayName;      // Display name, i.e., nick name.
        public string Email;            // Registration e-mail.
        public string Telephone;        // Registration telephone.
        public string PaymentCode;      // Payment code.
        public string AppendDate;       // Append date.
        public int AttachmentFlag;      // Attachment flag(0-no, 1-yes).
        public string AttachedFileName; // Attached file name with extension(without directory path).
        public string Remark;           // Remark.
    }
}
