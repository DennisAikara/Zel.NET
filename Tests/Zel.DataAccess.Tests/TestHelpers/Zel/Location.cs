// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;
using Zel.DataAccess.Entity.Auditing;

namespace Zel.DataAccess.Tests.TestHelpers.Zel
{
    [UniqueConstraint("A location with the same name already exists", "Name")]
    [Table("Location", Schema = "dbo")]
    public class Location : IEntity, IAuditCreatedByName, IAuditCreatedOn, IAuditModifiedByName, IAuditModifiedOn
    {
        [Key]
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Location name is required", AllowEmptyStrings = false)]
        public string Name { get; set; }

        #region IAuditCreatedByName Members

        public string CreatedBy { get; set; }

        #endregion

        #region IAuditCreatedOn Members

        public DateTime CreatedOn { get; set; }

        #endregion

        #region IAuditModifiedByName Members

        public string ModifiedBy { get; set; }

        #endregion

        #region IAuditModifiedOn Members

        public DateTime ModifiedOn { get; set; }

        #endregion
    }
}