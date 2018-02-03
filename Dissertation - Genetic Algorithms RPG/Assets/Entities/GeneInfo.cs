using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Entities
{
    class GeneInfo
    {
        public object Sequence { get; set; }
        public bool IsDominant { get; set; }
        public float DominanceWeight { get; set; }
    }
}
