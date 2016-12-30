// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.DataEntities
{
    [Table("Hash", Schema = "dbo")]
    [DisplayName("Hash")]
    public class Hash : IEntity
    {
        [Key]
        public int HashId { get; private set; }

        [Required]
        [MaxLength(16)]
        public byte[] Code { get; set; }
    }
}