// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.DataEntities
{
    [Table("LargeData", Schema = "dbo")]
    [DisplayName("Large Data")]
    public class LargeData : IEntity
    {
        [Key]
        public int LargeDataId { get; private set; }

        public long Size { get; set; }

        [ParentEntity(typeof(Hash))]
        public int HashId { get; set; }

        public byte? SyncPriority { get; set; }
    }
}