// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.DataEntities
{
    [Table("Data", Schema = "dbo")]
    [UniqueConstraint("UIX_DataTypeId_Name", "DataTypeId", "Name")]
    [DisplayName("Data")]
    public class Data : IEntity
    {
        [Key]
        public long DataId { get; private set; }

        [Required]
        [ParentEntity(typeof(DataType))]
        public int DataTypeId { get; set; }

        [Required]
        [MaxLength(448)]
        public string Name { get; set; }

        [MaxLength(450)]
        public string Value { get; set; }

        [ParentEntity(typeof(LargeData))]
        public int? LargeDataId { get; set; }
    }
}