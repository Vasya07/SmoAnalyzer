using System;

namespace SmoAnalyzer
{
    public static class SmoCalculator
    {
        // 1. Одноканальная СМО с отказами
        public static SmoResult Rejection(double lambda, double mu)
        {
            double rho = lambda / mu;
            double p0 = 1.0 / (1.0 + rho);
            double potk = rho / (1.0 + rho);
            double q = 1 - potk;
            double a = lambda * q;

            var r = new SmoResult { Title = "Одноканальная СМО с отказами (M/M/1/0)" };
            r.Add("λ (интенсивность потока)", lambda, "F4", "ед./ч");
            r.Add("μ (интенсивность обслуживания)", mu, "F4", "ед./ч");
            r.Add("ρ = λ/μ", rho, "F4");
            r.Add("P0 (вероятность простоя)", p0, "F6");
            r.Add("Pотк (вероятность отказа)", potk, "F6");
            r.Add("Q (относительная пропускная способность)", q, "F6");
            r.Add("A (абсолютная пропускная способность)", a, "F4", "ед./ч");
            r.Add("Lсист (среднее число заявок в системе)", potk, "F4");
            r.Add("Wсист (среднее время в системе)", 1.0 / mu, "F4", "ч");
            return r;
        }

        // 2. Многоканальная СМО с неограниченной очередью
        public static SmoResult InfiniteQueue(double lambda, double mu, int c)
        {
            if (c <= 0) throw new ArgumentException("Число каналов c должно быть > 0.");
            double rho = lambda / mu;
            if (rho / c >= 1.0)
                throw new InvalidOperationException("Система перегружена: λ/(c·μ) ≥ 1, стационарного режима нет ");

            double sum = 0, term = 1;
            for (int k = 0; k <= c; k++)
            {
                if (k > 0) term *= rho / k;
                sum += term;
            }
            int n = 2;
            double p0 = 1.0 / (sum + term * rho / (c - rho));
            double pAllBusy = term * rho / (c - rho) * p0;
            double lq = pAllBusy * rho / (c - rho);
            double lo = lq + rho;
            double wq = lq / lambda;
            double wo = wq + 1.0 / mu;
            double pAtMost2 = 0;
            double pAtMostN = 0;
            double termK = 1;
            for (int k = 0; k <= c + n; k++)
            {
                if (k <= c)
                {
                    if (k > 0) termK *= rho / k;
                    pAtMostN += termK * p0;
                }
                else
                {
                    double Pc = termK * p0;
                    pAtMostN += Pc * Math.Pow(rho / c, k - c);
                }
            }
            var r = new SmoResult { Title = $"Многоканальная СМО с неограниченной очередью (M/M/{c}/∞)" };
            r.Add("λ", lambda, "F4", "ед./ч");
            r.Add("μ", mu, "F4", "ед./ч");
            r.Add("c (число каналов)", c);
            r.Add("ρ = λ/μ", rho, "F4");
            r.Add("ρ/c (загрузка канала)", rho / c, "F4");
            r.Add("P0 (вероятность простоя)", p0, "F6");
            r.Add("P(все каналы заняты)", pAllBusy, "F6");
            r.Add("Lq (среднее число в очереди)", lq, "F4");
            r.Add("Lсист (среднее число в системе)", lo, "F4");
            r.Add("Wq (среднее время ожидания)", wq, "F4", "ч");
            r.Add("Wсист (среднее время в системе)", wo, "F4", "ч");
            r.Add("P(ожидают не более 2 заявок)", pAtMostN, "F6");
            return r;
        }

        // 3. Одноканальная СМО с ограниченной очередью
        public static SmoResult LimitedQueue(double lambda, double mu, int m)
        {
            if (m < 0) throw new ArgumentException("Длина очереди m должна быть ≥ 0");
            double rho = lambda / mu;
            double sum = 0, term = 1;

            for (int k = 0; k <= m + 1; k++)
            {
                if (k > 0) term *= rho;
                sum += term;
            }
            double p0 = 1.0 / sum;
            double potk = Math.Pow(rho, m + 1) * p0;
            double lq, lo;

            if (Math.Abs(rho - 1.0) < 1e-9)
            {
                lq = m * (m + 1) / 2.0 / (m + 2);
                lo = lq + (1 - p0);
            }
            else
            {
                lq = p0 * rho * (1 - (m + 1) * Math.Pow(rho, m) + m * Math.Pow(rho, m + 1))
                     / Math.Pow(1 - rho, 2);
                lo = lq + (1 - p0);
            }
            double lambdaEff = lambda * (1 - potk);
            double q = 1 - potk;
            double wq = lambdaEff > 0 ? lq / lambdaEff : 0;
            double wo = lambdaEff > 0 ? lo / lambdaEff : 0;

            var r = new SmoResult { Title = $"Одноканальная СМО с ограниченной очередью (M/M/1/{m})" };
            r.Add("λ", lambda, "F4", "ед./ч");
            r.Add("μ", mu, "F4", "ед./ч");
            r.Add("m (макс. длина очереди)", m);
            r.Add("ρ = λ/μ", rho, "F4");
            r.Add("P0 (вероятность простоя)", p0, "F6");
            r.Add("Pотк (вероятность отказа)", potk, "F6");
            r.Add("Q (относительная пропускная способность)", q, "F6");
            r.Add("A (абсолютная пропускная способность)", lambdaEff, "F4", "ед./ч");
            r.Add("Lq (среднее число в очереди)", lq, "F4");
            r.Add("Lсист (среднее число в системе)", lo, "F4");
            r.Add("Wq (среднее время ожидания)", wq, "F4", "ч");
            r.Add("Wсист (среднее время в системе)", wo, "F4", "ч");
            return r;
        }
    }
}