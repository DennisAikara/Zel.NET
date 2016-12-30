// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;
using Zel.DataAccess.Entity.Auditing;

namespace Zel.DataAccess.Tests.TestHelpers.Zel
{
    [UniqueConstraint("An employer with the same name already exist.", "Name")]
    [Table("Employer", Schema = "dbo")]
    public class Employer : IEntity, IAuditCreatedBy, IAuditCreatedOn, IAuditModifiedBy, IAuditModifiedOn
    {
        [Key]
        public int EmployerId { get; set; }

        [Required(ErrorMessage = "Employer name is required", AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Employer location is required")]
        [ParentEntity(typeof(Location), "The specified location doesn't exist.")]
        public int LocationId { get; set; }

        #region IAuditCreatedBy Members

        public int CreatedBy { get; set; }

        #endregion

        #region IAuditCreatedOn Members

        public DateTime CreatedOn { get; set; }

        #endregion

        #region IAuditModifiedBy Members

        public int ModifiedBy { get; set; }

        #endregion

        #region IAuditModifiedOn Members

        public DateTime ModifiedOn { get; set; }

        #endregion
    }
}