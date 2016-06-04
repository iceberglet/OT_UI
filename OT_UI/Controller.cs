﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using System.ComponentModel;
using System.Windows.Forms.DataVisualization.Charting;

namespace OT_UI
{
    static class Controller
    {
        public enum sl_Kendall { Min, Avg, Wtd };
        public static sl_Kendall selected_kendall = sl_Kendall.Min;
        public enum sl_Filter { None, Domi, Pncr };
        public static sl_Filter selected_filter = sl_Filter.None;
        public enum sl_Strategy { Det1, Det2, Tour, Kernel };
        public static sl_Strategy selected_strategy = sl_Strategy.Det1;
        
        public static int speed; //0-paused, 1-sample, 2-iteration, 3-grand_average
        
        private static Form1 f;
        //private static Thread iterator;
        private static Chart graph_rank;
        private static Chart graph_avg;
        private static OTVS otvs;

        //Called when initialize is clicked

        public static void Initialize(Form1 form, Chart graph_r, Chart graph_a)
        {
            f = form;

            //set up rank graph
            graph_rank = graph_r;
            graph_avg = graph_a;
            var solutions = Utility.Xu2014(g : 3);
            otvs = new OTVS(solutions);
            updateRankPoints();
            /*
            var sol1 = Utility.Xu2014(1);
            var sol2 = Utility.Xu2014(2);
            var sol3 = Utility.Xu2014(3);
            var res = "";
            res += Utility.hasMinKendallAtOptimum(solutions) + "\n";
            res += Utility.hasMinKendallAtOptimum(sol1) + "\n";
            res += Utility.hasMinKendallAtOptimum(sol2) + "\n";
            res += Utility.hasMinKendallAtOptimum(sol3) + "\n";
            System.Windows.Forms.MessageBox.Show(res);*/
            /*
            iterator = new Thread(Iterate);
            iterator.Start();*/

            /*
            while (runningRound)
            {
                //Call iterate every 10ms if running == false
                Timer timer = new Timer();
                // Tell the timer what to do when it elapses
                timer.Elapsed += new ElapsedEventHandler(Iterate);
                // Set it to go off every five seconds
                timer.Interval = 50;
                // And start it
                timer.Enabled = true;
            }*/
        }

        /*
        public static void Stop()
        {
            runningRound = false;
        }*/

        //Called repeatedly after Initialize
        public static void Iterate()
        {
            /*
            if (speed == 0)
            {
                return;
            }*/

            otvs.Iterate();
            //1. Already at end of one Round, add to average, refresh and continue.

            //2. Do one iteration

            //3. Now at the end of one Round? (3) Refresh and continue (2,1) Pause

            //4. (2) Continue (1) Pause
            updateRankPoints();
            
        }

        private static void updateRankPoints()
        {
            Series ranks = graph_rank.Series.Where(x => x.Name == "Ranks").ToList().First();
            ranks.Points.Clear();
            Series sampled = graph_rank.Series.Where(x => x.Name == "Sampled").ToList().First();
            sampled.Points.Clear();
            Series filtered = graph_rank.Series.Where(x => x.Name == "Filter").ToList().First();
            filtered.Points.Clear();
            foreach (var i in Enumerable.Range(0, otvs.Solutions.Count))
            {
                DataPoint dp = new DataPoint(otvs.Solutions[i].LFRank, otvs.Solutions[i].HFRank);
                if (otvs.partial.Contains(i))
                {
                    dp.MarkerSize = 15;
                    dp.MarkerColor = System.Drawing.Color.Violet;
                    dp.MarkerStyle = MarkerStyle.Diamond;
                    filtered.Points.Add(dp);
                }
                else if(otvs.SampledIndices.Contains(i))
                    sampled.Points.Add(dp);
                else
                    ranks.Points.Add(dp);
            }

            
            //Tau value lines
            if (otvs.LeftTaus.Count > 0 && otvs.RightTaus.Count > 0)
            {
                //System.Windows.Forms.MessageBox.Show("left: " + otvs.LeftTaus.Count  + "right: " + otvs.RightTaus.Count);
                Series left = graph_rank.Series.Where(x => x.Name == "LeftTauValue").ToList().First();
                left.Points.Clear();
                Series right = graph_rank.Series.Where(x => x.Name == "RightTauValue").ToList().First();
                right.Points.Clear();
                Series proba = graph_rank.Series.Where(x => x.Name == "ProbaValue").ToList().First();
                proba.Points.Clear();

                String newTau = "" + otvs.LeftTaus.Count + ',' + otvs.RightTaus.Count + ',';
                foreach (var i in Enumerable.Range(0, otvs.Solutions.Count))
                {
                    if (!otvs.SampledIndices.Contains(i))
                    {
                        newTau += Math.Round(otvs.LeftTaus[i], 3).ToString() + ',';
                        DataPoint l = new DataPoint(otvs.Solutions[i].LFRank, otvs.LeftTaus[i]);  //Left Tau -1: increasing, 1: decreasing
                        DataPoint r = new DataPoint(otvs.Solutions[i].LFRank, otvs.RightTaus[i]); //Right Tau-1: decreasing, 1: increasing
                        DataPoint p = new DataPoint(otvs.Solutions[i].LFRank, otvs[i]);
                        left.Points.Add(l);
                        right.Points.Add(r);
                        //proba.Points.Add(p);
                    }
                }

                using (var sw = new StreamWriter("LeftTauValue.csv", true)) sw.WriteLine(newTau);
            }

        }
        
    }
}
