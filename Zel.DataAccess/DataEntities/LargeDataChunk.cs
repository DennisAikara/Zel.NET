// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.DataEntities
{
    [Table("LargeDataChunk", Schema = "dbo")]
    [DisplayName("Large Data Chunk")]
    [UniqueConstraint("UIX_LargeDataChunk_LargeDataId-Position", "LargeDataId", "Position")]
    [UniqueConstraint("UIX_LargeDataChunk_LargeDataId-LargeDataChunkFileId", "LargeDataId", "LargeDataChunkFileId")]
    public class LargeDataChunk : IEntity
    {
        [Key]
        public int LargeDataChunkId { get; private set; }

        public short Position { get; set; }

        [ParentEntity(typeof(LargeData))]
        public int LargeDataId { get; set; }

        [ParentEntity(typeof(LargeDataChunkFile))]
        public int LargeDataChunkFileId { get; set; }
    }
}