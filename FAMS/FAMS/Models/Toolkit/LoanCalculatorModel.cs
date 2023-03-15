using FAMS.ViewModels.Toolkit;

namespace FAMS.Models.Toolkit
{
    class LoanCalculatorModel
    {
        public LoanCalculatorModel()
        {
        }

        /// <summary>
        /// (当前仅按无息贷款来计算)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public LoanCalculatorViewModel Calc(LoanCalculatorViewModel data)
        {
            float totalPayment = 0;
            float downPayment = 0;
            int terms = 0;
            float fees = 0;
            //float loanRate = 0; // (当前仅按无息贷款来计算)
            float investRate = 0;

            if (!float.TryParse(data.TotalPayment, out totalPayment) || totalPayment < 0)
            {
                return data;
            }

            if (totalPayment == 0)
            {
                data.ActualDownPayment = data.TotalPayment;
                data.TermlyRepay = "0";
                data.TotalRevenue = "0";
                data.NetRevenue = "0";
                data.PayoffInterestRate = "0";
                return data;
            }

            if (!float.TryParse(data.DownPayment, out downPayment) || downPayment < 0 || downPayment > 100)
            {
                return data;
            }

            if (downPayment == 100)
            {
                data.ActualDownPayment = data.TotalPayment;
                data.TermlyRepay = "0";
                data.TotalRevenue = "0";
                data.NetRevenue = "0";
                data.PayoffInterestRate = "0";
                return data;
            }

            data.ActualDownPayment = (totalPayment * downPayment * 0.01f).ToString();

            if (!int.TryParse(data.Terms, out terms) || terms < 0)
            {
                return data;
            }

            if (terms == 0)
            {
                data.ActualDownPayment = data.TotalPayment;
                data.TermlyRepay = "0";
                data.TotalRevenue = "0";
                data.NetRevenue = "0";
                data.PayoffInterestRate = "0";
                return data;
            }

            if (!float.TryParse(data.Fees, out fees) || fees < 0)
            {
                return data;
            }

            if (fees == 0)
            {
                data.PayoffInterestRate = "0";
            }

            //if (!float.TryParse(data.LoanInterestRate, out loanRate) || loanRate < 0)
            //{
            //    return data;
            //}
            data.LoanInterestRate = "0"; // (当前仅按无息贷款来计算)

            if (!float.TryParse(data.InvestInterestRate, out investRate) || investRate < 0)
            {
                return data;
            }

            float capital = totalPayment * (100 - downPayment) * 0.01f;
            investRate = investRate * 0.01f;

            float currentCapital = capital;
            float termlyRepay = capital / terms;
            float totalRevenue = 0;

            data.TermlyRepay = termlyRepay.ToString();

            for (int i = 0; i < terms; i++)
            {
                totalRevenue += currentCapital * investRate * 0.0833f; // 0.0833=1/12, 12 months
                currentCapital -= termlyRepay;
            }
            data.TotalRevenue = totalRevenue.ToString();
            data.NetRevenue = (totalRevenue - fees).ToString();

            if (fees > 0)
            {
                float init = 0.01f; // initial interest rate
                float incr = 0.01f; // interest rate increment
                investRate = (init - incr) * 0.01f;
                currentCapital = capital;
                totalRevenue = 0;

                while (totalRevenue < fees)
                {
                    investRate += incr * 0.01f;
                    currentCapital = capital;
                    totalRevenue = 0;
                    for (int i = 0; i < terms; i++)
                    {
                        totalRevenue += currentCapital * investRate * 0.0833f;
                        currentCapital -= termlyRepay;
                    }
                }
                data.PayoffInterestRate = (investRate * 100).ToString();
            }

            return data;
        }
    }
}
