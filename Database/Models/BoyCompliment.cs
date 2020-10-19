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
    [Table("BoyCompliments")]
    class BoyCompliment : Compliment
    {
        public BoyCompliment(string compliment) : base(compliment)
        { }
        public BoyCompliment()
        { }
    }
}
