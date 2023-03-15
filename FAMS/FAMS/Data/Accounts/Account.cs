namespace FAMS.Data.Accounts
{
    public struct Account
    {
        public string AccountName;         // Account name
        public int AccountType;            // Account type (0-普通账号, 1-财务账号, 2-工作账号, 3-政务账号)
        public string URL;                 // Website address
        public string UserName;            // User name, i.e., login name
        public string Password;            // Password to login
        public string DisplayName;         // Display name, i.e., nick name
        public string Email;               // Registration e-mail
        public string Telephone;           // Registration telephone
        public string PaymentCode;         // Payment code
        public string AppendDate;          // Append date
        public string LastRevised;         // Last revise date
        public string FormerAccountNames;  // Former account names (separated by '|')
        public int AttachmentFlag;         // Attachment flag (0-无, 1-有)
        public string AttachedFileNames;   // Attached file name with extension (without directory path)(separated by '|')
        public string Remarks;             // Remarks
        public string KeyWords;            // Key words (separated by '|')
    }
}
