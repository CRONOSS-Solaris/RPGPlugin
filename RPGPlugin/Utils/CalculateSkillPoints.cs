using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGPlugin.Utils
{
    public class SkillPointCalculator
    {
        public int CalculateSkillPoints(int level, List<KeyValuePair<int, int>> SkillPoints)
        {
            int skillPoints = 0;

            foreach (var skillPoint in SkillPoints)
            {
                if (level >= skillPoint.Key)
                {
                    skillPoints = skillPoint.Value;
                }
            }

            return skillPoints;
        }
    }
}
