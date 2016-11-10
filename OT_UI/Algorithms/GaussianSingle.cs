﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GaussianRegression.Core;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

namespace OT_UI
{
    class GaussianSingle : Algorithm
    {
        private GP myGP;

        public override void initialize(List<Solution> solutions)
        {
            base.initialize(solutions);

            var num = 50;

            for (int i = 0; i < num; i++)
            {
                var idx = solutions.Count / num * i + rand.Next(0, solutions.Count / num);
                sample(solutions.ElementAt(idx));
            }

            var initial = solutionsSampled.Select(s => new XYPair(GPUtility.V(s.LFRank), s.HFValue)).ToList();
            var list_x = solutions.Select(s => new LabeledVector(s.LFRank, GPUtility.V(s.LFRank))).ToList();
            myGP = new GP(initial, list_x, CovFunction.SquaredExponential(new LengthScale(50), new SigmaF(0.6)) + CovFunction.GaussianNoise(new SigmaJ(0.05)),
                    heteroscedastic: true,
                    sigma_f: 1
                    );
        }


        private int iter = 0;
        public override bool iterate()
        {
            iter++;
            //For logging purposes
            var X = new double[solutions.Count];
            var Y = new double[solutions.Count];
            var upper = new double[solutions.Count];
            var lower = new double[solutions.Count];
            var probas = new double[solutions.Count];

            
            var res = myGP.predict();
            foreach(var kv in res)
            {
                Solution s = solutions.Find(x => x.LFRank == kv.Key.idx);   //lf compare
                double mean = kv.Value.mu;
                double sd = kv.Value.sd;
                s.proba = Normal.CDF(mean, sd, optimum.HFValue);
                //if (sd > 0.01)
                //    Utility.popup(sd.ToString());
                s.a = mean + 1.96 * sd;
                s.b = mean - 1.96 * sd;


                X[kv.Key.idx] = kv.Key.idx;
                Y[kv.Key.idx] = s.HFValue;
                upper[kv.Key.idx] = s.a;
                lower[kv.Key.idx] = s.b;
                probas[kv.Key.idx] = solutionsSampled.Contains(s) ? 0 : s.proba;
            }

            var next = solutions[Utility.SampleAmong(probas)];
            sample(next);
            myGP.addPoint(new XYPair(GPUtility.V(next.LFRank), next.HFValue));

            //Logging
            //Utility.exportExcel<double>("Gaussian" + iter + ".csv", X.ToList(), Y.ToList(), lower.ToList(), upper.ToList(), probas.ToList());

            return true;
        }

        public override void resetIteration()
        {
            initialize(solutions);
        }
    }
}