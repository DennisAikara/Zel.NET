// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.DataEntities
{
    [Table("DataType", Schema = "dbo")]
    [UniqueConstraint("UIX_DataType_Type", "Type")]
    [DisplayName("Data Type")]
    public class DataType : IEntity
    {
        [Key]
        public int DataTypeId { get; private set; }

        [Required]
        [MaxLength(450)]
        public string Type { get; set; }
    }
}