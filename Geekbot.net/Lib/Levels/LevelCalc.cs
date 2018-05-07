﻿using System;
using System.Collections.Generic;

namespace Geekbot.net.Lib.Levels
{
    public class LevelCalc : ILevelCalc
    {
        private int[] _levels;

        public LevelCalc()
        {
            var levels = new List<int>();
            double total = 0;
            for (var i = 1; i < 120; i++)
            {
                total += Math.Floor(i + 300 * Math.Pow(2, i / 7.0));
                levels.Add((int) Math.Floor(total / 16));
            }
            _levels = levels.ToArray();
        }

        public int GetLevel(int messages)
        {
            var returnVal = 1;
            foreach (var level in _levels)
            {
                if (level > messages) break;
                returnVal++;
            }
            return returnVal;
        }
    }
}