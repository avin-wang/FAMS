using System.ComponentModel;

namespace FAMS.ViewModels.Toolkit
{
    public class LoanCalculatorViewModel : INotifyPropertyChanged
    {
        private string m_strTotalPayment = "0";       // total payment (absolute)
        private string m_strDownPayment = "0";       // down payment (relative, %)
        private string m_strTerms = "24";             // loan term
        private string m_strFees = "0";               // loan handling fees
        private string m_strLoanInterestRate = "0";   // loan average annual interest rate
        private string m_strInvestInterestRate = "0"; // investment average annual interest rate
        private string m_strActualDownPayment = "0";  // actual down payment (absolute)
        private string m_strTermlyRepay = "0";        // termly repayment
        private string m_strTotalRevenue = "0";       // total investment revenue
        private string m_strNetRevenue = "0";         // net investment revenue
        private string m_strPayoffInterestRate = "0"; // least interest rate to pay off the investment

        public string TotalPayment
        {
            get { return m_strTotalPayment; }
            set
            {
                m_strTotalPayment = value;
                NotifyPropertyChange("TotalPayment");
            }
        }

        public string DownPayment
        {
            get { return m_strDownPayment; }
            set
            {
                m_strDownPayment = value;
                NotifyPropertyChange("DownPayment");
            }
        }

        public string Terms
        {
            get { return m_strTerms; }
            set
            {
                m_strTerms = value;
                NotifyPropertyChange("Terms");
            }
        }

        public string Fees
        {
            get { return m_strFees; }
            set
            {
                m_strFees = value;
                NotifyPropertyChange("Fees");
            }
        }

        public string LoanInterestRate
        {
            get { return m_strLoanInterestRate; }
            set
            {
                m_strLoanInterestRate = value;
                NotifyPropertyChange("LoanInterestRate");
            }
        }

        public string InvestInterestRate
        {
            get { return m_strInvestInterestRate; }
            set
            {
                m_strInvestInterestRate = value;
                NotifyPropertyChange("InvestInterestRate");
            }
        }

        public string ActualDownPayment
        {
            get { return m_strActualDownPayment; }
            set
            {
                m_strActualDownPayment = value;
                NotifyPropertyChange("ActualDownPayment");
            }
        }

        public string TermlyRepay
        {
            get { return m_strTermlyRepay; }
            set
            {
                m_strTermlyRepay = value;
                NotifyPropertyChange("TermlyRepay");
            }
        }

        public string TotalRevenue
        {
            get { return m_strTotalRevenue; }
            set
            {
                m_strTotalRevenue = value;
                NotifyPropertyChange("TotalRevenue");
            }
        }

        public string NetRevenue
        {
            get { return m_strNetRevenue; }
            set
            {
                m_strNetRevenue = value;
                NotifyPropertyChange("NetRevenue");
            }
        }

        public string PayoffInterestRate
        {
            get { return m_strPayoffInterestRate; }
            set
            {
                m_strPayoffInterestRate = value;
                NotifyPropertyChange("PayoffInterestRate");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
