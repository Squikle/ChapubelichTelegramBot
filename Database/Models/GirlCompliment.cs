using ChapubelichBot.Types.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapubelichBot.Database.Models
{
    [Table("GirlCompliments")]
    class GirlCompliment : Compliment
    {
        public GirlCompliment(string compliment) : base(compliment)
        { }
        public GirlCompliment()
        { }
    }
}
