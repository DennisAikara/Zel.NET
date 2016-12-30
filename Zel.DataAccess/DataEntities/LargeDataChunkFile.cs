// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.DataEntities
{
    [Table("LargeDataChunkFile", Schema = "dbo")]
    [DisplayName("Large Data Chunk File")]
    [UniqueConstraint("UIX_LargeDataChunkFile_HashId", "HashId")]
    [UniqueConstraint("UIX_LargeDataChunkFile_SyncIdentifier", "SyncIdentifier")]
    public class LargeDataChunkFile : IEntity
    {
        [Key]
        public int LargeDataChunkFileId { get; private set; }

        [ParentEntity(typeof(Hash))]
        public int HashId { get; set; }

        public byte? SyncAccount { get; set; }

        [MaxLength(28)]
        public string SyncIdentifier { get; set; }

        public short? Folder { get; set; }
    }
}