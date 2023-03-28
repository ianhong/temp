using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlwaysOn.Shared.Models.DataTransfer
{
    public class InventoryItemDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string ProductId { get; set; }

        [Required]
        public int QuanityPlaced { get; set; }
    }

    public class ConnectionDto
    {
        public string ConnectionUrl { get; set; }
    }
}
