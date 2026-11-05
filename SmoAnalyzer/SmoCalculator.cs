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
            double pAtMostN = 0;
            double termK = 1;
            for (int k = 0; k <= c; k++)
            {
                if (k > 0) termK *= rho / k;
                pAtMostN += termK * p0;
            }
            double Pc = termK * p0;
            double queueTail = 0;
            for (int j = 1; j <= n; j++)
                queueTail += Pc * Math.Pow(rho / c, j);
            pAtMostN += queueTail;
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
        // 4. Многоканальная СМО с отказами
        public static SmoResult MultiRejection(double lambda, double mu, int c)
        {
            if (c <= 0) throw new ArgumentException("Число каналов c должно быть > 0.");
            double rho = lambda / mu;

            double sum = 0, term = 1;
            for (int k = 0; k <= c; k++)
            {
                if (k > 0) term *= rho / k;
                sum += term;
            }
            double p0 = 1.0 / sum;
            double pReject = term * p0;
            double q = 1 - pReject;
            double a = lambda * q;
            double lSys = a / mu;

            var r = new SmoResult { Title = $"Многоканальная СМО с отказами (M/M/{c}/0)" };
            r.Add("λ", lambda, "F4", "ед./ч");
            r.Add("μ", mu, "F4", "ед./ч");
            r.Add("c (число каналов)", c);
            r.Add("ρ = λ/μ", rho, "F4");
            r.Add("P0 (вероятность простоя)", p0, "F6");
            r.Add("Pотк (вероятность отказа, Эрланг B)", pReject, "F6");
            r.Add("Q (относительная пропускная способность)", q, "F6");
            r.Add("A (абсолютная пропускная способность)", a, "F4", "ед./ч");
            r.Add("Lсист (среднее число занятых каналов)", lSys, "F4");
            r.Add("Wсист (среднее время в системе)", 1.0 / mu, "F4", "ч");
            return r;
        }
        // 5. Многоканальная СМО с ограниченной очередью
        public static SmoResult MultiLimitedQueue(double lambda, double mu, int c, int m)
        {
            if (c <= 0) throw new ArgumentException("Число каналов c должно быть > 0.");
            if (m < 0) throw new ArgumentException("Длина очереди m должна быть ≥ 0");
            double rho = lambda / mu;
            double rc = rho / c;

            double p0sum = 0, term = 1;
            for (int k = 0; k <= c; k++)
            {
                if (k > 0) term *= rho / k;
                p0sum += term;
            }
            double pcTerm = term;
            double queueSum = 0, qTerm = 1;
            for (int j = 1; j <= m; j++)
            {
                qTerm *= rc;
                queueSum += qTerm;
            }
            double p0 = 1.0 / (p0sum + pcTerm * queueSum);

            double pReject = pcTerm * Math.Pow(rc, m) * p0;
            double q = 1 - pReject;
            double a = lambda * q;

            double lq;
            if (Math.Abs(rc - 1.0) < 1e-9)
                lq = pcTerm * p0 * m * (m + 1) / 2.0;
            else
                lq = pcTerm * p0 * rc *
                     (1 - (m + 1) * Math.Pow(rc, m) + m * Math.Pow(rc, m + 1))
                     / Math.Pow(1 - rc, 2);

            double lSys = lq + rho * q;
            double wq = a > 0 ? lq / a : 0;
            double wSys = a > 0 ? lSys / a : 0;

            var r = new SmoResult { Title = $"Многоканальная СМО с ограниченной очередью (M/M/{c}/{m})" };
            r.Add("λ", lambda, "F4", "ед./ч");
            r.Add("μ", mu, "F4", "ед./ч");
            r.Add("c (число каналов)", c);
            r.Add("m (макс. длина очереди)", m);
            r.Add("ρ = λ/μ", rho, "F4");
            r.Add("ρ/c (загрузка канала)", rc, "F4");
            r.Add("P0 (вероятность простоя)", p0, "F6");
            r.Add("Pотк (вероятность отказа)", pReject, "F6");
            r.Add("Q (относительная пропускная способность)", q, "F6");
            r.Add("A (абсолютная пропускная способность)", a, "F4", "ед./ч");
            r.Add("Lq (среднее число в очереди)", lq, "F4");
            r.Add("Lсист (среднее число в системе)", lSys, "F4");
            r.Add("Wq (среднее время ожидания)", wq, "F4", "ч");
            r.Add("Wсист (среднее время в системе)", wSys, "F4", "ч");
            return r;
        }
        // 6. Одноканальная СМО с неограниченной очередью
        public static SmoResult SingleInfiniteQueue(double lambda, double mu)
        {
            double rho = lambda / mu;
            if (rho >= 1.0)
                throw new InvalidOperationException("Система перегружена: λ/μ ≥ 1, стационарного режима нет");

            double p0 = 1 - rho;
            double lq = rho * rho / (1 - rho);
            double lSys = rho / (1 - rho);
            double wq = rho / (mu * (1 - rho));
            double wSys = 1.0 / (mu * (1 - rho));

            var r = new SmoResult { Title = "Одноканальная СМО с неограниченной очередью (M/M/1/∞)" };
            r.Add("λ", lambda, "F4", "ед./ч");
            r.Add("μ", mu, "F4", "ед./ч");
            r.Add("ρ = λ/μ", rho, "F4");
            r.Add("P0 (вероятность простоя)", p0, "F6");
            r.Add("Lq (среднее число в очереди)", lq, "F4");
            r.Add("Lсист (среднее число в системе)", lSys, "F4");
            r.Add("Wq (среднее время ожидания)", wq, "F4", "ч");
            r.Add("Wсист (среднее время в системе)", wSys, "F4", "ч");
            r.Add("P(в системе ≤ 5 заявок)", 1 - Math.Pow(rho, 6), "F6");
            return r;
        }
    }
}